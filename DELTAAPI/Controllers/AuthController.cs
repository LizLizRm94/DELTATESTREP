using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using DELTAAPI.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;


namespace DELTAAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DeltaTestContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        private static readonly HashSet<string> ExtensionesValidas = new(StringComparer.Ordinal)
        {
            "LP", "SC", "CB", "OR", "PT", "TJ", "BN", "CH", "PD"
        };

        public AuthController(DeltaTestContext context, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public class RegisterDto
        {
            public string? NombreCompleto { get; set; }
            public string? Ci { get; set; }
            public string? Expedicion { get; set; }
            public string? Correo { get; set; }
            public string? Telefono { get; set; }
            public string? FechaIngreso { get; set; } // formato ISO yyyy-MM-dd esperado desde <input type="date">
            public string? Password { get; set; }
            public string? Role { get; set; }
            public int? IdCreadoPor { get; set; }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.NombreCompleto) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Nombre completo y contraseña son obligatorios.");

            // Normalizar y validar la extensión (sigla del departamento)
            string? expedicion = null;
            if (!string.IsNullOrWhiteSpace(dto.Expedicion))
            {
                expedicion = dto.Expedicion.Trim().ToUpperInvariant();
                if (!ExtensionesValidas.Contains(expedicion))
                    return BadRequest("Extensión no válida.");
            }

            // Validar correo/ci únicos si se proporcionan
            if (!string.IsNullOrWhiteSpace(dto.Correo))
            {
                var existsCorreo = await Task.Run(() => _context.Usuarios.Any(u => u.Correo == dto.Correo));
                if (existsCorreo) return BadRequest("Correo ya registrado.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Ci))
            {
                var existsCi = await Task.Run(() => _context.Usuarios.Any(u => u.Ci == dto.Ci));
                if (existsCi) return BadRequest("CI ya registrado.");
            }

            var roleNormalized = "Usuario"; 
            if (!string.IsNullOrWhiteSpace(dto.Role) && dto.Role.Trim().Equals("Inspector", System.StringComparison.OrdinalIgnoreCase))
            {
                roleNormalized = "Inspector";
            }


            // Resolver el usuario que creó la cuenta (viene de la cookie de autenticación)
            int? idCreadoPor = null;
            var idCreadoPorClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idCreadoPorClaim, out var idCreadorClaim))
            {
                idCreadoPor = idCreadorClaim;
            }
            else if (dto.IdCreadoPor.HasValue)
            {
                // Fallback: en WebAssembly la cookie cross-origin puede no enviarse,
                // por lo que el cliente envía el id y se valida contra la base de datos.
                idCreadoPor = dto.IdCreadoPor.Value;
            }

            if (idCreadoPor.HasValue)
            {
                var existeCreador = await _context.Usuarios
                    .AsNoTracking()
                    .AnyAsync(u => u.IdUsuario == idCreadoPor.Value);

                if (!existeCreador)
                {
                    _logger.LogWarning("Register: el usuario creador {IdCreador} no existe en la base de datos", idCreadoPor);
                    idCreadoPor = null;
                }
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var usuario = new Usuario
            {
                NombreCompleto = dto.NombreCompleto!.Trim(),
                Ci = dto.Ci ?? string.Empty,
                Expedicion = expedicion,
                Correo = dto.Correo,
                Telefono = dto.Telefono,
                Contraseña = hashedPassword, 
                Rol = roleNormalized,
                Estado = "Activo",
                IdCreadoPor = idCreadoPor
            };

            // Parse FechaIngreso si viene
            if (!string.IsNullOrWhiteSpace(dto.FechaIngreso))
            {
                if (DateOnly.TryParse(dto.FechaIngreso, out var d))
                {
                    usuario.FechaIngreso = d;
                }
            }

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return StatusCode(201);
        }

        // Login DTO esperado por el cliente
        public class LoginDto
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Usuario y contraseña son obligatorios.");

            var usernameNormalized = dto.Username.Trim();
            var usernameLower = usernameNormalized.ToLowerInvariant();

            _logger.LogInformation("Login attempt for {User}", usernameNormalized);

            // Buscar usuario por correo o CI, comparando en minusculas y sin espacios
            var user = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    (!string.IsNullOrEmpty(u.Correo) && u.Correo.ToLower() == usernameLower) ||
                    (!string.IsNullOrEmpty(u.Ci) && u.Ci.ToLower() == usernameLower)
                );

            if (user == null)
            {
                _logger.LogWarning("Login failed: user not found {User}", usernameNormalized);
                return NotFound("Usuario no encontrado.");
            }

            var providedPassword = dto.Password.Trim();
            var storedPassword = (user.Contraseña ?? string.Empty).Trim();

            if (!BCrypt.Net.BCrypt.Verify(providedPassword, storedPassword))
            {
                _logger.LogWarning("Login failed: invalid credentials for {User}", usernameNormalized);
                return Unauthorized("Credenciales inválidas.");
            }

            // Crear claims para la cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, user.NombreCompleto ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Rol ?? "Usuario")
            };

            if (!string.IsNullOrWhiteSpace(user.Correo))
                claims.Add(new Claim(ClaimTypes.Email, user.Correo));

            // Crear la identidad y principal para la cookie
            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Establecer la cookie
            await HttpContext.SignInAsync("Cookies", claimsPrincipal, new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                ExpiresUtc = DateTime.UtcNow.AddHours(8),
                IsPersistent = true
            });

            var result = new
            {
                IdUsuario = user.IdUsuario,
                NombreCompleto = user.NombreCompleto,
                Rol = user.Rol,
                Mensaje = "Login exitoso"
            };

            _logger.LogInformation("Login successful for {User}", usernameNormalized);

            return Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return Ok(new { mensaje = "Logout exitoso" });
        }

        [HttpGet("current-user")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                if (!User.Identity?.IsAuthenticated ?? false)
                {
                    _logger.LogWarning("Current-user request without authentication");
                    return Unauthorized("No autorizado");
                }

                var idUsuarioStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(idUsuarioStr, out var idUsuario))
                {
                    _logger.LogWarning("Invalid user ID in claims");
                    return Unauthorized("Token inválido");
                }

                var user = await _context.Usuarios.FindAsync(idUsuario);
                if (user == null)
                {
                    _logger.LogWarning($"User {idUsuario} not found in database");
                    return NotFound("Usuario no encontrado");
                }

                _logger.LogInformation($"Current-user verified: {user.NombreCompleto}");

                return Ok(new
                {
                    idUsuario = user.IdUsuario,
                    nombreCompleto = user.NombreCompleto,
                    correo = user.Correo,
                    rol = user.Rol
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetCurrentUser: {ex.Message}");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
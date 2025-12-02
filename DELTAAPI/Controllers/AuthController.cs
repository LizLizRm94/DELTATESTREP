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
            public string? Correo { get; set; }
            public string? Telefono { get; set; }
            public string? FechaIngreso { get; set; } // formato ISO yyyy-MM-dd esperado desde <input type="date">
            public string? Password { get; set; }
            public string? Role { get; set; }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.NombreCompleto) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Nombre completo y contraseña son obligatorios.");

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


            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var usuario = new Usuario
            {
                NombreCompleto = dto.NombreCompleto!.Trim(),
                Ci = dto.Ci ?? string.Empty,
                Correo = dto.Correo,
                Telefono = dto.Telefono,
                Contraseña = hashedPassword, 
                Rol = roleNormalized,
                Estado = "Activo"
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
            if (!User.Identity?.IsAuthenticated ?? false)
                return Unauthorized("No autorizado");

            var idUsuarioStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idUsuarioStr, out var idUsuario))
                return Unauthorized("Token inválido");

            var user = await _context.Usuarios.FindAsync(idUsuario);
            if (user == null)
                return NotFound("Usuario no encontrado");

            return Ok(new
            {
                idUsuario = user.IdUsuario,
                nombreCompleto = user.NombreCompleto,
                correo = user.Correo,
                rol = user.Rol
            });
        }
    }
}
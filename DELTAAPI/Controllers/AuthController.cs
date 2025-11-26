using Microsoft.AspNetCore.Mvc;
using DELTAAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

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

            // Normalizar rol: solo se permiten 'Inspector' y 'Usuario'
            var roleNormalized = "Usuario"; // default
            if (!string.IsNullOrWhiteSpace(dto.Role) && dto.Role.Trim().Equals("Inspector", System.StringComparison.OrdinalIgnoreCase))
            {
                roleNormalized = "Inspector";
            }

            var usuario = new Usuario
            {
                NombreCompleto = dto.NombreCompleto!.Trim(),
                Ci = dto.Ci ?? string.Empty,
                Correo = dto.Correo,
                Telefono = dto.Telefono,
                Contraseña = dto.Password ?? string.Empty, // considerar hashing en producción
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

            // Comparación simple; en producción usar hashing
            var providedPassword = dto.Password.Trim();
            var storedPassword = (user.Contraseña ?? string.Empty).Trim();

            if (storedPassword != providedPassword)
            {
                _logger.LogWarning("Login failed: invalid credentials for {User}", usernameNormalized);
                return Unauthorized("Credenciales inválidas.");
            }

            // Generar token JWT
            var key = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(key))
                return StatusCode(500, "Configuración de JWT no encontrada.");

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.NombreCompleto ?? string.Empty),
                new Claim("IdUsuario", user.IdUsuario.ToString()),
                new Claim(ClaimTypes.Role, user.Rol ?? "Usuario")
            };

            if (!string.IsNullOrWhiteSpace(user.Correo))
                claims.Add(new Claim(ClaimTypes.Email, user.Correo));

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            var result = new
            {
                Token = tokenString,
                IdUsuario = user.IdUsuario,
                NombreCompleto = user.NombreCompleto,
                Rol = user.Rol
            };

            _logger.LogInformation("Login successful for {User}", usernameNormalized);

            return Ok(result);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using DELTAAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DELTAAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly DeltaTestContext _context;

        public AuthController(IConfiguration configuration, DeltaTestContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Buscar por CI o correo (versión asíncrona)
            var user = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Ci == login.Username || u.Correo == login.Username);

            if (user == null) return Unauthorized("Credenciales inválidas");

            var hasher = new PasswordHasher<Usuario>();
            var result = hasher.VerifyHashedPassword(user, user.Contraseña, login.Password);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Credenciales inválidas");

            var token = GenerateToken(user);
            return Ok(new { token, user.IdUsuario, user.NombreCompleto, user.Rol });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto register)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

                    // Verificar existencia por CI o correo (asíncrono)
            if (!string.IsNullOrWhiteSpace(register.Ci) && await _context.Usuarios.AnyAsync(u => u.Ci == register.Ci))
                return Conflict("Ya existe un usuario con ese CI");
            if (!string.IsNullOrWhiteSpace(register.Correo) && await _context.Usuarios.AnyAsync(u => u.Correo == register.Correo))
                return Conflict("Ya existe un usuario con ese correo");

            var user = new Usuario
            {
                NombreCompleto = register.NombreCompleto,
                Ci = register.Ci ?? string.Empty,
                Correo = register.Correo,
                Telefono = register.Telefono,
                Rol = register.Rol ?? "User",
                FechaIngreso = register.FechaIngreso.HasValue ? DateOnly.FromDateTime(register.FechaIngreso.Value) : null,
                Estado = register.Estado,
                PuestoActual = register.PuestoActual,
                PuestoSolicitado = register.PuestoSolicitado,
                PuestoARotar = register.PuestoARotar
            };

            var hasher = new PasswordHasher<Usuario>();
            user.Contraseña = hasher.HashPassword(user, register.Password);

            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Register), new { id = user.IdUsuario });
        }

        private string GenerateToken(Usuario user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.NombreCompleto ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.IdUsuario.ToString()),
                new Claim(ClaimTypes.Role, user.Rol ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, System.Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "ompremesalchipapaporfavorMAMAMePortareBien"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: System.DateTime.Now.AddMinutes(60),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // DTOs for authentication
    public class LoginDto
    {
        [Required(ErrorMessage = "Usuario es obligatorio")]
        public string Username { get; set; } = string.Empty; // puede ser CI o correo

        [Required(ErrorMessage = "Contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        [Required(ErrorMessage = "Nombre completo es obligatorio")]
        public string NombreCompleto { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "CI demasiado largo")]
        public string? Ci { get; set; }

        [EmailAddress(ErrorMessage = "Correo no válido")]
        public string? Correo { get; set; }

        public string? Telefono { get; set; }
        public string? Rol { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FechaIngreso { get; set; }

        public string? Estado { get; set; }
        public string? PuestoActual { get; set; }
        public string? PuestoSolicitado { get; set; }
        public string? PuestoARotar { get; set; }

        // Validación mínima para contraseña
        [Required(ErrorMessage = "Contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;
    }
}

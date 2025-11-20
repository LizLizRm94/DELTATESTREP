using Microsoft.AspNetCore.Mvc;
using DELTAAPI.Models;

namespace DELTAAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DeltaTestContext _context;

        public AuthController(DeltaTestContext context) => _context = context;

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
    }
}

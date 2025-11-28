using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DELTAAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace DELTAAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly DeltaTestContext _context;
        public UsuariosController(DeltaTestContext context) => _context = context;

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.Usuarios
                .Select(u => new {
                    u.IdUsuario,
                    u.NombreCompleto,
                    u.Correo,
                    FechaIngreso = u.FechaIngreso,
                    u.PuestoActual,
                    PuestoSolicitado = u.PuestoSolicitado,
                    Estado = u.Estado,
                    // Obtener la última evaluación práctica (TipoEvaluacion = false)
                    NotaPractica = u.EvaluacionIdEvaluadoNavigations
                        .Where(e => e.TipoEvaluacion == false)
                        .OrderByDescending(e => e.FechaEvaluacion)
                        .Select(e => e.Nota)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _context.Usuarios.FindAsync(id);
            if (user == null) return NotFound();

            DateTime? fecha = null;
            if (user.FechaIngreso.HasValue)
            {
                var d = user.FechaIngreso.Value;
                fecha = new DateTime(d.Year, d.Month, d.Day);
            }

            var result = new
            {
                user.IdUsuario,
                user.NombreCompleto,
                user.Correo,
                user.Ci,
                user.Telefono,
                FechaIngreso = fecha,
                user.PuestoActual,
                PuestoSolicitado = user.PuestoSolicitado,
                user.PuestoARotar,
                user.Estado,
                user.Rol
            };

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Usuarios.FindAsync(id);
            if (user == null) return NotFound();
            _context.Usuarios.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Accept a DTO that matches the payload sent from the Blazor client
        public class UpdateUsuarioDto
        {
            public string? NombreCompleto { get; set; }
            public string? Ci { get; set; }
            public string? Correo { get; set; }
            public string? Telefono { get; set; }
            public string? FechaIngreso { get; set; } // expected yyyy-MM-dd or null
            public string? PuestoActual { get; set; }
            public string? PuestoSolicitado { get; set; }
            public string? PuestoARotar { get; set; }
            public string? Estado { get; set; }
            public string? Rol { get; set; }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUsuarioDto dto)
        {
            if (dto == null) return BadRequest("Datos inválidos.");

            var user = await _context.Usuarios.FindAsync(id);
            if (user == null) return NotFound("Usuario no encontrado.");

            // Optional: validate uniqueness of correo/ci if they changed
            if (!string.IsNullOrWhiteSpace(dto.Correo) && dto.Correo != user.Correo)
            {
                var exists = await _context.Usuarios.AnyAsync(u => u.Correo == dto.Correo && u.IdUsuario != id);
                if (exists) return BadRequest("Correo ya registrado por otro usuario.");
                user.Correo = dto.Correo;
            }

            if (!string.IsNullOrWhiteSpace(dto.Ci) && dto.Ci != user.Ci)
            {
                var exists = await _context.Usuarios.AnyAsync(u => u.Ci == dto.Ci && u.IdUsuario != id);
                if (exists) return BadRequest("CI ya registrado por otro usuario.");
                user.Ci = dto.Ci;
            }

            // Update other fields if provided
            if (!string.IsNullOrWhiteSpace(dto.NombreCompleto)) user.NombreCompleto = dto.NombreCompleto;
            user.Telefono = dto.Telefono;
            user.PuestoActual = dto.PuestoActual;
            user.PuestoSolicitado = dto.PuestoSolicitado;
            user.PuestoARotar = dto.PuestoARotar;
            user.Estado = dto.Estado;
            if (!string.IsNullOrWhiteSpace(dto.Rol)) user.Rol = dto.Rol;

            // Parse FechaIngreso (string yyyy-MM-dd) into DateOnly
            if (!string.IsNullOrWhiteSpace(dto.FechaIngreso))
            {
                if (DateOnly.TryParse(dto.FechaIngreso, out var d))
                {
                    user.FechaIngreso = d;
                }
                else
                {
                    // try parse as DateTime then convert
                    if (DateTime.TryParse(dto.FechaIngreso, out var dt))
                    {
                        user.FechaIngreso = DateOnly.FromDateTime(dt);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}

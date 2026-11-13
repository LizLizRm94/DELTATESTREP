using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DELTAAPI.Models;
using DELTAAPI.Security;
using Microsoft.AspNetCore.Authorization;

namespace DELTAAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly DeltaTestContext _context;
        public UsuariosController(DeltaTestContext context) => _context = context;

        [HttpGet]
        [Authorize(Roles = "Administrador,Inspector")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.Usuarios
    .Include(u => u.EvaluacionIdEvaluadoNavigations)
 .AsNoTracking()
 .Select(u => new UserViewModel {
IdUsuario = u.IdUsuario,
 NombreCompleto = u.NombreCompleto,
       Correo = u.Correo,
   FechaIngreso = u.FechaIngreso,
      PuestoActual = u.PuestoActual,
       PuestoSolicitado = u.PuestoSolicitado,
   Estado = u.Estado,
        Rol = u.Rol,
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
            if (!User.CanAccessUser(id))
                return Forbid();

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
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int id)
        {
            if (User.GetUserId() == id)
                return BadRequest("No puede eliminar su propia cuenta.");

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

            var currentId = User.GetUserId();
            var isAdmin = User.IsAdministrador();
            var isInspector = User.IsInspector();
            var isSelf = currentId == id;

            if (!isAdmin && !isInspector && !isSelf)
                return Forbid();

            var validRoles = new[] { "Usuario", "Inspector", "Administrador" };
            if (!string.IsNullOrWhiteSpace(dto.Rol)
                && !validRoles.Contains(dto.Rol.Trim(), StringComparer.OrdinalIgnoreCase))
                return BadRequest("Rol no válido.");

            if (!string.IsNullOrWhiteSpace(dto.Estado)
                && !dto.Estado.Trim().Equals("Activo", StringComparison.OrdinalIgnoreCase)
                && !dto.Estado.Trim().Equals("Inactivo", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Estado no válido.");

            // Un usuario normal solo puede editar su propio perfil y no puede
            // cambiar su rol ni su estado.
            if (!isAdmin && !isInspector)
            {
                if (!string.IsNullOrWhiteSpace(dto.Rol)
                    && !dto.Rol.Trim().Equals(user.Rol, StringComparison.OrdinalIgnoreCase))
                    return Forbid();

                if (!string.IsNullOrWhiteSpace(dto.Estado)
                    && !dto.Estado.Trim().Equals(user.Estado, StringComparison.OrdinalIgnoreCase))
                    return Forbid();
            }

            // Un inspector no puede modificar administradores ni crear nuevos.
            if (isInspector && !isAdmin)
            {
                if (string.Equals(user.Rol, "Administrador", StringComparison.OrdinalIgnoreCase))
                    return Forbid();

                if (!string.IsNullOrWhiteSpace(dto.Rol)
                    && dto.Rol.Trim().Equals("Administrador", StringComparison.OrdinalIgnoreCase))
                    return Forbid();
            }

            // Evitar que un administrador se quite a sí mismo el acceso.
            if (isAdmin && isSelf)
            {
                if (!string.IsNullOrWhiteSpace(dto.Rol)
                    && !dto.Rol.Trim().Equals("Administrador", StringComparison.OrdinalIgnoreCase))
                    return BadRequest("No puede quitarse el rol de Administrador.");

                if (!string.IsNullOrWhiteSpace(dto.Estado)
                    && dto.Estado.Trim().Equals("Inactivo", StringComparison.OrdinalIgnoreCase))
                    return BadRequest("No puede desactivar su propia cuenta.");
            }

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
            if (!string.IsNullOrWhiteSpace(dto.Estado))
                user.Estado = dto.Estado.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Rol))
                user.Rol = validRoles.First(r => r.Equals(dto.Rol.Trim(), StringComparison.OrdinalIgnoreCase));

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

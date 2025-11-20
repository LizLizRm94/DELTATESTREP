using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DELTAAPI.Models;

namespace DELTAAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly DeltaTestContext _context;
        public UsuariosController(DeltaTestContext context) => _context = context;

        [HttpGet]
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
                    Estado = u.Estado
                })
                .ToListAsync();

            return Ok(list);
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
    }
}

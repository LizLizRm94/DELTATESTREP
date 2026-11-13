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
    public class NotificacionesController : ControllerBase
    {
        private readonly DeltaTestContext _context;

        public NotificacionesController(DeltaTestContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene todas las notificaciones de un usuario
        /// </summary>
        [HttpGet("usuario/{idUsuario}")]
        public async Task<IActionResult> GetNotificacionesUsuario(int idUsuario)
        {
            if (!User.CanAccessUser(idUsuario))
                return Forbid();

            var notificaciones = await _context.Notificacions
            .Where(n => n.IdUsuarioDestino == idUsuario)
            .OrderByDescending(n => n.FechaEnvio)
            .Select(n => new
            {
                n.IdNotificacion,
                n.TipoNotificacion,
                n.Mensaje,
                n.FechaEnvio
            })
            .ToListAsync();

            return Ok(notificaciones);
        }
    }
}

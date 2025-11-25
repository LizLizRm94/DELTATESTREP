using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DELTAAPI.Models;

namespace DELTAAPI.Controllers
{
    [ApiController]
    [Route("api/evaluados")]
    public class EvaluadosController : ControllerBase
    {
        private readonly DeltaTestContext _context;

        public EvaluadosController(DeltaTestContext context)
        {
            _context = context;
        }

        [HttpGet("estado/{idUsuario}")]
        public async Task<ActionResult<EvaluadoEstado>> ObtenerEstado(int idUsuario)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
            if (usuario == null) return NotFound();

            var evaluaciones = await _context.Evaluacions
                .Where(e => e.IdEvaluado == idUsuario)
                .Include(e => e.IdAreaNavigation)
                .OrderByDescending(e => e.FechaEvaluacion)
                .ToListAsync();

            var notificaciones = await _context.Notificacions
                .Where(n => n.IdUsuarioDestino == idUsuario)
                .OrderByDescending(n => n.FechaEnvio)
                .ToListAsync();

            var notas = evaluaciones.Where(e => e.Nota.HasValue).Select(e => e.Nota.Value).ToList();
            var promedio = notas.Count > 0 ? Math.Round(notas.Average(), 0) : 0;

            var estado = new EvaluadoEstado
            {
                IdUsuario = usuario.IdUsuario,
                NombreCompleto = usuario.NombreCompleto,
                PromedioGeneral = (decimal)promedio,
                ProximaRotacion = usuario.PuestoARotar ?? "Por definir...",
                Evaluaciones = evaluaciones,
                Notificaciones = notificaciones
            };

            return Ok(estado);
        }
    }
}

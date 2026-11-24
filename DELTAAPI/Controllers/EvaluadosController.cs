using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DELTAAPI.Models;
using DELTAAPI.Security;
using Microsoft.AspNetCore.Authorization;

namespace DELTAAPI.Controllers
{
    [ApiController]
    [Route("api/evaluados")]
    [Authorize]
    public class EvaluadosController : ControllerBase
    {
        private readonly DeltaTestContext _context;

        public EvaluadosController(DeltaTestContext context)
        {
            _context = context;
        }

        [HttpGet("panel/{idUsuario:int}")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Panel(int idUsuario, CancellationToken ct)
        {
            if (!User.CanAccessUser(idUsuario)) return Forbid();
            var relational = _context.Database.IsRelational();
            if (relational) await _context.Database.OpenConnectionAsync(ct);
            try
            {
                var validado = HttpContext.Items[typeof(SessionUserSnapshot)] as SessionUserSnapshot;
                var nombre = validado?.IdUsuario == idUsuario ? validado.NombreCompleto
                    : await _context.Usuarios.AsNoTracking().Where(u => u.IdUsuario == idUsuario)
                        .Select(u => u.NombreCompleto).FirstOrDefaultAsync(ct);
                if (nombre == null) return NotFound();
                var ultima = await _context.Evaluacions.AsNoTracking()
                    .Where(e => e.IdEvaluado == idUsuario && e.Nota != null)
                    .OrderByDescending(e => e.FechaEvaluacion).ThenByDescending(e => e.IdEvaluacion)
                    .Select(e => new { e.IdEvaluacion, e.FechaEvaluacion, e.Nota, TipoEvaluacion = e.TipoEvaluacion == true ? "Teórica" : "Práctica" })
                    .FirstOrDefaultAsync(ct);
                var notificaciones = await _context.Notificacions.AsNoTracking().Where(n => n.IdUsuarioDestino == idUsuario)
                    .OrderByDescending(n => n.FechaEnvio).ThenByDescending(n => n.IdNotificacion).Take(50)
                    .Select(n => new { n.TipoNotificacion, n.Mensaje, n.FechaEnvio }).ToListAsync(ct);
                return Ok(new { NombreUsuario = nombre, UltimaEvaluacion = ultima, Notificaciones = notificaciones });
            }
            finally { if (relational) await _context.Database.CloseConnectionAsync(); }
        }

        [HttpGet("estado/{idUsuario}")]
        public async Task<ActionResult<EvaluadoEstado>> ObtenerEstado(int idUsuario)
        {
            if (!User.CanAccessUser(idUsuario))
                return Forbid();

            var usuario = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
            if (usuario == null) return NotFound();

            var evaluaciones = await _context.Evaluacions
                .Where(e => e.IdEvaluado == idUsuario)
                .AsNoTracking()
                .Select(e => new Evaluacion
                {
                    IdEvaluacion = e.IdEvaluacion, IdEvaluado = e.IdEvaluado,
                    IdAdministrador = e.IdAdministrador, IdArea = e.IdArea,
                    FechaEvaluacion = e.FechaEvaluacion, Nota = e.Nota,
                    EstadoEvaluacion = e.EstadoEvaluacion, TipoEvaluacion = e.TipoEvaluacion,
                    Recomendaciones = e.Recomendaciones
                })
                .OrderByDescending(e => e.FechaEvaluacion)
                .ToListAsync();

            var notificaciones = await _context.Notificacions.AsNoTracking()
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

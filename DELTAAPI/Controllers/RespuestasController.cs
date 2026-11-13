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
    public class RespuestasController : ControllerBase
    {
        private readonly DeltaTestContext _context;

   public RespuestasController(DeltaTestContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Guardar las respuestas de un usuario en una evaluación teórica
        /// </summary>
     [HttpPost("guardar")]
   public async Task<IActionResult> GuardarRespuestas([FromBody] GuardarRespuestasRequest request)
        {
            if (request == null || request.IdEvaluacion <= 0 || request.Respuestas == null)
       {
         return BadRequest(new { mensaje = "El ID de la evaluación es requerido" });
     }

            try
          {
      // Verificar que la evaluación existe
          var evaluacion = await _context.Evaluacions.FindAsync(request.IdEvaluacion);
     if (evaluacion == null)
                {
       return NotFound(new { mensaje = "Evaluación no encontrada" });
      }

            if (evaluacion.TipoEvaluacion != true)
                return BadRequest(new { mensaje = "La evaluación no es teórica." });

            // El evaluado solo puede guardar respuestas de su propia evaluación.
            if (!User.IsStaff() && evaluacion.IdEvaluado != User.GetUserId())
                return Forbid();

            if (evaluacion.EstadoEvaluacion is "Aprobada" or "Reprobada" or "Calificado")
                return Conflict(new { mensaje = "La evaluación ya fue calificada y no admite cambios." });

            var preguntasValidas = await _context.Preguntas
                .Where(p => p.IdEvaluacion == request.IdEvaluacion)
                .Select(p => p.IdPregunta)
                .ToListAsync();

            if (request.Respuestas.Any(r => !preguntasValidas.Contains(r.IdPregunta)))
                return BadRequest(new { mensaje = "Una o más preguntas no pertenecen a la evaluación." });

             // Eliminar respuestas anteriores si existen
          var respuestasAnteriores = await _context.Respuestas
     .Where(r => r.IdEvaluacion == request.IdEvaluacion)
          .ToListAsync();

      if (respuestasAnteriores.Count > 0)
     {
          _context.Respuestas.RemoveRange(respuestasAnteriores);
       await _context.SaveChangesAsync();
       }

            // Guardar las nuevas respuestas
    if (request.Respuestas != null && request.Respuestas.Count > 0)
         {
    foreach (var respuestaDto in request.Respuestas)
  {
   var respuesta = new Respuesta
      {
    IdPregunta = respuestaDto.IdPregunta,
    IdEvaluacion = request.IdEvaluacion,
   IdUsuario = evaluacion.IdEvaluado,
  TextoRespuesta = respuestaDto.TextoRespuesta ?? string.Empty
      };

_context.Respuestas.Add(respuesta);
      }

        await _context.SaveChangesAsync();
     }

 // Actualizar estado de la evaluación a "Respondida"
        evaluacion.EstadoEvaluacion = "Respondida";
         _context.Evaluacions.Update(evaluacion);
                await _context.SaveChangesAsync();

     return Ok(new
       {
         mensaje = "Respuestas guardadas exitosamente",
   idEvaluacion = request.IdEvaluacion,
                 cantidadRespuestas = request.Respuestas?.Count ?? 0
                });
            }
catch (DbUpdateException dbEx)
      {
       var innerMessage = dbEx.InnerException?.Message ?? "Sin detalles";
      Console.WriteLine($"DbUpdateException: {dbEx.Message}");
                Console.WriteLine($"Inner Exception: {innerMessage}");
   return StatusCode(500, new
      {
    mensaje = "Error de base de datos al guardar las respuestas"
 });
            }
 catch (Exception ex)
            {
       Console.WriteLine($"Exception: {ex.Message}");
     Console.WriteLine($"StackTrace: {ex.StackTrace}");
          return StatusCode(500, new
       {
         mensaje = "Error al guardar las respuestas"
        });
     }
        }

      /// <summary>
        /// Obtener las respuestas de un usuario en una evaluación
  /// </summary>
   [HttpGet("evaluacion/{idEvaluacion}")]
        public async Task<IActionResult> GetRespuestasByEvaluacion(int idEvaluacion)
        {
  try
            {
     var evaluacion = await _context.Evaluacions
         .AsNoTracking()
         .FirstOrDefaultAsync(e => e.IdEvaluacion == idEvaluacion);

     if (evaluacion == null)
         return NotFound(new { mensaje = "Evaluación no encontrada" });

     if (!User.IsStaff() && evaluacion.IdEvaluado != User.GetUserId())
         return Forbid();

     var respuestas = await _context.Respuestas
         .Where(r => r.IdEvaluacion == idEvaluacion)
          .Select(r => new
{
    r.IdRespuesta,
        r.IdPregunta,
       r.TextoRespuesta,
      r.IdUsuario,
    r.IdEvaluacion
   })
         .ToListAsync();

    return Ok(respuestas);
            }
     catch (Exception ex)
          {
       Console.WriteLine($"Error en GetRespuestasByEvaluacion: {ex.Message}");
     return StatusCode(500, new { mensaje = "Error al obtener las respuestas" });
        }
}

     /// <summary>
     /// Obtener todas las respuestas de un usuario
        /// </summary>
        [HttpGet("usuario/{idUsuario}")]
        public async Task<IActionResult> GetRespuestasByUsuario(int idUsuario)
        {
       if (!User.CanAccessUser(idUsuario))
           return Forbid();

       try
    {
    var respuestas = await _context.Respuestas
  .Where(r => r.IdUsuario == idUsuario)
    .Select(r => new
   {
            r.IdRespuesta,
   r.IdPregunta,
        r.TextoRespuesta,
    r.IdEvaluacion
             })
      .ToListAsync();

          return Ok(respuestas);
         }
      catch (Exception ex)
       {
     Console.WriteLine($"Error en GetRespuestasByUsuario: {ex.Message}");
     return StatusCode(500, new { mensaje = "Error al obtener las respuestas del usuario" });
       }
        }

        /// <summary>
        /// Eliminar una respuesta
  /// </summary>
      [HttpDelete("{idRespuesta}")]
   [Authorize(Roles = "Administrador,Inspector")]
      public async Task<IActionResult> DeleteRespuesta(int idRespuesta)
      {
   try
            {
        var respuesta = await _context.Respuestas.FindAsync(idRespuesta);
          if (respuesta == null)
            {
  return NotFound(new { mensaje = "Respuesta no encontrada" });
  }

        _context.Respuestas.Remove(respuesta);
                await _context.SaveChangesAsync();

           return Ok(new { mensaje = "Respuesta eliminada exitosamente" });
}
      catch (Exception ex)
        {
         Console.WriteLine($"Error en DeleteRespuesta: {ex.Message}");
 return StatusCode(500, new { mensaje = "Error al eliminar la respuesta" });
     }
        }
    }
}

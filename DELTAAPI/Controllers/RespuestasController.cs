using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DELTAAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace DELTAAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        [AllowAnonymous]
   public async Task<IActionResult> GuardarRespuestas([FromBody] GuardarRespuestasRequest request)
        {
            if (request == null || request.IdEvaluacion <= 0)
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
    mensaje = "Error de base de datos al guardar las respuestas",
  error = dbEx.Message,
                  details = innerMessage
 });
            }
 catch (Exception ex)
            {
       Console.WriteLine($"Exception: {ex.Message}");
     Console.WriteLine($"StackTrace: {ex.StackTrace}");
          return StatusCode(500, new
       {
         mensaje = "Error al guardar las respuestas",
      error = ex.Message,
          innerException = ex.InnerException?.Message
        });
     }
        }

      /// <summary>
        /// Obtener las respuestas de un usuario en una evaluación
  /// </summary>
   [HttpGet("evaluacion/{idEvaluacion}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRespuestasByEvaluacion(int idEvaluacion)
        {
  try
            {
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
     return StatusCode(500, new { mensaje = "Error al obtener las respuestas", error = ex.Message });
        }
}

     /// <summary>
     /// Obtener todas las respuestas de un usuario
        /// </summary>
        [HttpGet("usuario/{idUsuario}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRespuestasByUsuario(int idUsuario)
        {
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
     return StatusCode(500, new { mensaje = "Error al obtener las respuestas del usuario", error = ex.Message });
       }
        }

        /// <summary>
        /// Eliminar una respuesta
  /// </summary>
      [HttpDelete("{idRespuesta}")]
   [AllowAnonymous]
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
 return StatusCode(500, new { mensaje = "Error al eliminar la respuesta", error = ex.Message });
     }
        }
    }
}

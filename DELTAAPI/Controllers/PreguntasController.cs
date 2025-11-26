using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DELTAAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace DELTAAPI.Controllers
{
    [ApiController]
[Route("api/[controller]")]
 public class PreguntasController : ControllerBase
    {
        private readonly DeltaTestContext _context;

        public PreguntasController(DeltaTestContext context)
        {
         _context = context;
}

   /// <summary>
  /// Guarda un lote de preguntas de evaluación teórica en la base de datos
    /// </summary>
        [HttpPost("crear-evaluacion-teorica")]
        [AllowAnonymous]
    public async Task<IActionResult> CrearEvaluacionTeorica([FromBody] CrearPreguntasRequest request)
        {
            if (request == null || request.Preguntas == null || request.Preguntas.Count == 0)
            {
    return BadRequest(new { mensaje = "La lista de preguntas no puede estar vacía" });
            }

  try
       {
      // Filtrar preguntas que tengan texto (no guardar preguntas vacías)
        var preguntasValidas = request.Preguntas
      .Where(p => !string.IsNullOrWhiteSpace(p.Texto))
   .ToList();

         if (preguntasValidas.Count == 0)
    {
        return BadRequest(new { mensaje = "Debe ingresar al menos una pregunta con texto" });
     }

      // OPCIÓN: Guardar solo las preguntas sin crear una evaluación
      // Las preguntas se asociarán con una evaluación cuando se asigne un evaluado
      foreach (var preguntaDto in preguntasValidas)
      {
         var pregunta = new Pregunta
        {
 Texto = preguntaDto.Texto,
       TipoEvaluacion = true
    };

_context.Preguntas.Add(pregunta);
      }

await _context.SaveChangesAsync();

return Ok(new
    {
     mensaje = "Preguntas de evaluación teórica guardadas exitosamente",
        cantidadPreguntas = preguntasValidas.Count
     });
       }
   catch (DbUpdateException dbEx)
            {
 var innerMessage = dbEx.InnerException?.Message ?? "Sin detalles";
       return StatusCode(500, new
          {
   mensaje = "Error de base de datos al guardar",
     error = dbEx.Message,
               details = innerMessage
      });
       }
            catch (Exception ex)
      {
         return StatusCode(500, new
  {
                  mensaje = "Error al guardar la evaluación",
       error = ex.Message,
     innerException = ex.InnerException?.Message,
            stackTrace = ex.StackTrace
      });
      }
     }

    /// <summary>
        /// Obtiene todas las preguntas de tipo teórico
        /// </summary>
        [HttpGet("teoricas")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPreguntasTeorica()
        {
     try
            {
      var preguntas = await _context.Preguntas
         .Where(p => p.TipoEvaluacion == true)
         .ToListAsync();

      return Ok(preguntas);
    }
            catch (Exception ex)
    {
     return StatusCode(500, new { mensaje = "Error al obtener las preguntas", error = ex.Message });
     }
        }

  /// <summary>
        /// Obtiene una pregunta específica por ID
        /// </summary>
     [HttpGet("{id}")]
      [AllowAnonymous]
   public async Task<IActionResult> GetPreguntaById(int id)
        {
 try
          {
        var pregunta = await _context.Preguntas.FindAsync(id);

       if (pregunta == null)
     return NotFound(new { mensaje = "Pregunta no encontrada" });

    return Ok(pregunta);
      }
     catch (Exception ex)
{
   return StatusCode(500, new { mensaje = "Error al obtener la pregunta", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza una pregunta existente
        /// </summary>
        [HttpPut("{id}")]
      [AllowAnonymous]
     public async Task<IActionResult> UpdatePregunta(int id, [FromBody] PreguntaDto preguntaDto)
        {
     if (string.IsNullOrWhiteSpace(preguntaDto.Texto))
    return BadRequest(new { mensaje = "El texto de la pregunta no puede estar vacío" });

            try
     {
          var pregunta = await _context.Preguntas.FindAsync(id);

        if (pregunta == null)
  return NotFound(new { mensaje = "Pregunta no encontrada" });

  pregunta.Texto = preguntaDto.Texto;
    pregunta.TipoEvaluacion = preguntaDto.TipoEvaluacion;

            _context.Preguntas.Update(pregunta);
     await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Pregunta actualizada exitosamente" });
     }
    catch (Exception ex)
          {
    return StatusCode(500, new { mensaje = "Error al actualizar la pregunta", error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina una pregunta
 /// </summary>
      [HttpDelete("{id}")]
        [AllowAnonymous]
  public async Task<IActionResult> DeletePregunta(int id)
   {
      try
          {
  var pregunta = await _context.Preguntas.FindAsync(id);

          if (pregunta == null)
      return NotFound(new { mensaje = "Pregunta no encontrada" });

     _context.Preguntas.Remove(pregunta);
        await _context.SaveChangesAsync();

           return Ok(new { mensaje = "Pregunta eliminada exitosamente" });
}
      catch (Exception ex)
     {
     return StatusCode(500, new { mensaje = "Error al eliminar la pregunta", error = ex.Message });
       }
        }
    }
}

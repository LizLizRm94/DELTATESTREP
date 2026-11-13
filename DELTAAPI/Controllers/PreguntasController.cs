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
        [Authorize(Roles = "Administrador,Inspector")]
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
            catch (DbUpdateException)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error de base de datos al guardar"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al guardar la evaluación"
                });
            }
        }

        /// <summary>
        /// Obtiene todas las preguntas de tipo teórico
        /// </summary>
        [HttpGet("teoricas")]
        [Authorize(Roles = "Administrador,Inspector")]
        public async Task<IActionResult> GetPreguntasTeorica()
        {
            try
            {
                var preguntas = await _context.Preguntas
                   .Where(p => p.TipoEvaluacion == true)
                   .ToListAsync();

                return Ok(preguntas);
            }
            catch (Exception)
            {
                return StatusCode(500, new { mensaje = "Error al obtener las preguntas" });
            }
        }

        /// <summary>
        /// Obtiene SOLO las preguntas asignadas a una evaluación teórica específica
        /// por medio de las respuestas registradas, o las preguntas más recientes si no hay respuestas aún
        /// </summary>
        [HttpGet("evaluacion/{idEvaluacion}")]
        public async Task<IActionResult> GetPreguntasPorEvaluacion(int idEvaluacion)
        {
            try
            {
                var evaluacion = await _context.Evaluacions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.IdEvaluacion == idEvaluacion);

                if (evaluacion == null)
                    return NotFound(new { mensaje = "Evaluación no encontrada" });

                // Un usuario solo puede consultar las preguntas de su propia evaluación.
                if (!User.IsStaff() && evaluacion.IdEvaluado != User.GetUserId())
                    return Forbid();

                // Obtener preguntas directamente asociadas a esta evaluación
                var preguntas = await _context.Preguntas
                .Where(p => p.IdEvaluacion == idEvaluacion)
                .OrderBy(p => p.IdPregunta)
                .Select(p => new
                {
                    p.IdPregunta,
                    p.Texto,
                    p.TipoEvaluacion,
                    p.Opciones,
                    p.RespuestaCorrectaIndex,
                    p.Puntos
                })
                 .ToListAsync();

                if (preguntas.Count == 0)
                {
                    return NotFound(new { mensaje = "No hay preguntas para esta evaluación" });
                }

                // Nunca exponer la respuesta correcta al evaluado.
                if (User.IsStaff())
                    return Ok(preguntas);

                return Ok(preguntas.Select(p => new
                {
                    p.IdPregunta,
                    p.Texto,
                    p.TipoEvaluacion,
                    p.Opciones,
                    p.Puntos
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetPreguntasPorEvaluacion: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error al obtener las preguntas de la evaluación" });
            }
        }

        /// <summary>
        /// Obtiene una pregunta específica por ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Inspector")]
        public async Task<IActionResult> GetPreguntaById(int id)
        {
            try
            {
                var pregunta = await _context.Preguntas.FindAsync(id);

                if (pregunta == null)
                    return NotFound(new { mensaje = "Pregunta no encontrada" });

                return Ok(pregunta);
            }
            catch (Exception)
            {
                return StatusCode(500, new { mensaje = "Error al obtener la pregunta" });
            }
        }

        /// <summary>
        /// Actualiza una pregunta existente
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Inspector")]
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
            catch (Exception)
            {
                return StatusCode(500, new { mensaje = "Error al actualizar la pregunta" });
            }
        }

        /// <summary>
        /// Elimina una pregunta
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador,Inspector")]
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
            catch (Exception)
            {
                return StatusCode(500, new { mensaje = "Error al eliminar la pregunta" });
            }
        }
    }
}

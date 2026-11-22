using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DELTAAPI.Models;
using DELTAAPI.Security;
using Microsoft.AspNetCore.Authorization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DELTAAPI.Controllers
{
    [ApiController]
    [Route("api/evaluacionesteoricass")]
    [Authorize]
    public class EvaluacionesTeoricasController : ControllerBase
    {
        private readonly DeltaTestContext _context;
        private readonly ILogger<EvaluacionesTeoricasController> _logger;

        public EvaluacionesTeoricasController(DeltaTestContext context, ILogger<EvaluacionesTeoricasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Guarda las respuestas del usuario para una evaluación teórica
        /// </summary>
        [HttpPost("guardar-respuestas")]
        public async Task<IActionResult> GuardarRespuestasTeoricas([FromBody] GuardarRespuestasRequest request)
        {
            if (request == null || request.IdUsuario <= 0 || request.IdEvaluacion <= 0
                || request.Respuestas == null || request.Respuestas.Count == 0)
            {
                return BadRequest(new { mensaje = "Datos inválidos" });
            }

            try
            {
                // Verificar que el usuario existe
                var usuario = await _context.Usuarios.FindAsync(request.IdUsuario);
                if (usuario == null)
                    return NotFound(new { mensaje = "Usuario no encontrado" });

                // Verificar que la evaluación existe
                var evaluacion = await _context.Evaluacions.FindAsync(request.IdEvaluacion);
                if (evaluacion == null)
                    return NotFound(new { mensaje = "Evaluación no encontrada" });

                if (evaluacion.TipoEvaluacion != true)
                    return BadRequest(new { mensaje = "La evaluación no es teórica." });

                if (evaluacion.IdEvaluado != request.IdUsuario)
                    return BadRequest(new { mensaje = "La evaluación no pertenece al usuario indicado." });

                if (!User.IsStaff() && User.GetUserId() != request.IdUsuario)
                    return Forbid();

                if (evaluacion.EstadoEvaluacion is "Aprobada" or "Reprobada" or "Calificado")
                    return Conflict(new { mensaje = "La evaluación ya fue calificada y no admite cambios." });

                if (request.Respuestas.Select(r => r.IdPregunta).Distinct().Count() != request.Respuestas.Count
                    || request.Respuestas.Any(r => string.IsNullOrWhiteSpace(r.TextoRespuesta)))
                    return BadRequest(new { mensaje = "Responda cada pregunta una sola vez y sin respuestas vacías." });

                // Solo se aceptan preguntas que pertenezcan a esta evaluación.
                var preguntasValidas = await _context.Preguntas
                    .Where(p => p.IdEvaluacion == request.IdEvaluacion)
                    .Select(p => p.IdPregunta)
                    .ToListAsync();

                var preguntasSolicitadas = request.Respuestas.Select(r => r.IdPregunta).Distinct().ToList();
                if (preguntasSolicitadas.Any(id => !preguntasValidas.Contains(id)))
                    return BadRequest(new { mensaje = "Una o más preguntas no pertenecen a la evaluación." });

                // Reemplazar respuestas anteriores para evitar duplicados.
                var anteriores = await _context.Respuestas
                    .Where(r => r.IdEvaluacion == request.IdEvaluacion && r.IdUsuario == request.IdUsuario)
                    .ToListAsync();

                if (anteriores.Count > 0)
                    _context.Respuestas.RemoveRange(anteriores);

                // Guardar las respuestas
                foreach (var respuestaDto in request.Respuestas)
                {
                    var respuesta = new Respuesta
                    {
                        IdUsuario = request.IdUsuario,
                        IdPregunta = respuestaDto.IdPregunta,
                        IdEvaluacion = request.IdEvaluacion,
                        TextoRespuesta = respuestaDto.TextoRespuesta ?? string.Empty
                    };

                    _context.Respuestas.Add(respuesta);
                }

                // Actualizar estado de la evaluación a "Respondida" SIN guardar nota
                // La nota solo se guardará cuando el administrador la califique
                evaluacion.EstadoEvaluacion = "Respondida";

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    mensaje = "Respuestas guardadas exitosamente. Pendiente de calificación.",
                    cantidadRespuestas = request.Respuestas.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al guardar respuestas: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error al guardar las respuestas" });
            }
        }

        /// <summary>
        /// Obtiene las respuestas de un usuario para una evaluación teórica específica
        /// </summary>
        [HttpGet("respuestas/{idUsuario}/{idEvaluacion}")]
        public async Task<IActionResult> GetRespuestasUsuario(int idUsuario, int idEvaluacion)
        {
            if (!User.CanAccessUser(idUsuario))
                return Forbid();

            try
            {
                var respuestas = await _context.Respuestas
                .Where(r => r.IdUsuario == idUsuario && r.IdEvaluacion == idEvaluacion)
                .Include(r => r.IdPreguntaNavigation)
                .OrderBy(r => r.IdPregunta)
                .ToListAsync();

                if (!respuestas.Any())
                {
                    _logger.LogWarning($"No se encontraron respuestas para usuario {idUsuario} en evaluación {idEvaluacion}");
                    return NotFound(new { mensaje = "No se encontraron respuestas" });
                }

                var resultado = respuestas.Select(r => new
                {
                    idRespuesta = r.IdRespuesta,
                    idPregunta = r.IdPregunta,
                    pregunta = r.IdPreguntaNavigation?.Texto ?? "",
                    textoRespuesta = r.TextoRespuesta ?? ""
                }).ToList();

                _logger.LogInformation($"Se encontraron {resultado.Count} respuestas para usuario {idUsuario}");
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener respuestas: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error al obtener las respuestas" });
            }
        }

        /// <summary>
        /// Genera un PDF con las preguntas y respuestas de la evaluación teórica
        /// </summary>
        [HttpGet("descargar-pdf/{idUsuario}/{idEvaluacion}")]
        public async Task<IActionResult> DescargarPDF(int idUsuario, int idEvaluacion)
        {
            if (!User.CanAccessUser(idUsuario))
                return Forbid();

            try
            {
                // Obtener usuario
                var usuario = await _context.Usuarios.FindAsync(idUsuario);
                if (usuario == null)
                    return NotFound(new { mensaje = "Usuario no encontrado" });

                // Obtener evaluación
                var evaluacion = await _context.Evaluacions.FindAsync(idEvaluacion);
                if (evaluacion == null)
                    return NotFound(new { mensaje = "Evaluación no encontrada" });

                if (evaluacion.IdEvaluado != idUsuario || evaluacion.TipoEvaluacion != true)
                    return BadRequest(new { mensaje = "La evaluación no corresponde al usuario indicado." });

                // Obtener respuestas con preguntas
                var respuestas = await _context.Respuestas
                      .Where(r => r.IdUsuario == idUsuario && r.IdEvaluacion == idEvaluacion)
                         .Include(r => r.IdPreguntaNavigation)
                        .OrderBy(r => r.IdPregunta)
                   .ToListAsync();

                if (!respuestas.Any())
                    return NotFound(new { mensaje = "No hay respuestas para generar el PDF" });

                var pdf = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(25);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header().Column(header =>
                        {
                            header.Item().AlignCenter().Text("EVALUACIÓN TEÓRICA").FontSize(18).Bold();
                            header.Item().PaddingTop(8).Text($"Evaluado: {usuario.NombreCompleto}");
                            header.Item().Text($"Correo: {usuario.Correo}");
                            header.Item().Text($"Fecha de Evaluación: {evaluacion.FechaEvaluacion?.ToString("dd/MM/yyyy")}");
                            if (evaluacion.Nota.HasValue)
                                header.Item().Text($"Calificación: {evaluacion.Nota.Value} / 100");
                            header.Item().PaddingBottom(12);
                        });

                        page.Content().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background("#F58220").Padding(6)
                                    .Text("PREGUNTA").Bold().FontColor(Colors.White);
                                header.Cell().Background("#F58220").Padding(6)
                                    .Text("RESPUESTA").Bold().FontColor(Colors.White);
                            });

                            foreach (var respuesta in respuestas)
                            {
                                table.Cell().Border(0.5f).Padding(6)
                                    .Text(respuesta.IdPreguntaNavigation?.Texto ?? string.Empty);
                                table.Cell().Border(0.5f).Padding(6)
                                    .Text(respuesta.TextoRespuesta ?? string.Empty);
                            }
                        });

                        page.Footer().AlignCenter().Text(text =>
                        {
                            text.Span("Página ");
                            text.CurrentPageNumber();
                            text.Span(" de ");
                            text.TotalPages();
                        });
                    });
                });

                var pdfBytes = pdf.GeneratePdf();
                var invalid = Path.GetInvalidFileNameChars();
                var safeName = string.Concat((usuario.NombreCompleto ?? "usuario")
                    .Select(c => invalid.Contains(c) ? '_' : c));

                return File(pdfBytes, "application/pdf", $"Evaluacion_Teorica_{safeName}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar el PDF.");
                return StatusCode(500, new { mensaje = "Error al generar el PDF" });
            }
        }

        /// <summary>
        /// Obtiene todas las evaluaciones teóricas de un usuario
        /// </summary>
        [HttpGet("usuario/{idUsuario}")]
        public async Task<IActionResult> GetEvaluacionesTeoricasUsuario(int idUsuario)
        {
            if (!User.CanAccessUser(idUsuario))
                return Forbid();

            try
            {
                var evaluaciones = await _context.Evaluacions
            .Where(e => e.IdEvaluado == idUsuario && e.TipoEvaluacion == true)
               .Select(e => new
               {
                   e.IdEvaluacion,
                   e.FechaEvaluacion,
                   e.Nota,
                   e.EstadoEvaluacion,
                   cantidadPreguntas = e.Respuestas.Count
               })
          .ToListAsync();

                return Ok(evaluaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener evaluaciones: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error al obtener las evaluaciones" });
            }
        }

        /// <summary>
        /// Obtiene todas las evaluaciones teóricas del sistema
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrador,Inspector")]
        public async Task<IActionResult> GetTodasEvaluacionesTeoricass()
        {
            try
            {
                var evaluaciones = await _context.Evaluacions
                        .Where(e => e.TipoEvaluacion == true)
               .Include(e => e.IdEvaluadoNavigation)
                   .Include(e => e.Respuestas)
                .Select(e => new
                {
                    idEvaluacion = e.IdEvaluacion,
                    idEvaluado = e.IdEvaluado,
                    nombreEvaluado = e.IdEvaluadoNavigation.NombreCompleto,
                    fechaEvaluacion = e.FechaEvaluacion,
                    nota = e.Nota,
                    estadoEvaluacion = e.EstadoEvaluacion,
                    cantidadPreguntas = e.Respuestas.Count
                })
                    .ToListAsync();

                _logger.LogInformation($"GetTodasEvaluacionesTeoricass: Total={evaluaciones.Count}");
                foreach (var e in evaluaciones)
                {
                    _logger.LogInformation($"  - ID={e.idEvaluacion}, Estado={e.estadoEvaluacion}, Nota={e.nota}");
                }

                return Ok(evaluaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener evaluaciones: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error al obtener las evaluaciones" });
            }
        }

        /// <summary>
        /// Obtiene todas las evaluaciones teóricas del sistema (SIN FILTROS - para depuración)
        /// </summary>
        [HttpGet("debug/todas")]
        [Authorize(Roles = "Administrador,Inspector")]
        public async Task<IActionResult> GetTodasEvaluacionesDebug()
        {
            try
            {
                var evaluaciones = await _context.Evaluacions
                   .Where(e => e.TipoEvaluacion == true)
                       .Include(e => e.IdEvaluadoNavigation)
                .Select(e => new
                {
                    e.IdEvaluacion,
                    e.IdEvaluado,
                    NombreEvaluado = e.IdEvaluadoNavigation.NombreCompleto,
                    e.FechaEvaluacion,
                    e.Nota,
                    e.EstadoEvaluacion,
                    cantidadPreguntas = e.Respuestas.Count,
                    tieneRespuestas = e.Respuestas.Count > 0
                })
               .ToListAsync();

                _logger.LogInformation($"DEBUG: Total evaluaciones teóricas: {evaluaciones.Count}");
                foreach (var e in evaluaciones)
                {
                    _logger.LogInformation($"DEBUG: ID={e.IdEvaluacion}, Estado={e.EstadoEvaluacion}, Nota={e.Nota}, Respuestas={e.cantidadPreguntas}");
                }

                return Ok(evaluaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener evaluaciones debug: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error al obtener las evaluaciones" });
            }
        }

        /// <summary>
        /// Obtiene evaluaciones teóricas RESPONDIDAS (simpler version)
        /// </summary>
        [HttpGet("pendientes")]
        [Authorize(Roles = "Administrador,Inspector")]
        public async Task<IActionResult> GetEvaluacionesPendientesCalificacion()
        {
            try
            {
                var evaluaciones = await _context.Evaluacions
                 .Where(e => e.TipoEvaluacion == true)
              .Include(e => e.IdEvaluadoNavigation)
                    .Include(e => e.Respuestas)
               .Select(e => new
               {
                   idEvaluacion = e.IdEvaluacion,
                   idEvaluado = e.IdEvaluado,
                   nombreEvaluado = e.IdEvaluadoNavigation.NombreCompleto,
                   fechaEvaluacion = e.FechaEvaluacion,
                   nota = e.Nota,
                   estadoEvaluacion = e.EstadoEvaluacion,
                   cantidadPreguntas = e.Respuestas.Count,
                   tieneRespuestas = e.Respuestas.Count > 0
               })
                     .OrderByDescending(e => e.fechaEvaluacion)
                   .ToListAsync();

                _logger.LogInformation($"GetEvaluacionesPendientes: Total evaluaciones={evaluaciones.Count}");
                foreach (var e in evaluaciones)
                {
                    _logger.LogInformation($"  ID={e.idEvaluacion}, Estado='{e.estadoEvaluacion}', Respuestas={e.cantidadPreguntas}");
                }

                return Ok(evaluaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener evaluaciones: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error" });
            }
        }

        /// <summary>
        /// Obtiene preguntas con respuestas para calificación (para admin)
        /// </summary>
        [HttpGet("calificar/{idEvaluacion}")]
        [Authorize(Roles = "Administrador,Inspector")]
        public async Task<IActionResult> GetPreguntasParaCalificar(int idEvaluacion)
        {
            try
            {
                _logger.LogInformation($"=== GetPreguntasParaCalificar - Evaluación ID: {idEvaluacion} ===");

                // Obtener la evaluación con todas sus relaciones
                var evaluacion = await _context.Evaluacions
                  .Include(e => e.IdEvaluadoNavigation)
                  .Include(e => e.Respuestas)
                     .FirstOrDefaultAsync(e => e.IdEvaluacion == idEvaluacion);

                if (evaluacion == null)
                {
                    _logger.LogWarning($"Evaluación no encontrada: {idEvaluacion}");
                    return NotFound(new { mensaje = "Evaluación no encontrada" });
                }

                // Obtener preguntas por separado
                var preguntas = await _context.Preguntas
                 .Where(p => p.IdEvaluacion == idEvaluacion)
               .OrderBy(p => p.IdPregunta)
                 .ToListAsync();

                _logger.LogInformation($"Evaluación encontrada. Preguntas: {preguntas.Count}, Respuestas: {evaluacion.Respuestas.Count}");

                // Obtener preguntas con sus respuestas
                var preguntasConRespuestas = new List<object>();

                foreach (var pregunta in preguntas)
                {
                    var respuesta = evaluacion.Respuestas.FirstOrDefault(r => r.IdPregunta == pregunta.IdPregunta);
                    var respuestaTexto = respuesta?.TextoRespuesta ?? "Sin respuesta";



                    var preguntaObj = new
                    {
                        idPregunta = pregunta.IdPregunta,
                        pregunta = pregunta.Texto,
                        tipoEvaluacion = pregunta.TipoEvaluacion,
                        opciones = pregunta.Opciones,
                        respuestaCorrectaIndex = pregunta.RespuestaCorrectaIndex,
                        respuesta = respuestaTexto,
                        puntos = pregunta.Puntos,
                        puntosObtenidos = pregunta.PuntosObtenidos
                    };

                    preguntasConRespuestas.Add(preguntaObj);
                }

                _logger.LogInformation($"Preguntas con respuestas procesadas: {preguntasConRespuestas.Count}");

                var resultado = new
                {
                    idEvaluacion = evaluacion.IdEvaluacion,
                    idEvaluado = evaluacion.IdEvaluado,
                    nombreEvaluado = evaluacion.IdEvaluadoNavigation?.NombreCompleto ?? "Desconocido",
                    fechaEvaluacion = evaluacion.FechaEvaluacion,
                    estadoActual = evaluacion.EstadoEvaluacion,
                    notaActual = evaluacion.Nota,
                    recomendacionesActuales = evaluacion.Recomendaciones,
                    preguntas = preguntasConRespuestas
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en GetPreguntasParaCalificar: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new
                {
                    mensaje = "Error al obtener preguntas y respuestas"
                });
            }
        }

        [HttpPut("calificar/{idEvaluacion}")]
        [Authorize(Roles = "Administrador,Inspector")]
        public async Task<IActionResult> CalificarEvaluacion(int idEvaluacion, [FromBody] CalificarEvaluacionRequest request)
        {
        try
        {
            if (request == null || request.Nota < 0 || request.Nota > 100)
            {
                return BadRequest(new { mensaje = "La calificación debe estar entre 0 y 100" });
            }

            var evaluacion = await _context.Evaluacions.FindAsync(idEvaluacion);
            if (evaluacion == null)
            {
                return NotFound(new { mensaje = "Evaluación no encontrada" });
            }

            if (evaluacion.TipoEvaluacion != true)
                return BadRequest(new { mensaje = "La evaluación no es teórica." });

            // Verificar que las preguntas enviadas pertenecen a esta evaluación.
            if (request.PreguntasConPuntos != null && request.PreguntasConPuntos.Count > 0)
            {
                if (request.PreguntasConPuntos.Any(p => p.Puntos < 0))
                    return BadRequest(new { mensaje = "Los puntos no pueden ser negativos." });

                var idsValidos = await _context.Preguntas
                    .Where(p => p.IdEvaluacion == idEvaluacion)
                    .Select(p => p.IdPregunta)
                    .ToListAsync();

                if (request.PreguntasConPuntos.Any(p => !idsValidos.Contains(p.IdPregunta)))
                    return BadRequest(new { mensaje = "Una o más preguntas no pertenecen a la evaluación." });

                if (request.PreguntasConPuntos.Select(p => p.IdPregunta).Distinct().Count() != request.PreguntasConPuntos.Count)
                    return BadRequest(new { mensaje = "No repita preguntas al calificar." });

                if (request.PreguntasConPuntos.Count != idsValidos.Count)
                    return BadRequest(new { mensaje = "Al calificar por pregunta, incluya todas las preguntas." });
                var maximo = await _context.Preguntas.Where(p => p.IdEvaluacion == idEvaluacion).SumAsync(p => p.Puntos);
                if (maximo <= 0)
                    return BadRequest(new { mensaje = "Configure puntajes máximos positivos antes de calificar por pregunta." });
                var calculada = Math.Round((decimal)request.PreguntasConPuntos.Sum(p => (long)p.Puntos) * 100 / maximo, 2);
                if (request.Nota != calculada)
                    return BadRequest(new { mensaje = "La nota total no coincide con los puntos obtenidos." });

                foreach (var preguntaRequest in request.PreguntasConPuntos)
                {
                    var pregunta = await _context.Preguntas.FindAsync(preguntaRequest.IdPregunta);
                    if (pregunta != null)
                    {
                        if (preguntaRequest.Puntos > pregunta.Puntos)
                            return BadRequest(new { mensaje = "El puntaje obtenido no puede superar el máximo de la pregunta." });
                        pregunta.PuntosObtenidos = preguntaRequest.Puntos;
                        _context.Preguntas.Update(pregunta);
                    }
                }
            }

            evaluacion.Nota = request.Nota;
            evaluacion.Recomendaciones = request.Recomendaciones ?? string.Empty;
            evaluacion.EstadoEvaluacion = request.Nota >= 80 ? "Aprobada" : "Reprobada";

                    _context.Evaluacions.Update(evaluacion);
                    await _context.SaveChangesAsync();

           return Ok(new
           {
              mensaje = "Evaluación calificada exitosamente",
              idEvaluacion = evaluacion.IdEvaluacion,
              nota = evaluacion.Nota,
              estado = evaluacion.EstadoEvaluacion
           });
        }
        catch (Exception ex)
        {
             _logger.LogError($"Error en CalificarEvaluacion: {ex.Message}");
             return StatusCode(500, new { mensaje = "Error al calificar la evaluación" });
            }
        }
    }

    public class GuardarRespuestasRequest
    {
        public int IdUsuario { get; set; }
        public int IdEvaluacion { get; set; }
        public List<RespuestaDto> Respuestas { get; set; } = new();
    }

    public class RespuestaDto
    {
        public int IdPregunta { get; set; }
        public string TextoRespuesta { get; set; } = string.Empty;
    }

    public class CalificarEvaluacionRequest
    {
       public decimal Nota { get; set; }
       public string? Recomendaciones { get; set; }
       public List<PreguntaPuntosRequest>? PreguntasConPuntos { get; set; }
    }

    public class PreguntaPuntosRequest
    {
        public int IdPregunta { get; set; }
        public int Puntos { get; set; }
    }
}

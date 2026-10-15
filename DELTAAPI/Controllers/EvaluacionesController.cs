using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DELTAAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Text.Json;

namespace DELTAAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EvaluacionesController : ControllerBase
    {
        private readonly DeltaTestContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EvaluacionesController> _logger;

        public EvaluacionesController(
            DeltaTestContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<EvaluacionesController> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("crear-evaluacion-practica")]
        [AllowAnonymous]
        public async Task<IActionResult> CrearEvaluacionPractica([FromBody] CrearEvaluacionPracticaRequest request)
        {
            if (request == null || request.IdUsuario <= 0)
        {
            return BadRequest(new { mensaje = "El ID del usuario es requerido" });
        }

        try
        {
            var usuario = await _context.Usuarios.FindAsync(request.IdUsuario);
            if (usuario == null)
            {
                return NotFound(new { mensaje = "Usuario no encontrado" });
            }

            var evaluacion = new Evaluacion
            {
                IdEvaluado = request.IdUsuario,
                FechaEvaluacion = DateOnly.FromDateTime(DateTime.Now),
                TipoEvaluacion = false,
                EstadoEvaluacion = "Completada",
                Nota = CalcularCalificacion(request.Tareas),
                Recomendaciones = request.Recomendaciones
            };

        _context.Evaluacions.Add(evaluacion);
await _context.SaveChangesAsync();

  return Ok(new
       {
         mensaje = "Evaluación práctica guardada exitosamente",
       idEvaluacion = evaluacion.IdEvaluacion,
      cantidadTareas = request.Tareas?.Count ?? 0,
             calificacion = evaluacion.Nota
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
    innerException = ex.InnerException?.Message
        });
         }
        }

   [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetEvaluaciones()
        {
            try
            {
                var evaluaciones = await _context.Evaluacions
                    .Include(e => e.IdEvaluadoNavigation)
                    .Select(e => new
                    {
                        e.IdEvaluacion,
                        e.IdEvaluado,
                        NombreEvaluado = e.IdEvaluadoNavigation != null ? e.IdEvaluadoNavigation.NombreCompleto : "Sin evaluado",
                        e.FechaEvaluacion,
                        e.Nota,
                        e.EstadoEvaluacion,
                        TipoEvaluacion = e.TipoEvaluacion == true ? "Teórica" : "Práctica"
                    })
                    .ToListAsync();

                return Ok(evaluaciones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener las evaluaciones", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEvaluacionById(int id)
        {
            try
            {
                var evaluacion = await _context.Evaluacions
                    .Include(e => e.IdEvaluadoNavigation)
                    .Include(e => e.IdAdministradorNavigation)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.IdEvaluacion == id);

                if (evaluacion == null)
                    return NotFound(new { mensaje = "Evaluación no encontrada" });

                // Validar que el usuario evaluado existe
                if (evaluacion.IdEvaluadoNavigation == null)
                    return StatusCode(500, new { mensaje = "Error: Usuario evaluado no encontrado en la base de datos" });

                return Ok(new
                {
                    evaluacion.IdEvaluacion,
                    evaluacion.IdEvaluado,
                    NombreEvaluado = evaluacion.IdEvaluadoNavigation?.NombreCompleto ?? "Sin nombre",
                    CiEvaluado = evaluacion.IdEvaluadoNavigation?.Ci ?? "Sin CI",
                    NombreAdministrador = evaluacion.IdAdministradorNavigation?.NombreCompleto ?? "N/A",
                    evaluacion.FechaEvaluacion,
                    evaluacion.Nota,
                    evaluacion.EstadoEvaluacion,
                    TipoEvaluacion = evaluacion.TipoEvaluacion == true ? "Teórica" : "Práctica",
                    Recomendaciones = evaluacion.Recomendaciones ?? string.Empty
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetEvaluacionById: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    mensaje = "Error al obtener la evaluación",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("usuario/{idUsuario}")]
   [AllowAnonymous]
        public async Task<IActionResult> GetEvaluacionesByUsuario(int idUsuario)
        {
      try
{
       var evaluaciones = await _context.Evaluacions
          .Where(e => e.IdEvaluado == idUsuario)
           .Select(e => new
     {
               e.IdEvaluacion,
       e.FechaEvaluacion,
          e.Nota,
             e.EstadoEvaluacion,
                TipoEvaluacion = e.TipoEvaluacion == true ? "Teórica" : "Práctica"
       })
         .ToListAsync();

         return Ok(evaluaciones);
       }
catch (Exception ex)
  {
 return StatusCode(500, new { mensaje = "Error al obtener las evaluaciones", error = ex.Message });
        }
        }

    [HttpGet("ultima-practica/{idUsuario}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUltimaPracticaByUsuario(int idUsuario)
        {
   try
            {
       var ultimaEvaluacion = await _context.Evaluacions
           .Where(e => e.IdEvaluado == idUsuario && e.TipoEvaluacion == false)
     .OrderByDescending(e => e.FechaEvaluacion)
          .FirstOrDefaultAsync();

      if (ultimaEvaluacion == null)
       {
  return Ok(new { nota = (decimal?)null });
              }

                return Ok(new
 {
  idEvaluacion = ultimaEvaluacion.IdEvaluacion,
         fecha = ultimaEvaluacion.FechaEvaluacion,
         nota = ultimaEvaluacion.Nota,
         estado = ultimaEvaluacion.EstadoEvaluacion
    });
     }
catch (Exception ex)
      {
    return StatusCode(500, new { mensaje = "Error al obtener la última evaluación práctica", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
    [AllowAnonymous]
        public async Task<IActionResult> UpdateEvaluacion(int id, [FromBody] UpdateEvaluacionRequest request)
        {
            try
         {
      var evaluacion = await _context.Evaluacions.FindAsync(id);
     if (evaluacion == null)
    return NotFound(new { mensaje = "Evaluación no encontrada" });

       if (!string.IsNullOrWhiteSpace(request.EstadoEvaluacion))
       evaluacion.EstadoEvaluacion = request.EstadoEvaluacion;

 if (request.Nota.HasValue)
    evaluacion.Nota = request.Nota.Value;

         if (!string.IsNullOrWhiteSpace(request.Recomendaciones))
  evaluacion.Recomendaciones = request.Recomendaciones;

     _context.Evaluacions.Update(evaluacion);
     await _context.SaveChangesAsync();

     return Ok(new { mensaje = "Evaluación actualizada exitosamente" });
            }
  catch (Exception ex)
            {
         return StatusCode(500, new { mensaje = "Error al actualizar la evaluación", error = ex.Message });
   }
        }

      [HttpDelete("{id}")]
        [AllowAnonymous]
  public async Task<IActionResult> DeleteEvaluacion(int id)
        {
       try
      {
    var evaluacion = await _context.Evaluacions.FindAsync(id);
      if (evaluacion == null)
 return NotFound(new { mensaje = "Evaluación no encontrada" });

_context.Evaluacions.Remove(evaluacion);
                await _context.SaveChangesAsync();

   return Ok(new { mensaje = "Evaluación eliminada exitosamente" });
            }
    catch (Exception ex)
        {
     return StatusCode(500, new { mensaje = "Error al eliminar la evaluación", error = ex.Message });
}
        }

        private decimal CalcularCalificacion(List<TareaRequest>? tareas)
        {
            if (tareas == null || tareas.Count == 0)
      return 0;

            var sumaCalificaciones = tareas
                .Where(t => t.Calificacion.HasValue && t.Calificacion.Value > 0)
       .Sum(t => t.Calificacion.Value);

            if (tareas.Count > 0)
            {
  var promedio = (decimal)sumaCalificaciones / tareas.Count;
    return promedio * 10;
        }

          return 0;
        }

      [HttpPost("crear-evaluacion-teorica")]
   [AllowAnonymous]
     public async Task<IActionResult> CrearEvaluacionTeorica([FromBody] CrearEvaluacionTeoricaRequest request)
 {
 if (request == null || request.IdUsuario <= 0)
     {
return BadRequest(new { mensaje = "El ID del usuario es requerido" });
     }

  try
 {
     var usuario = await _context.Usuarios.FindAsync(request.IdUsuario);
     if (usuario == null)
        {
        return NotFound(new { mensaje = "Usuario no encontrado" });
        }

      // Validar que hay preguntas
    if (request.Preguntas == null || request.Preguntas.Count == 0)
      {
      return BadRequest(new { mensaje = "Debe proporcionar al menos una pregunta" });
  }

   // Crear una nueva evaluación teórica
   var evaluacion = new Evaluacion
        {
  IdEvaluado = request.IdUsuario,
  IdAdministrador = null,
  FechaEvaluacion = DateOnly.FromDateTime(DateTime.Now),
TipoEvaluacion = true, // true = teórica
 EstadoEvaluacion = "Pendiente",
    Nota = null // Sin calificación hasta que responda
     };

  _context.Evaluacions.Add(evaluacion);
     await _context.SaveChangesAsync();

  if (request.Preguntas != null && request.Preguntas.Count > 0)
    {
    var preguntasValidas = request.Preguntas
.Where(p => !string.IsNullOrWhiteSpace(p.Texto))
 .ToList();

   foreach (var preguntaDto in preguntasValidas)
    {
  // Encontrar el índice de la opción correcta (si es múltiple)
      int? respuestaCorrectaIndex = null;
 if (!preguntaDto.TipoEvaluacion && preguntaDto.OpcionesConCorrecta != null && preguntaDto.OpcionesConCorrecta.Count > 0)
    {
    respuestaCorrectaIndex = preguntaDto.OpcionesConCorrecta
      .FindIndex(o => o.EsCorrecta);
  if (respuestaCorrectaIndex == -1)
   respuestaCorrectaIndex = null;
   }

 var pregunta = new Pregunta
   {
     Texto = preguntaDto.Texto,
   TipoEvaluacion = preguntaDto.TipoEvaluacion, // true=abierta, false=múltiple
IdEvaluacion = evaluacion.IdEvaluacion,
      Opciones = preguntaDto.Opciones, // Guardar opciones como JSON (ya serializado desde el cliente)
RespuestaCorrectaIndex = respuestaCorrectaIndex,
Puntos = preguntaDto.Puntos // Guardar puntos
   };
  _context.Preguntas.Add(pregunta);
}
   await _context.SaveChangesAsync();
       }

return Ok(new
 {
mensaje = "Evaluación teórica creada exitosamente",
     idEvaluacion = evaluacion.IdEvaluacion,
    idUsuario = usuario.IdUsuario,
   nombreUsuario = usuario.NombreCompleto,
      cantidadPreguntas = request.Preguntas?.Count ?? 0
       });
 }
      catch (DbUpdateException dbEx)
 {
     var innerMessage = dbEx.InnerException?.Message ?? "Sin detalles";
      Console.WriteLine($"DbUpdateException: {dbEx.Message}");
       Console.WriteLine($"Inner Exception: {innerMessage}");
      Console.WriteLine($"Stack Trace: {dbEx.StackTrace}");
      return StatusCode(500, new
 {
 mensaje = "Error de base de datos al guardar",
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
 mensaje = "Error al crear la evaluación",
  error = ex.Message,
        innerException = ex.InnerException?.Message
  });
       }
        }

        /// <summary>
        /// HU11 - Asistente de IA para el Administrador.
        /// Genera preguntas abiertas y de opción múltiple contextualizadas en el
        /// rubro farmacéutico de Delta usando Google Gemini (gemini-2.5-flash).
        /// </summary>
        [HttpPost("generar-ia")]
        [Authorize(Roles = "Inspector,Administrador")]
        public async Task<IActionResult> GenerarPreguntasIA([FromBody] GenerarPreguntasIARequest request)
        {
            if (request == null)
                return BadRequest(new { mensaje = "Solicitud inválida" });

            var subarea = string.IsNullOrWhiteSpace(request.Subarea) ? "Desarrollo de Medicamentos" : request.Subarea.Trim();
            var dificultad = string.IsNullOrWhiteSpace(request.Dificultad) ? "Intermedio" : request.Dificultad.Trim();
            var cantidadAbiertas = request.CantidadAbiertas > 0 ? Math.Min(request.CantidadAbiertas, 10) : 5;
            var cantidadMultiples = request.CantidadMultiples > 0 ? Math.Min(request.CantidadMultiples, 10) : 5;
            var puntosAbiertas = request.PuntosAbiertas > 0 ? request.PuntosAbiertas : 10;
            var puntosMultiples = request.PuntosMultiples > 0 ? request.PuntosMultiples : 10;

            // API Key desde variable de entorno / configuración (GEMINI_API_KEY)
            var apiKey = _configuration["GEMINI_API_KEY"]
                         ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogError("GEMINI_API_KEY no está configurada.");
                return StatusCode(500, new
                {
                    mensaje = "La API Key de Gemini no está configurada en el servidor. Defina la variable de entorno GEMINI_API_KEY."
                });
            }

            // Modelo de Gemini configurable (GEMINI_MODEL). Por defecto el alias vigente.
            var modelo = _configuration["GEMINI_MODEL"];
            if (string.IsNullOrWhiteSpace(modelo))
                modelo = "gemini-flash-latest";
            modelo = modelo.Trim();

            var systemPrompt = ConstruirSystemPromptFarmaceutico();
            var userPrompt = ConstruirUserPrompt(subarea, dificultad, cantidadAbiertas, cantidadMultiples, puntosAbiertas, puntosMultiples);

            var requestBody = new
            {
                systemInstruction = new
                {
                    parts = new[] { new { text = systemPrompt } }
                },
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = userPrompt } }
                    }
                },
                generationConfig = new
                {
                    responseMimeType = "application/json",
                    responseSchema = ConstruirEsquemaRespuestaIA(),
                    temperature = 0.9,
                    topP = 0.95,
                    maxOutputTokens = 8192,
                    thinkingConfig = new { thinkingBudget = 0 }
                }
            };

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(120);

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelo}:generateContent?key={apiKey}";

                HttpResponseMessage response;
                string responseString;

                // Reintentar ante saturación temporal del plan gratuito (503/429)
                for (int intento = 1; ; intento++)
                {
                    using var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                    response = await httpClient.PostAsync(url, content);
                    responseString = await response.Content.ReadAsStringAsync();

                    var recuperable =
                        response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                        response.StatusCode == System.Net.HttpStatusCode.TooManyRequests;

                    if (response.IsSuccessStatusCode || !recuperable || intento >= 3)
                        break;

                    _logger.LogWarning("Gemini devolvió {Status}. Reintentando {Intento}/3...", response.StatusCode, intento);
                    response.Dispose();
                    await Task.Delay(TimeSpan.FromMilliseconds(700 * intento));
                }

                if (!response.IsSuccessStatusCode)
                {
                    response.Dispose();
                    _logger.LogError("Gemini API ({Modelo}) respondió {Status}: {Body}", modelo, response.StatusCode, responseString);
                    return StatusCode((int)response.StatusCode, new
                    {
                        mensaje = $"Error al consultar la IA de Gemini (modelo {modelo}).",
                        detalle = responseString
                    });
                }
                response.Dispose();

                using var doc = JsonDocument.Parse(responseString);
                var textoJson = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrWhiteSpace(textoJson))
                {
                    return StatusCode(502, new { mensaje = "La IA no devolvió contenido. Intente nuevamente." });
                }

                var opcionesJson = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var generado = JsonSerializer.Deserialize<GenerarPreguntasIAResponse>(textoJson, opcionesJson);

                if (generado == null)
                {
                    return StatusCode(502, new { mensaje = "No se pudo interpretar la respuesta de la IA." });
                }

                // Normalizar / validar preguntas abiertas
                var preguntasAbiertas = (generado.PreguntasAbiertas ?? new List<PreguntaGeneradaIA>())
                    .Where(p => !string.IsNullOrWhiteSpace(p.Texto))
                    .Take(cantidadAbiertas)
                    .Select(p => new PreguntaGeneradaIA
                    {
                        Texto = p.Texto.Trim(),
                        Puntos = p.Puntos > 0 ? p.Puntos : puntosAbiertas
                    })
                    .ToList();

                // Normalizar / validar preguntas de opción múltiple
                var preguntasMultiples = (generado.PreguntasMultiples ?? new List<PreguntaMultipleGeneradaIA>())
                    .Where(p => !string.IsNullOrWhiteSpace(p.Texto)
                                && p.Opciones != null
                                && p.Opciones.Count(o => !string.IsNullOrWhiteSpace(o.Texto)) >= 2)
                    .Take(cantidadMultiples)
                    .Select(p =>
                    {
                        var opciones = p.Opciones!
                            .Where(o => !string.IsNullOrWhiteSpace(o.Texto))
                            .Select(o => new OpcionGeneradaIA { Texto = o.Texto.Trim(), EsCorrecta = o.EsCorrecta })
                            .ToList();

                        if (!opciones.Any(o => o.EsCorrecta))
                            opciones[0].EsCorrecta = true;

                        return new PreguntaMultipleGeneradaIA
                        {
                            Texto = p.Texto.Trim(),
                            Puntos = p.Puntos > 0 ? p.Puntos : puntosMultiples,
                            Opciones = opciones
                        };
                    })
                    .ToList();

                return Ok(new
                {
                    mensaje = "Preguntas generadas con IA exitosamente",
                    subarea,
                    dificultad,
                    preguntasAbiertas,
                    preguntasMultiples
                });
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("Timeout al consultar Gemini API.");
                return StatusCode(504, new { mensaje = "La IA tardó demasiado en responder. Intente nuevamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar preguntas con IA");
                return StatusCode(500, new { mensaje = "Error al generar preguntas con IA", error = ex.Message });
            }
        }

        private static string ConstruirSystemPromptFarmaceutico()
        {
            return
                "Eres un experto farmacéutico, químico farmacéutico y auditor técnico senior de la empresa DELTA, " +
                "dedicada al desarrollo, producción, control de calidad y comercialización de medicamentos y productos farmacéuticos. " +
                "Redactas preguntas de evaluación teórica en español, claras, precisas, técnicamente correctas y contextualizadas " +
                "en el sector de medicamentos, desarrollo farmacéutico y normativas de salud (por ejemplo, BPM/GMP, farmacopeas, " +
                "regulación sanitaria, control de calidad, farmacología y farmacovigilancia). " +
                "Cada pregunta debe evaluar competencias reales del personal y pasantes de la empresa. " +
                "No incluyas explicaciones, saludos ni texto fuera del JSON solicitado.";
        }

        private static string ConstruirUserPrompt(string subarea, string dificultad, int cantidadAbiertas, int cantidadMultiples, int puntosAbiertas, int puntosMultiples)
        {
            return
                $"Genera un examen técnico para la empresa farmacéutica DELTA.\n" +
                $"- Subárea / tema: {subarea}\n" +
                $"- Nivel de dificultad: {dificultad}\n" +
                $"- Cantidad de preguntas abiertas: {cantidadAbiertas} (cada una con {puntosAbiertas} puntos)\n" +
                $"- Cantidad de preguntas de opción múltiple: {cantidadMultiples} (cada una con {puntosMultiples} puntos)\n\n" +
                "Requisitos:\n" +
                "1. Las preguntas abiertas deben requerir respuestas de desarrollo técnico (no de sí/no).\n" +
                "2. Cada pregunta de opción múltiple debe tener exactamente 4 opciones y SOLO una marcada con \"esCorrecta\": true.\n" +
                "3. Usa terminología farmacéutica correcta y, cuando aplique, referencia normativas de salud.\n" +
                "4. Ajusta la complejidad al nivel de dificultad indicado.\n" +
                "5. Devuelve únicamente el JSON con el esquema solicitado.";
        }

        private static object ConstruirEsquemaRespuestaIA()
        {
            return new
            {
                type = "OBJECT",
                properties = new
                {
                    preguntasAbiertas = new
                    {
                        type = "ARRAY",
                        items = new
                        {
                            type = "OBJECT",
                            properties = new
                            {
                                texto = new { type = "STRING" },
                                puntos = new { type = "INTEGER" }
                            },
                            required = new[] { "texto", "puntos" }
                        }
                    },
                    preguntasMultiples = new
                    {
                        type = "ARRAY",
                        items = new
                        {
                            type = "OBJECT",
                            properties = new
                            {
                                texto = new { type = "STRING" },
                                puntos = new { type = "INTEGER" },
                                opciones = new
                                {
                                    type = "ARRAY",
                                    items = new
                                    {
                                        type = "OBJECT",
                                        properties = new
                                        {
                                            texto = new { type = "STRING" },
                                            esCorrecta = new { type = "BOOLEAN" }
                                        },
                                        required = new[] { "texto", "esCorrecta" }
                                    }
                                }
                            },
                            required = new[] { "texto", "puntos", "opciones" }
                        }
                    }
                },
                required = new[] { "preguntasAbiertas", "preguntasMultiples" }
            };
        }

        private async Task CrearNotificacion(int idUsuarioDestino, string tipoNotificacion, string mensaje, int? idEvaluacion = null)
{
    var notificacion = new Notificacion
    {
        IdUsuarioDestino = idUsuarioDestino,
        TipoNotificacion = tipoNotificacion,
        Mensaje = mensaje,
        FechaEnvio = DateTime.UtcNow,
        IdEvaluacion = idEvaluacion
    };

    _context.Notificacions.Add(notificacion);
    await _context.SaveChangesAsync();
}

// En el método para calificar evaluaciones:
[HttpPost("calificar/{idEvaluacion}")]
public async Task<IActionResult> CalificarEvaluacion(int idEvaluacion, [FromBody] decimal nota)
{
    var evaluacion = await _context.Evaluacions.FindAsync(idEvaluacion);
    if (evaluacion == null) return NotFound(new { mensaje = "Evaluación no encontrada" });

    evaluacion.Nota = nota;
    evaluacion.EstadoEvaluacion = "Calificado";
    await _context.SaveChangesAsync();

    await CrearNotificacion(
        evaluacion.IdEvaluado,
        "Evaluación Calificada",
        $"Tu evaluación con ID {idEvaluacion} ha sido calificada.",
        idEvaluacion
    );

    return Ok(new { mensaje = "Evaluación calificada y notificación enviada." });
}
    }

    public class CrearEvaluacionPracticaRequest
    {
      public int IdUsuario { get; set; }
  public string NombreUsuario { get; set; } = string.Empty;
   public List<TareaRequest> Tareas { get; set; } = new();
        public decimal? Puntuacion { get; set; }
        public string? Recomendaciones { get; set; }
    }

    public class TareaRequest
    {
        public int IdTarea { get; set; }
   public string Descripcion { get; set; } = string.Empty;
    public string? ResultadoObtenido { get; set; }
        public bool Completada { get; set; }
        public int? Calificacion { get; set; }
  }

    public class UpdateEvaluacionRequest
    {
        public string? EstadoEvaluacion { get; set; }
        public decimal? Nota { get; set; }
        public string? Recomendaciones { get; set; }
    }

    public class CrearEvaluacionTeoricaRequest
    {
        public int IdUsuario { get; set; }
        public int? IdAdministrador { get; set; }
        public List<PreguntaRequest> Preguntas { get; set; } = new();
    }

    public class PreguntaRequest
    {
   public string Texto { get; set; } = string.Empty;
   public bool TipoEvaluacion { get; set; } // true=abierta, false=múltiple
     public string? Opciones { get; set; } // JSON serializado con las opciones
   public int Puntos { get; set; } = 0; // Puntos para esta pregunta
      public List<OpcionConCorrecta> OpcionesConCorrecta { get; set; } = new(); // Para procesar cuál es correcta
    }

    public class OpcionConCorrecta
    {
        public string Texto { get; set; } = string.Empty;
        public bool EsCorrecta { get; set; } = false;
    }

    // ===== HU11: DTOs del Asistente de IA =====
    public class GenerarPreguntasIARequest
    {
        public string? Subarea { get; set; }
        public string? Dificultad { get; set; }
        public int CantidadAbiertas { get; set; } = 5;
        public int CantidadMultiples { get; set; } = 5;
        public int PuntosAbiertas { get; set; } = 10;
        public int PuntosMultiples { get; set; } = 10;
        public string? ContextoAdicional { get; set; }
    }

    public class GenerarPreguntasIAResponse
    {
        public List<PreguntaGeneradaIA>? PreguntasAbiertas { get; set; }
        public List<PreguntaMultipleGeneradaIA>? PreguntasMultiples { get; set; }
    }

    public class PreguntaGeneradaIA
    {
        public string Texto { get; set; } = string.Empty;
        public int Puntos { get; set; }
    }

    public class PreguntaMultipleGeneradaIA : PreguntaGeneradaIA
    {
        public List<OpcionGeneradaIA>? Opciones { get; set; }
    }

    public class OpcionGeneradaIA
    {
        public string Texto { get; set; } = string.Empty;
        public bool EsCorrecta { get; set; }
    }
}

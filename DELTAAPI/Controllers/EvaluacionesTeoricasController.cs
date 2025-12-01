using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DELTAAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Globalization;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace DELTAAPI.Controllers
{
    [ApiController]
    [Route("api/evaluacionesteoricass")]
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
        [AllowAnonymous]
   public async Task<IActionResult> GuardarRespuestasTeoricas([FromBody] GuardarRespuestasRequest request)
      {
  if (request == null || request.IdUsuario <= 0 || request.IdEvaluacion <= 0)
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

  // Guardar las respuestas
      foreach (var respuestaDto in request.Respuestas)
      {
 var respuesta = new Respuesta
    {
IdUsuario = request.IdUsuario,
       IdPregunta = respuestaDto.IdPregunta,
     IdEvaluacion = request.IdEvaluacion,
         TextoRespuesta = respuestaDto.TextoRespuesta
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
         return StatusCode(500, new { mensaje = "Error al guardar las respuestas", error = ex.Message });
   }
 }

    /// <summary>
        /// Obtiene las respuestas de un usuario para una evaluación teórica específica
 /// </summary>
     [HttpGet("respuestas/{idUsuario}/{idEvaluacion}")]
        [AllowAnonymous]
  public async Task<IActionResult> GetRespuestasUsuario(int idUsuario, int idEvaluacion)
  {
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
  return StatusCode(500, new { mensaje = "Error al obtener las respuestas", error = ex.Message });
    }
        }

        /// <summary>
      /// Genera un PDF con las preguntas y respuestas de la evaluación teórica
/// </summary>
     [HttpGet("descargar-pdf/{idUsuario}/{idEvaluacion}")]
 [AllowAnonymous]
   public async Task<IActionResult> DescargarPDF(int idUsuario, int idEvaluacion)
        {
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

      // Obtener respuestas con preguntas
 var respuestas = await _context.Respuestas
       .Where(r => r.IdUsuario == idUsuario && r.IdEvaluacion == idEvaluacion)
          .Include(r => r.IdPreguntaNavigation)
         .OrderBy(r => r.IdPregunta)
    .ToListAsync();

if (!respuestas.Any())
   return NotFound(new { mensaje = "No hay respuestas para generar el PDF" });

    // Crear documento PDF
        Document doc = new Document(PageSize.A4, 25, 25, 30, 30);
       MemoryStream ms = new MemoryStream();
       PdfWriter writer = PdfWriter.GetInstance(doc, ms);

          doc.Open();

  // Título
  iTextSharp.text.Font titleFont = new iTextSharp.text.Font(BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED), 18, iTextSharp.text.Font.BOLD);
        Paragraph title = new Paragraph("EVALUACIÓN TEÓRICA", titleFont)
    {
   Alignment = Element.ALIGN_CENTER,
     SpacingAfter = 20
     };
     doc.Add(title);

      // Información del usuario
    iTextSharp.text.Font infoFont = new iTextSharp.text.Font(BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED), 11, iTextSharp.text.Font.NORMAL);
doc.Add(new Paragraph($"Evaluado: {usuario.NombreCompleto}", infoFont));
      doc.Add(new Paragraph($"Correo: {usuario.Correo}", infoFont));
        doc.Add(new Paragraph($"Fecha de Evaluación: {evaluacion.FechaEvaluacion?.ToString("dd/MM/yyyy")}", infoFont));
   if (evaluacion.Nota.HasValue)
          doc.Add(new Paragraph($"Calificación: {evaluacion.Nota.Value} / 100", infoFont));

 doc.Add(new Paragraph("\n"));

      // Tabla con preguntas y respuestas
        PdfPTable table = new PdfPTable(2);
     table.WidthPercentage = 100;
      table.SetWidths(new float[] { 50, 50 });

    // Encabezados
       iTextSharp.text.Font headerFont = new iTextSharp.text.Font(BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED), 12, iTextSharp.text.Font.BOLD);
         PdfPCell headerCell1 = new PdfPCell(new Phrase("PREGUNTA", headerFont))
 {
  BackgroundColor = new BaseColor(245, 130, 32),
      Padding = 10,
  HorizontalAlignment = Element.ALIGN_CENTER,
   VerticalAlignment = Element.ALIGN_MIDDLE
       };

        PdfPCell headerCell2 = new PdfPCell(new Phrase("RESPUESTA", headerFont))
    {
      BackgroundColor = new BaseColor(245, 130, 32),
      Padding = 10,
        HorizontalAlignment = Element.ALIGN_CENTER,
    VerticalAlignment = Element.ALIGN_MIDDLE
          };

       table.AddCell(headerCell1);
       table.AddCell(headerCell2);

            // Contenido
         iTextSharp.text.Font contentFont = new iTextSharp.text.Font(BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED), 10, iTextSharp.text.Font.NORMAL);
  foreach (var respuesta in respuestas)
      {
  PdfPCell preguntaCell = new PdfPCell(new Phrase(respuesta.IdPreguntaNavigation?.Texto ?? "", contentFont))
    {
     Padding = 10,
  HorizontalAlignment = Element.ALIGN_LEFT,
       VerticalAlignment = Element.ALIGN_TOP,
    MinimumHeight = 40
    };

      PdfPCell respuestaCell = new PdfPCell(new Phrase(respuesta.TextoRespuesta ?? "", contentFont))
         {
       Padding = 10,
            HorizontalAlignment = Element.ALIGN_LEFT,
          VerticalAlignment = Element.ALIGN_TOP,
      MinimumHeight = 40
          };

      table.AddCell(preguntaCell);
 table.AddCell(respuestaCell);
   }

         doc.Add(table);
     doc.Close();

     byte[] pdfBytes = ms.ToArray();
   ms.Close();

        return File(pdfBytes, "application/pdf", $"Evaluacion_Teorica_{usuario.NombreCompleto}_{DateTime.Now:yyyyMMdd}.pdf");
    }
  catch (Exception ex)
            {
           _logger.LogError($"Error al generar PDF: {ex.Message}");
      return StatusCode(500, new { mensaje = "Error al generar el PDF", error = ex.Message });
            }
        }

  /// <summary>
   /// Obtiene todas las evaluaciones teóricas de un usuario
        /// </summary>
     [HttpGet("usuario/{idUsuario}")]
    [AllowAnonymous]
     public async Task<IActionResult> GetEvaluacionesTeoricasUsuario(int idUsuario)
        {
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
 return StatusCode(500, new { mensaje = "Error al obtener las evaluaciones", error = ex.Message });
  }
      }

   /// <summary>
        /// Obtiene todas las evaluaciones teóricas del sistema
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
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
      return StatusCode(500, new { mensaje = "Error al obtener las evaluaciones", error = ex.Message });
   }
        }

       /// <summary>
      /// Obtiene todas las evaluaciones teóricas del sistema (SIN FILTROS - para depuración)
  /// </summary>
        [HttpGet("debug/todas")]
        [AllowAnonymous]
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
      return StatusCode(500, new { mensaje = "Error al obtener las evaluaciones", error = ex.Message });
       }
    }

    /// <summary>
       /// Obtiene evaluaciones teóricas RESPONDIDAS (simpler version)
        /// </summary>
       [HttpGet("pendientes")]
        [AllowAnonymous]
public async Task<IActionResult> GetEvaluacionesPendientesCalificacion()
 {
     try
  {
  var evaluaciones = await _context.Evaluacions
   .Where(e => e.TipoEvaluacion == true && 
             (e.EstadoEvaluacion == "Respondida" || 
  e.EstadoEvaluacion == "Pendiente" ||
            e.Nota == null))
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

  _logger.LogInformation($"GetEvaluacionesPendientes: Total pendientes={evaluaciones.Count}");
 foreach (var e in evaluaciones)
  {
    _logger.LogInformation($"  ID={e.idEvaluacion}, Estado='{e.estadoEvaluacion}', Respuestas={e.cantidadPreguntas}");
       }

   return Ok(evaluaciones);
  }
  catch (Exception ex)
     {
        _logger.LogError($"Error al obtener pendientes: {ex.Message}");
     return StatusCode(500, new { mensaje = "Error", error = ex.Message });
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
}

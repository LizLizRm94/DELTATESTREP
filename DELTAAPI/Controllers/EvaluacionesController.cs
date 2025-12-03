using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DELTAAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace DELTAAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EvaluacionesController : ControllerBase
    {
      private readonly DeltaTestContext _context;

        public EvaluacionesController(DeltaTestContext context)
        {
     _context = context;
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

      // Verificar que el administrador existe si se proporciona
            if (request.IdAdministrador.HasValue && request.IdAdministrador > 0)
            {
                var admin = await _context.Usuarios.FindAsync(request.IdAdministrador);
                if (admin == null)
                {
                    return NotFound(new { mensaje = "Administrador no encontrado" });
                }
            }

      // Crear una nueva evaluación teórica
   var evaluacion = new Evaluacion
        {
    IdEvaluado = request.IdUsuario,
  IdAdministrador = request.IdAdministrador.HasValue && request.IdAdministrador > 0 ? request.IdAdministrador : null,
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
 var pregunta = new Pregunta
       {
    Texto = preguntaDto.Texto,
                  TipoEvaluacion = true,
    IdEvaluacion = evaluacion.IdEvaluacion
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
          mensaje = "Error al crear la evaluación",
    error = ex.Message,
                innerException = ex.InnerException?.Message
    });
       }
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
    }
}

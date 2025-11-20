using Microsoft.AspNetCore.Mvc;
using DELTAAPI.Models; 

namespace DELTAAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PreguntasController : ControllerBase
    {
        private readonly DeltaTestContext _db;

        public PreguntasController(DeltaTestContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> CrearPreguntas([FromBody] List<Pregunta> preguntas)
        {
            // Cambia _db.Pregunta por _db.Set<Pregunta>()
            foreach (var p in preguntas)
            {
                _db.Set<Pregunta>().Add(p);
            }

            await _db.SaveChangesAsync();
            return Ok(new { mensaje = "Preguntas guardadas correctamente" });
        }
    }
}

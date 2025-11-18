using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DELTAAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ValuesController : ControllerBase
    {
        // Este endpoint es accesible por cualquier usuario autenticado
        [Authorize]
        [HttpGet("user")]
        public IActionResult GetUserValues()
        {
            return Ok(new { message = "Valor accesible para cualquier usuario autenticado", user = User.Identity?.Name });
        }

        // Este endpoint solo es accesible por usuarios con el rol "Admin"
        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult GetAdminValues()
        {
            return Ok(new { message = "Valor solo accesible para Admin", user = User.Identity?.Name });
        }
    }
}

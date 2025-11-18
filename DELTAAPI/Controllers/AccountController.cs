using Microsoft.AspNetCore.Mvc;

namespace DELTAAPI.Controllers
{
    // Controlador MVC para mostrar vistas de autenticación
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
    }
}

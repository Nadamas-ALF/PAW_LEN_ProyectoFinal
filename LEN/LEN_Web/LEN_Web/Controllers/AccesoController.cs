using LEN_Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Controllers
{

    public class AccesoController : Controller
    {
        public IActionResult IniciarSesion()
        {
            return View(new LoginViewModel());
        }

        public IActionResult RecuperarContrasena()
        {
            return View(new RecuperarContrasenaViewModel());
        }

        public IActionResult Registro()
        {
            return View(new RegistroViewModel());
        }
    }
}

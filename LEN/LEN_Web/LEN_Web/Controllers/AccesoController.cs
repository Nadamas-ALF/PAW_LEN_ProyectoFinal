using LEN_Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Controllers
{
    public class AccesoController : Controller
    {
        // Las acciones POST existen pero todavia no hablan con el API. Devuelven a
        // la pantalla con un aviso en vez de fingir que guardaron algo, que es lo
        // que pide RG-123. Cuando se conecte el API este aviso se cae solo
        private const string Pendiente =
            "Esta acci&oacute;n todav&iacute;a no est&aacute; conectada al servidor.";

        [HttpGet]
        public IActionResult IniciarSesion()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult IniciarSesion(LoginViewModel modelo)
        {
            TempData["MensajePendiente"] = Pendiente;
            return RedirectToAction(nameof(IniciarSesion));
        }

        [HttpGet]
        public IActionResult Registro()
        {
            return View(new RegistroViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registro(RegistroViewModel modelo)
        {
            TempData["MensajePendiente"] = Pendiente;
            return RedirectToAction(nameof(Registro));
        }

        [HttpGet]
        public IActionResult RecuperarContrasena()
        {
            return View(new RecuperarContrasenaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RecuperarContrasena(RecuperarContrasenaViewModel modelo)
        {
            TempData["MensajePendiente"] = Pendiente;
            return RedirectToAction(nameof(RecuperarContrasena));
        }

        // Lo llaman los dos layouts desde el boton de cerrar sesion, que hasta ahora
        // daba 404. Cuando exista autenticacion aqui va el SignOut y la vuelta a la
        // portada. Mientras tanto manda al inicio de sesion, que es donde el aviso
        // se puede ver
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CerrarSesion()
        {
            TempData["MensajePendiente"] = Pendiente;
            return RedirectToAction(nameof(IniciarSesion));
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Controllers
{
    // El cliente sobre sus propios datos. No tiene vistas: las pantallas son
    // MiCuenta y EditarCuenta de ClienteController
    //
    // Los cinco son stubs: vuelven a la pantalla con un aviso de que falta el
    // API. Ninguno finge haber guardado algo
    public class CuentaController : Controller
    {
        private const string Pendiente =
            "Esta acci&oacute;n todav&iacute;a no est&aacute; conectada: falta el servidor.";

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActualizarDatos()
        {
            TempData["MensajePendiente"] = Pendiente;
            return RedirectToAction("EditarCuenta", "Cliente");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarContrasena()
        {
            TempData["MensajePendiente"] = Pendiente;
            return RedirectToAction("EditarCuenta", "Cliente");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarDireccion()
        {
            TempData["MensajePendiente"] = Pendiente;
            return RedirectToAction("EditarCuenta", "Cliente");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActualizarDireccion()
        {
            TempData["MensajePendiente"] = Pendiente;
            return RedirectToAction("EditarCuenta", "Cliente");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DesactivarDireccion()
        {
            TempData["MensajePendiente"] = Pendiente;
            return RedirectToAction("EditarCuenta", "Cliente");
        }
    }
}

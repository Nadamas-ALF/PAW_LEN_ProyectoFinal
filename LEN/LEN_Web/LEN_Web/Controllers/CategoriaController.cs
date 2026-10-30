using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Controllers
{
    // El administrador sobre las categorias. Sin vistas: la pantalla es
    // Admin/Categorias y las dos modales viven ahi
    //
    // Los tres son stubs y vuelven a la pantalla con el aviso de pendiente
    public class CategoriaController : Controller
    {
        private const string Pendiente =
            "Esta acci&oacute;n todav&iacute;a no est&aacute; conectada: falta el servidor.";

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear()
        {
            TempData["MensajePendiente"] = Pendiente;
            return RedirectToAction("Categorias", "Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar()
        {
            TempData["MensajePendiente"] = Pendiente;
            return RedirectToAction("Categorias", "Admin");
        }

        // No se puede desactivar una categoria con productos activos: el
        // procedimiento lanza THROW 50000 y el mensaje sale en esta pantalla
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarActivo(int id)
        {
            TempData["MensajePendiente"] = Pendiente;
            return RedirectToAction("Categorias", "Admin");
        }
    }
}

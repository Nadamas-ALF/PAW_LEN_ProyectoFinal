using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Controllers
{
    // El administrador sobre los productos. Sin vistas: las pantallas son
    // Productos, ProductoCrear, ProductoEditar y ProductoImagenes de AdminController
    //
    // Los siete son stubs. Vuelven a la pantalla de donde salio el formulario con
    // un aviso de que falta el API, y ninguno finge haber guardado nada
    [Authorize(Roles = "Administrador")]
    public class ProductoController : Controller
    {
        private const string Pendiente =
            "Esta acci&oacute;n todav&iacute;a no est&aacute; conectada: falta el servidor.";

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear()
        {
            TempData["MensajePendiente"] = Pendiente;
            return RedirectToAction("ProductoCrear", "Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int idProducto)
        {
            TempData["MensajePendiente"] = Pendiente;
            return RedirectToAction("ProductoEditar", "Admin", new { idProducto });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarActivo(int id)
        {
            TempData["MensajePendiente"] = Pendiente;
            return RedirectToAction("Productos", "Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActualizarStock(int id, int stock)
        {
            TempData["MensajePendiente"] = Pendiente;
            return RedirectToAction("Productos", "Admin");
        }

        // El formulario llega como multipart por el archivo. El API valida tipo y
        // tamanio otra vez: el accept del campo es solo comodidad
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubirImagen(int id)
        {
            TempData["MensajePendiente"] = Pendiente;
            return RedirectToAction("ProductoImagenes", "Admin", new { idProducto = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarImagen(int id, int idProducto)
        {
            TempData["MensajePendiente"] = Pendiente;
            return RedirectToAction("ProductoImagenes", "Admin", new { idProducto });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarcarPrincipal(int id, int idProducto)
        {
            TempData["MensajePendiente"] = Pendiente;
            return RedirectToAction("ProductoImagenes", "Admin", new { idProducto });
        }
    }
}

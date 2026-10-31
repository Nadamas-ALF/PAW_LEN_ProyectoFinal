using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Controllers
{
    // Agregar y quitar favoritos. Sin vistas: la lista es Cliente/Favoritos y el
    // corazon vive en la tarjeta del catalogo
    //
    // Los dos son stubs. Vuelven a donde se apreto el boton, que llega en
    // urlRetorno para no perder los filtros de busqueda del catalogo
    [Authorize(Roles = "Cliente")]
    public class FavoritoController : Controller
    {
        private const string Pendiente =
            "Los favoritos todav&iacute;a no se guardan: falta conectar el servidor.";

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Agregar(int idProducto, string? urlRetorno)
        {
            TempData["MensajePendiente"] = Pendiente;
            return Volver(urlRetorno);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int idProducto, string? urlRetorno)
        {
            TempData["MensajePendiente"] = Pendiente;
            return Volver(urlRetorno);
        }

        // Solo se acepta una direccion de este mismo sitio. Sin la comprobacion
        // bastaria con armar el formulario a mano para mandar al usuario afuera
        private IActionResult Volver(string? urlRetorno)
        {
            if (!string.IsNullOrWhiteSpace(urlRetorno) && Url.IsLocalUrl(urlRetorno))
            {
                return Redirect(urlRetorno);
            }

            return RedirectToAction("Favoritos", "Cliente");
        }
    }
}

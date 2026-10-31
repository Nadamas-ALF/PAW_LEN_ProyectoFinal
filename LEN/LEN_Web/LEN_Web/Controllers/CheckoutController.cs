using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Controllers
{
    // Solo recibe el formulario del checkout. Las pantallas viven en ClienteController
    [Authorize(Roles = "Cliente")]
    public class CheckoutController : Controller
    {
        // Stub con aviso mientras no exista el API. No redirige a la confirmacion
        // porque eso seria dar por hecha una compra que no se registro (RG-123)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Confirmar(int idDireccionSeleccionada, int idMetodoPagoSeleccionado, string? datoPagoSimulado)
        {
            TempData["MensajePendiente"] =
                "La compra todav&iacute;a no se puede confirmar: falta conectar el servidor.";

            return RedirectToAction("Checkout", "Cliente");
        }
    }
}

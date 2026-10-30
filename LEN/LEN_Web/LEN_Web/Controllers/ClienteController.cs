using LEN_Web.Utilidades;
using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Controllers
{
    // Pantallas del cliente sobre su propia compra y su cuenta. Piden sesion, pero
    // el atributo Authorize entra cuando se configure la autenticacion: hoy no hay
    // esquema registrado y poner Authorize reventaria la vista
    public class ClienteController : Controller
    {
        private readonly IWebHostEnvironment _entorno;

        public ClienteController(IWebHostEnvironment entorno)
        {
            _entorno = entorno;
        }

        public IActionResult Carrito()
        {
            return View(DatosEjemplo.Carrito(_entorno));
        }

        public IActionResult Checkout()
        {
            return View(DatosEjemplo.Checkout(_entorno));
        }

        // Se llega despues de confirmar la compra. El procedimiento recibe IdPedido
        // y IdUsuario los dos a proposito: sin el IdUsuario cualquiera veria la
        // confirmacion de otro cambiando el numero
        public IActionResult ConfirmacionPedido(int idPedido)
        {
            return View(DatosEjemplo.Confirmacion(_entorno));
        }
    }
}

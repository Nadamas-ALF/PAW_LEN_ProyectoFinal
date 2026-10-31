using Microsoft.AspNetCore.Authorization;
using LEN_Web.Utilidades;
using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Controllers
{
    // Pantallas del cliente sobre su propia compra y su cuenta. Solo del rol Cliente
    [Authorize(Roles = "Cliente")]
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

        public IActionResult MisPedidos(int pagina = 1)
        {
            return View(DatosEjemplo.HistorialPedidos(pagina));
        }

        // El procedimiento recibe IdPedido y IdUsuario los dos: con solo el
        // IdPedido cualquiera veria el pedido de otro cambiando el numero. Si no
        // es suyo se vuelve al historial con aviso, no se muestra un 404 que ya
        // confirmaria que el pedido existe
        public IActionResult PedidoDetalle(int idPedido)
        {
            var pedido = DatosEjemplo.DetallePedido(_entorno, idPedido);

            if (pedido is null)
            {
                TempData["MensajeError"] = "No fue posible abrir el pedido solicitado.";
                return RedirectToAction(nameof(MisPedidos));
            }

            return View(pedido);
        }

        public IActionResult Favoritos()
        {
            return View(DatosEjemplo.Favoritos(_entorno));
        }

        public IActionResult MiCuenta()
        {
            return View(DatosEjemplo.Cuenta());
        }

        public IActionResult EditarCuenta()
        {
            return View(DatosEjemplo.EdicionCuenta());
        }
    }
}

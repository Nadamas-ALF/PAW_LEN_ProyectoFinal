using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Controllers
{
    // El administrador sobre los pedidos. Sin vistas: las pantallas son
    // Admin/Pedidos y Admin/PedidoDetalle
    //
    // Se llama PedidoAdmin y no Pedido a proposito: el cliente sobre sus propios
    // pedidos es otra cosa y vive en ClienteController
    public class PedidoAdminController : Controller
    {
        // Cambiar el estado devuelve stock, anula el pago y anula la factura, y
        // no se puede deshacer. Mientras no exista el API el stub no toca nada
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarEstado(int id, int idEstadoNuevo, string? motivoCancelacion)
        {
            TempData["MensajePendiente"] =
                "El cambio de estado todav&iacute;a no est&aacute; conectado: falta el servidor.";

            return RedirectToAction("PedidoDetalle", "Admin", new { idPedido = id });
        }
    }
}

using LEN_Web.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Controllers
{
    // El administrador sobre los pedidos. Sin vistas: las pantallas son
    // Admin/Pedidos y Admin/PedidoDetalle
    //
    // Se llama PedidoAdmin y no Pedido a proposito: el cliente sobre sus propios
    // pedidos es otra cosa y vive en ClienteController
    [Authorize(Roles = "Administrador")]
    public class PedidoAdminController : Controller
    {
        // 4 es Cancelado en TB_ESTADO_PEDIDO
        private const int EstadoCancelado = 4;

        private readonly IClienteApi _api;

        public PedidoAdminController(IClienteApi api)
        {
            _api = api;
        }

        // BT-11. HU-ADM-PED-003 y 004. id es el IdPedido.
        // Cancelar devuelve el stock y anula el pago y la factura en una misma
        // transaccion (lo hace SP_Pedido_CambiarEstado), y no se puede deshacer:
        // Cancelado es final y ya no ofrece estados (CA-6). La confirmacion
        // explicita la pide la vista antes de enviar (CA-2)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, int idEstadoNuevo, string? motivoCancelacion)
        {
            // las dos las repiten el API y el procedimiento: esto solo evita un viaje
            var error =
                idEstadoNuevo <= 0 ? "Seleccione el nuevo estado."
                : idEstadoNuevo == EstadoCancelado && string.IsNullOrWhiteSpace(motivoCancelacion)
                    ? "Para cancelar un pedido hay que indicar el motivo."
                : null;

            if (error is not null)
            {
                TempData["MensajeError"] = error;
                return Volver(id);
            }

            var resultado = await _api.Enviar(HttpMethod.Patch, $"pedidos/{id}/estado",
                new { idEstadoNuevo, motivoCancelacion });

            // 400: transicion no permitida, pedido ya final, motivo vacio (CA-5)
            if (resultado.Tipo == TipoResultadoApi.Invalido)
            {
                TempData["MensajeError"] = resultado.MensajeHtml;
                return Volver(id);
            }

            if (resultado.Tipo == TipoResultadoApi.SinPermiso) return await this.SinPermiso(resultado);
            if (!resultado.EsExito) return this.PaginaError(resultado);

            TempData["MensajeExito"] = idEstadoNuevo == EstadoCancelado
                ? "El pedido fue cancelado. Se devolvi&oacute; el stock y el pago y la factura quedaron anulados."
                : "Estado del pedido actualizado correctamente.";

            return Volver(id);
        }

        private IActionResult Volver(int idPedido) =>
            RedirectToAction("PedidoDetalle", "Admin", new { idPedido });
    }
}

using LEN_Web.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Controllers
{
    // BT-07. Los formularios POST estan protegidos con antiforgery.
    // La API utiliza el JWT de la sesion para identificar al cliente.
    [Authorize(Roles = "Cliente")]
    public class FavoritoController : Controller
    {
        private readonly IClienteApi _api;

        public FavoritoController(IClienteApi api)
        {
            _api = api;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar(int idProducto, string? urlRetorno)
        {
            if (idProducto <= 0)
            {
                TempData["MensajeError"] = "Seleccione un producto válido.";
                return Volver(urlRetorno);
            }

            var resultado = await _api.Enviar(HttpMethod.Post, $"favoritos/{idProducto}");
            if (resultado.Tipo == TipoResultadoApi.SinPermiso)
                return await this.SinPermiso(resultado);

            if (resultado.EsExito)
                TempData["MensajeExito"] = "Producto agregado a favoritos.";
            else if (resultado.Tipo == TipoResultadoApi.Invalido)
                TempData["MensajeError"] = resultado.MensajeHtml ?? "No se pudo agregar el producto.";
            else
                return this.PaginaError(resultado);

            return Volver(urlRetorno);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int idProducto, string? urlRetorno)
        {
            if (idProducto <= 0)
            {
                TempData["MensajeError"] = "Seleccione un producto válido.";
                return Volver(urlRetorno);
            }

            var resultado = await _api.Enviar(HttpMethod.Delete, $"favoritos/{idProducto}");
            if (resultado.Tipo == TipoResultadoApi.SinPermiso)
                return await this.SinPermiso(resultado);

            if (resultado.EsExito)
                TempData["MensajeExito"] = "Producto eliminado de favoritos.";
            else if (resultado.Tipo == TipoResultadoApi.Invalido)
                TempData["MensajeError"] = resultado.MensajeHtml ?? "No se pudo eliminar el favorito.";
            else
                return this.PaginaError(resultado);

            return Volver(urlRetorno);
        }

        // Volver a la misma pagina de origen (con filtros) sin redirigir a otro dominio.
        private IActionResult Volver(string? urlRetorno)
        {
            if (!string.IsNullOrWhiteSpace(urlRetorno) && Url.IsLocalUrl(urlRetorno))
                return Redirect(urlRetorno);

            return RedirectToAction("Favoritos", "Cliente");
        }
    }
}
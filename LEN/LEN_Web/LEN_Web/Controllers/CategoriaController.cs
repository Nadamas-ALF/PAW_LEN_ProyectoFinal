using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LEN_Web.Utilidades;

namespace LEN_Web.Controllers
{
    // El administrador sobre las categorias. Sin vistas: la pantalla es
    // Admin/Categorias y las dos modales viven ahi
    //
    // Los tres son stubs y vuelven a la pantalla con el aviso de pendiente
    [Authorize(Roles = "Administrador")]
    public class CategoriaController : Controller
    {
        private readonly IClienteApi _api;

        public CategoriaController(IClienteApi api)
        {
            _api = api;
        }


        private const string Pendiente =
            "Esta acci&oacute;n todav&iacute;a no est&aacute; conectada: falta el servidor.";

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(string nombre, string? descripcion)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                TempData["MensajeError"] = "El nombre de la categoría es obligatorio.";
                return RedirectToAction("Categorias", "Admin");
            }

            var datos = new
            {
                Nombre = nombre,
                Descripcion = descripcion
            };

            var resultado = await _api.Enviar(
                HttpMethod.Post,
                "categorias",
                datos);

            if (resultado.Tipo == TipoResultadoApi.Invalido)
            {
                TempData["MensajeError"] = resultado.MensajeHtml;
                return RedirectToAction("Categorias", "Admin");
            }

            if (resultado.Tipo == TipoResultadoApi.SinPermiso)
                return await this.SinPermiso(resultado);

            if (!resultado.EsExito)
                return this.PaginaError(resultado);

            TempData["MensajeExito"] = "Categoría registrada correctamente.";

            return RedirectToAction("Categorias", "Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            int idCategoria,
            string nombre,
            string? descripcion)
        {
            if (idCategoria <= 0 || string.IsNullOrWhiteSpace(nombre))
            {
                TempData["MensajeError"] = "Los datos de la categoría no son válidos.";
                return RedirectToAction("Categorias", "Admin");
            }

            var datos = new
            {
                Nombre = nombre,
                Descripcion = descripcion
            };

            var resultado = await _api.Enviar(
                HttpMethod.Put,
                $"categorias/{idCategoria}",
                datos);

            if (resultado.Tipo == TipoResultadoApi.Invalido)
            {
                TempData["MensajeError"] = resultado.MensajeHtml;
                return RedirectToAction("Categorias", "Admin");
            }

            if (resultado.Tipo == TipoResultadoApi.SinPermiso)
                return await this.SinPermiso(resultado);

            if (!resultado.EsExito)
                return this.PaginaError(resultado);

            TempData["MensajeExito"] = "Categoría actualizada correctamente.";

            return RedirectToAction("Categorias", "Admin");
        }

        // No se puede desactivar una categoria con productos activos: el
        // procedimiento lanza THROW 50000 y el mensaje sale en esta pantalla
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarActivo(int id)
        {
            if (id <= 0)
            {
                TempData["MensajeError"] = "La categoría indicada no es válida.";
                return RedirectToAction("Categorias", "Admin");
            }

            var resultado = await _api.Enviar(
                HttpMethod.Patch,
                $"categorias/{id}/estado");

            if (resultado.Tipo == TipoResultadoApi.Invalido)
            {
                TempData["MensajeError"] = resultado.MensajeHtml;
                return RedirectToAction("Categorias", "Admin");
            }

            if (resultado.Tipo == TipoResultadoApi.SinPermiso)
                return await this.SinPermiso(resultado);

            if (!resultado.EsExito)
                return this.PaginaError(resultado);

            TempData["MensajeExito"] = "Estado de la categoría actualizado correctamente.";

            return RedirectToAction("Categorias", "Admin");
        }
    }
}

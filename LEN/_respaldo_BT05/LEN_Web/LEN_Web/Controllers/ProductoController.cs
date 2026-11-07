
using LEN_Web.Models;
using LEN_Web.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ProductoController : Controller
    {
        private readonly IClienteApi _api;

        public ProductoController(IClienteApi api)
        {
            _api = api;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ProductoFormViewModel modelo)
        {
            // Validaciones básicas antes de enviar los datos al API
            var error =
                string.IsNullOrWhiteSpace(modelo.NombreProducto)
                    ? "El nombre del producto es obligatorio."
                : !modelo.IdCategoria.HasValue || modelo.IdCategoria.Value < 1
                    ? "Debe seleccionar una categoría."
                : !modelo.Precio.HasValue || modelo.Precio.Value < 0
                    ? "El precio debe ser mayor o igual a cero."
                : modelo.Stock < 0
                    ? "El stock debe ser mayor o igual a cero."
                : modelo.EsPiezaUnica && modelo.Stock > 1
                    ? "Un producto de pieza única no puede tener más de una unidad."
                : null;

            if (error is not null)
            {
                TempData["MensajeError"] = error;
                return RedirectToAction("ProductoCrear", "Admin");
            }

            // El formulario utiliza NombreProducto, pero el API espera Nombre
            var datos = new
            {
                Nombre = modelo.NombreProducto!.Trim(),
                Descripcion = modelo.Descripcion,
                Precio = modelo.Precio!.Value,
                Stock = modelo.Stock,
                EsPiezaUnica = modelo.EsPiezaUnica,
                Destacado = modelo.Destacado,
                IdCategoria = modelo.IdCategoria!.Value
            };

            var resultado = await _api.Enviar(
                HttpMethod.Post,
                "productos",
                datos);

            if (resultado.Tipo == TipoResultadoApi.Invalido)
            {
                TempData["MensajeError"] =
                    resultado.MensajeHtml ?? "No fue posible registrar el producto.";

                return RedirectToAction("ProductoCrear", "Admin");
            }

            if (resultado.Tipo == TipoResultadoApi.SinPermiso)
                return await this.SinPermiso(resultado);

            if (!resultado.EsExito)
                return this.PaginaError(resultado);

            TempData["MensajeExito"] = "El producto fue registrado correctamente.";

            return RedirectToAction("Productos", "Admin");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(ProductoFormViewModel modelo)
        {
            int idProducto = modelo.IdProducto;

            // Validaciones básicas
            var error =
                idProducto < 1
                    ? "El producto indicado no es válido."
                : string.IsNullOrWhiteSpace(modelo.NombreProducto)
                    ? "El nombre del producto es obligatorio."
                : !modelo.IdCategoria.HasValue || modelo.IdCategoria.Value < 1
                    ? "Debe seleccionar una categoría."
                : !modelo.Precio.HasValue || modelo.Precio.Value < 0
                    ? "El precio debe ser mayor o igual a cero."
                : modelo.Stock < 0
                    ? "El stock debe ser mayor o igual a cero."
                : modelo.EsPiezaUnica && modelo.Stock > 1
                    ? "Un producto de pieza única no puede tener más de una unidad."
                : null;

            if (error is not null)
            {
                TempData["MensajeError"] = error;

                return RedirectToAction(
                    "ProductoEditar", "Admin", new { idProducto });
            }

            // El API espera Nombre, no NombreProducto
            var datos = new
            {
                Nombre = modelo.NombreProducto!.Trim(),
                Descripcion = modelo.Descripcion,
                Precio = modelo.Precio!.Value,
                Stock = modelo.Stock,
                EsPiezaUnica = modelo.EsPiezaUnica,
                Destacado = modelo.Destacado,
                IdCategoria = modelo.IdCategoria!.Value
            };

            // Actualizar mediante PUT
            var resultado = await _api.Enviar(
                HttpMethod.Put,
                $"productos/{idProducto}",
                datos);

            if (resultado.Tipo == TipoResultadoApi.Invalido)
            {
                TempData["MensajeError"] =
                    resultado.MensajeHtml ?? "No fue posible actualizar el producto.";

                return RedirectToAction(
                    "ProductoEditar", "Admin", new { idProducto });
            }

            if (resultado.Tipo == TipoResultadoApi.SinPermiso)
                return await this.SinPermiso(resultado);

            if (!resultado.EsExito)
                return this.PaginaError(resultado);

            TempData["MensajeExito"] =
                "El producto fue actualizado correctamente.";

            return RedirectToAction("Productos", "Admin");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarActivo(int id)
        {
            if (id < 1)
            {
                TempData["MensajeError"] = "El producto indicado no es válido.";
                return RedirectToAction("Productos", "Admin");
            }

            var resultado = await _api.Enviar(
                HttpMethod.Patch,
                $"productos/{id}/estado");

            if (resultado.Tipo == TipoResultadoApi.Invalido)
            {
                TempData["MensajeError"] =
                    resultado.MensajeHtml ?? "No fue posible cambiar el estado del producto.";

                return RedirectToAction("Productos", "Admin");
            }

            if (resultado.Tipo == TipoResultadoApi.SinPermiso)
                return await this.SinPermiso(resultado);

            if (!resultado.EsExito)
                return this.PaginaError(resultado);

            TempData["MensajeExito"] =
                "El estado del producto fue actualizado correctamente.";

            return RedirectToAction("Productos", "Admin");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarStock(int id, int stock)
        {
            // Validar los datos recibidos
            if (id < 1 || stock < 0)
            {
                TempData["MensajeError"] =
                    "El producto o la cantidad de stock no son válidos.";

                return RedirectToAction("Productos", "Admin");
            }

            // El API recibe la nueva cantidad de stock
            var datos = new
            {
                Stock = stock
            };

            var resultado = await _api.Enviar(
                HttpMethod.Patch,
                $"productos/{id}/stock",
                datos);

            if (resultado.Tipo == TipoResultadoApi.Invalido)
            {
                TempData["MensajeError"] =
                    resultado.MensajeHtml ?? "No fue posible actualizar el inventario.";

                return RedirectToAction("Productos", "Admin");
            }

            if (resultado.Tipo == TipoResultadoApi.SinPermiso)
                return await this.SinPermiso(resultado);

            if (!resultado.EsExito)
                return this.PaginaError(resultado);

            TempData["MensajeExito"] =
                "El inventario fue actualizado correctamente.";

            return RedirectToAction("Productos", "Admin");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubirImagen(int id)
        {
            TempData["MensajePendiente"] =
                "Esta acción todavía no está conectada.";

            return RedirectToAction("ProductoImagenes", "Admin", new { idProducto = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarImagen(int id, int idProducto)
        {
            TempData["MensajePendiente"] =
                "Esta acción todavía no está conectada.";

            return RedirectToAction("ProductoImagenes", "Admin", new { idProducto });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarcarPrincipal(int id, int idProducto)
        {
            TempData["MensajePendiente"] =
                "Esta acción todavía no está conectada.";

            return RedirectToAction("ProductoImagenes", "Admin", new { idProducto });
        }
    }
}

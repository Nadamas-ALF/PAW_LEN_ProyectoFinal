
using System.Net.Http.Headers;
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
        private readonly IWebHostEnvironment _entorno;

        // HU-PRD-007 CA-1 y CA-2. El API repite las dos reglas (CA-3)
        private const long TamanoMaximoImagen = 5 * 1024 * 1024;
        private static readonly string[] ExtensionesImagen = [".jpg", ".jpeg", ".png", ".webp"];

        public ProductoController(IClienteApi api, IWebHostEnvironment entorno)
        {
            _api = api;
            _entorno = entorno;
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


        // ------------------------------------------------------------
        // BT-05. Imagenes del producto
        // ------------------------------------------------------------

        // HU-PRD-007 y HU-PRD-010. id es el IdProducto.
        // 1. Se valida aca (interfaz). 2. El API vuelve a validar, registra la
        // imagen y devuelve el nombre del archivo. 3. Se guarda el archivo en
        // wwwroot/imagenes/productos con ese nombre. La base nunca apunta a un
        // archivo que el API no aprobo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubirImagen(int id, IFormFile? archivo, string? textoAlternativo)
        {
            var error =
                archivo is null || archivo.Length == 0 ? "Seleccione una imagen."
                : archivo.Length > TamanoMaximoImagen ? "La imagen supera el m&aacute;ximo de 5 MB."
                : !ExtensionesImagen.Contains(Path.GetExtension(archivo.FileName).ToLowerInvariant())
                    ? "Solo se aceptan im&aacute;genes JPG, PNG o WEBP."
                : textoAlternativo?.Trim().Length > 150 ? "La descripci&oacute;n admite hasta 150 caracteres."
                : null;

            if (error is not null)
            {
                TempData["MensajeError"] = error;
                return VolverAImagenes(id);
            }

            // Ya se sabe que pesa 5 MB o menos: se lee entero una vez y se usa para
            // el API y para el disco
            byte[] bytes;
            using (var memoria = new MemoryStream())
            {
                await archivo!.CopyToAsync(memoria);
                bytes = memoria.ToArray();
            }

            var parteArchivo = new ByteArrayContent(bytes);
            parteArchivo.Headers.ContentType =
                MediaTypeHeaderValue.TryParse(archivo.ContentType, out var tipo)
                    ? tipo
                    : new MediaTypeHeaderValue("application/octet-stream");

            var contenido = new MultipartFormDataContent();
            contenido.Add(parteArchivo, "archivo", Path.GetFileName(archivo.FileName));

            if (!string.IsNullOrWhiteSpace(textoAlternativo))
                contenido.Add(new StringContent(textoAlternativo.Trim()), "textoAlternativo");

            var resultado = await _api.Enviar<ImagenProducto>(HttpMethod.Post, $"productos/{id}/imagenes", contenido);

            if (resultado.Tipo == TipoResultadoApi.Invalido)
            {
                TempData["MensajeError"] = resultado.MensajeHtml;
                return VolverAImagenes(id);
            }

            if (resultado.Tipo == TipoResultadoApi.SinPermiso) return await this.SinPermiso(resultado);
            if (!resultado.EsExito || resultado.Datos is null) return this.PaginaError(resultado);

            var imagen = resultado.Datos;

            // El nombre lo armo el API, pero antes de escribir en disco se revisa que
            // sea productos/<archivo> y nada mas: ninguna ruta sale de esa carpeta
            var ruta = imagen.RutaImagen ?? string.Empty;
            var nombre = Path.GetFileName(ruta);
            var rutaValida = nombre.Length > 0 && ruta == $"productos/{nombre}";

            try
            {
                if (!rutaValida)
                    throw new IOException($"Ruta de imagen inesperada: {ruta}");

                var carpeta = Path.Combine(_entorno.WebRootPath, "imagenes", "productos");
                Directory.CreateDirectory(carpeta);
                await System.IO.File.WriteAllBytesAsync(Path.Combine(carpeta, nombre), bytes);
            }
            catch (Exception falla) when (falla is IOException or UnauthorizedAccessException)
            {
                // La fila ya quedo en la base. Se da de baja para que la tienda no
                // muestre una imagen rota (si era la principal, el SP promueve otra)
                await _api.Enviar(HttpMethod.Patch, $"productos/imagenes/{imagen.IdImagen}/estado", new { });

                TempData["MensajeError"] = "No se pudo guardar el archivo de la imagen. Intente de nuevo.";
                return VolverAImagenes(id);
            }

            // CA-4. La primera imagen del producto queda principal sola (lo hace el SP)
            TempData["MensajeExito"] = imagen.EsPrincipal
                ? "Imagen subida. Qued&oacute; como principal porque es la primera del producto."
                : "Imagen subida correctamente.";

            return VolverAImagenes(id);
        }

        // HU-PRD-009. id es el IdImagen. Baja logica: deja de mostrarse en la
        // tienda y el archivo se conserva en disco (DEC-006). Si era la
        // principal, el SP promueve otra activa (CA-3)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarImagen(int id, int idProducto)
        {
            var resultado = await _api.Enviar(HttpMethod.Patch, $"productos/imagenes/{id}/estado", new { });

            return await TerminarImagen(resultado, idProducto, "La imagen fue desactivada y ya no se muestra en la tienda.");
        }

        // HU-PRD-008. id es el IdImagen. La anterior pierde la marca (CA-1)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarPrincipal(int id, int idProducto)
        {
            var resultado = await _api.Enviar(HttpMethod.Patch, $"productos/imagenes/{id}/principal", new { });

            return await TerminarImagen(resultado, idProducto, "La imagen qued&oacute; como principal.");
        }

        private async Task<IActionResult> TerminarImagen(ResultadoApi resultado, int idProducto, string mensajeExito)
        {
            if (resultado.Tipo == TipoResultadoApi.Invalido)
            {
                TempData["MensajeError"] = resultado.MensajeHtml;
                return VolverAImagenes(idProducto);
            }

            if (resultado.Tipo == TipoResultadoApi.SinPermiso) return await this.SinPermiso(resultado);
            if (!resultado.EsExito) return this.PaginaError(resultado);

            TempData["MensajeExito"] = mensajeExito;
            return VolverAImagenes(idProducto);
        }

        private IActionResult VolverAImagenes(int idProducto) =>
            RedirectToAction("ProductoImagenes", "Admin", new { idProducto });
    }
}

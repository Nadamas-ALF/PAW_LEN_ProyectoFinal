using LEN_Web.Models;
using Microsoft.AspNetCore.Authorization;
using LEN_Web.Utilidades;
using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Controllers
{
    // Pantallas publicas de la tienda. No piden sesion
    [AllowAnonymous]
    public class PublicoController : Controller
    {
        private readonly IClienteApi _api;

        // HU-PUB-004. Productos por pagina del catalogo
        private const int TamanoPagina = 12;

        public PublicoController(IClienteApi api)
        {
            _api = api;
        }

        // BT-05. HU-PUB-002, 003 y 004.
        // Los nombres de los parametros son los name del formulario de filtros y no
        // se cambian: la busqueda se comparte por enlace (HU-PUB-003 CA-4 y CA-5)
        public async Task<IActionResult> Catalogo(string? busqueda, int? idCategoria, string? color, int pagina = 1)
        {
            if (pagina < 1) pagina = 1;

            var categorias = await _api.Obtener<List<CategoriaAdmin>>("categorias/activas");
            if (!categorias.EsExito || categorias.Datos is null) return this.PaginaError(categorias);

            var filtros = new List<string> { $"pagina={pagina}", $"tamanoPagina={TamanoPagina}" };
            if (!string.IsNullOrWhiteSpace(busqueda)) filtros.Add($"busqueda={Uri.EscapeDataString(busqueda.Trim())}");
            if (idCategoria.HasValue) filtros.Add($"idCategoria={idCategoria.Value}");
            if (!string.IsNullOrWhiteSpace(color)) filtros.Add($"color={Uri.EscapeDataString(color.Trim())}");

            var productos = await _api.Obtener<List<ProductoResumen>>($"productos/catalogo?{string.Join("&", filtros)}");
            if (!productos.EsExito || productos.Datos is null) return this.PaginaError(productos);

            // Una pagina que ya no existe (por ejemplo un enlace viejo) vuelve a
            // la primera con los mismos filtros, en vez de mostrar "sin resultados"
            if (productos.Datos.Count == 0 && pagina > 1)
                return RedirectToAction(nameof(Catalogo), new { busqueda, idCategoria, color });

            // HU-PUB-002 CA-3. Agotado se calcula del stock
            foreach (var producto in productos.Datos)
                producto.Disponible = producto.Stock > 0;

            var totalFilas = productos.Datos.FirstOrDefault()?.TotalFilas ?? 0;

            return View(new CatalogoViewModel
            {
                Busqueda = busqueda,
                IdCategoria = idCategoria,
                Color = color,
                PaginaActual = pagina,
                TotalPaginas = Math.Max(1, (int)Math.Ceiling(totalFilas / (double)TamanoPagina)),
                Productos = productos.Datos,
                Categorias = categorias.Datos.Select(c => (c.IdCategoria, c.Nombre)).ToArray()
            });
        }

        // BT-05. HU-PUB-005
        public async Task<IActionResult> ProductoDetalle(int idProducto)
        {
            var detalle = await _api.Obtener<ProductoDetalleViewModel>($"productos/{idProducto}/publico");

            // CA-2. Un producto dado de baja no devuelve fila: la pantalla da 404 en
            // vez de una pagina vacia
            if (detalle.Tipo == TipoResultadoApi.NoEncontrado)
                return NotFound();

            if (!detalle.EsExito || detalle.Datos is null)
                return this.PaginaError(detalle);

            // CA-5. La galeria, con la principal primero
            var galeria = await _api.Obtener<List<ImagenProducto>>($"productos/{idProducto}/galeria");
            if (!galeria.EsExito || galeria.Datos is null) return this.PaginaError(galeria);

            detalle.Datos.Imagenes = galeria.Datos;

            return View(detalle.Datos);
        }
    }
}

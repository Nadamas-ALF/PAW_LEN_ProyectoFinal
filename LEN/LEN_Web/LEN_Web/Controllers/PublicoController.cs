using Microsoft.AspNetCore.Authorization;
using LEN_Web.Utilidades;
using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Controllers
{
    // Pantallas publicas de la tienda. No piden sesion
    [AllowAnonymous]
    public class PublicoController : Controller
    {
        private readonly IWebHostEnvironment _entorno;

        public PublicoController(IWebHostEnvironment entorno)
        {
            _entorno = entorno;
        }

        // Los nombres de los parametros son los name del formulario de filtros y no
        // se cambian: la busqueda se comparte por enlace
        public IActionResult Catalogo(string? busqueda, int? idCategoria, string? color, int pagina = 1)
        {
            return View(DatosEjemplo.CatalogoFiltrado(_entorno, busqueda, idCategoria, color, pagina));
        }

        public IActionResult ProductoDetalle(int idProducto)
        {
            var detalle = DatosEjemplo.Detalle(_entorno, idProducto);

            // Un producto dado de baja no devuelve fila: la pantalla da 404 en vez
            // de una pagina vacia
            if (detalle is null)
            {
                return NotFound();
            }

            return View(detalle);
        }
    }
}

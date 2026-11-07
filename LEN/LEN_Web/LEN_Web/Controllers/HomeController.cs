using Microsoft.AspNetCore.Authorization;
using LEN_Web.Models;
using LEN_Web.Utilidades;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LEN_Web.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _entorno;
        private readonly IClienteApi _api;

        // HU-PUB-001 CA-3. Cuantos productos lleva la seleccion inicial
        private const int ProductosEnPortada = 8;

        public HomeController(IWebHostEnvironment entorno, IClienteApi api)
        {
            _entorno = entorno;
            _api = api;
        }

        // BT-05. HU-PUB-001. Publica (CA-1). Las tres imagenes de marca son fijas
        // del sitio y siguen saliendo de wwwroot/imagenes
        public async Task<IActionResult> Index()
        {
            ViewData["ImagenMarca"] = DatosEjemplo.Imagen(_entorno, "marca-hero.jpeg");
            ViewData["ImagenColeccionCharms"] = DatosEjemplo.Imagen(_entorno, "coleccion-charms.jpeg");
            ViewData["ImagenColeccionLen"] = DatosEjemplo.Imagen(_entorno, "coleccion-len.jpeg");

            var seleccion = await _api.Obtener<List<ProductoResumen>>(
                $"productos/catalogo?pagina=1&tamanoPagina={ProductosEnPortada}");

            if (!seleccion.EsExito || seleccion.Datos is null)
                return this.PaginaError(seleccion);

            foreach (var producto in seleccion.Datos)
                producto.Disponible = producto.Stock > 0;

            // CA-4. Sin destacados, o si esa consulta falla, la seccion no se
            // pinta y el resto de la portada sigue funcionando
            var destacados = await _api.Obtener<List<ProductoDestacado>>("productos/destacados?cantidad=4");

            return View(new PortadaViewModel
            {
                Destacados = destacados.EsExito && destacados.Datos is not null ? destacados.Datos : new(),
                Productos = seleccion.Datos
            });
        }

        // Lo llama UseStatusCodePagesWithReExecute con el codigo que ya traia la respuesta.
        // 401 no llega aca: la cookie lo convierte en redireccion al inicio de sesion
        [Route("Home/Estado/{codigo:int}")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Estado(int codigo)
        {
            return codigo switch
            {
                403 => View("AccesoDenegado"),
                404 => View("NoEncontrado"),
                _ => View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier })
            };
        }

        // Entra por el fallback de Program.cs. Una ruta inexistente no tiene endpoint, y sin
        // endpoint la politica de respaldo manda al visitante anonimo al inicio de sesion
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult NoEncontrado()
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NoEncontrado");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

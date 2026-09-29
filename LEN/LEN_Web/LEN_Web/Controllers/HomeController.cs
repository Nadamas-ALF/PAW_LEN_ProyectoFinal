using LEN_Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LEN_Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _entorno;

        public HomeController(IHttpClientFactory http, IConfiguration config, IWebHostEnvironment entorno)
        {
            _http = http;
            _config = config;
            _entorno = entorno;
        }

        public IActionResult Index()
        {
            var modelo = new PortadaViewModel
            {
                Destacados = new List<ProductoDestacado>
                {
                    new() { IdProducto = 1, NombreCategoria = "Collares", NombreProducto = "Collar Bello", Precio = 8500m, RutaImagen = RutaImagenSiExiste("destacado-collar-bello.jpeg") },
                    new() { IdProducto = 3, NombreCategoria = "Pulseras", NombreProducto = "Pulsera de Perlas", Precio = 5500m, RutaImagen = RutaImagenSiExiste("destacado-pulsera-perlas.jpeg") },
                    new() { IdProducto = 5, NombreCategoria = "Aretes", NombreProducto = "Aretes Luna", Precio = 3800m, RutaImagen = RutaImagenSiExiste("destacado-aretes-luna.jpeg") },
                },
                Productos = new List<ProductoResumen>
                {
                    new() { IdProducto = 1, NombreCategoria = "Collares", NombreProducto = "Collar Bello", Precio = 8500m, Disponible = true, RutaImagen = RutaImagenSiExiste("producto-collar-bello.jpeg") },
                    new() { IdProducto = 2, NombreCategoria = "Collares", NombreProducto = "Collar Pato", Precio = 7000m, Disponible = true, RutaImagen = RutaImagenSiExiste("producto-collar-pato.jpeg") },
                    new() { IdProducto = 3, NombreCategoria = "Pulseras", NombreProducto = "Pulsera de Perlas", Precio = 5500m, Disponible = true, RutaImagen = RutaImagenSiExiste("producto-pulsera-perlas.jpeg") },
                    new() { IdProducto = 4, NombreCategoria = "Llaveros", NombreProducto = "Llavero Flor", Precio = 2500m, Disponible = false, RutaImagen = RutaImagenSiExiste("producto-llavero-flor.jpeg") },
                },
            };

            ViewData["ImagenMarca"] = RutaImagenSiExiste("marca-hero.jpeg");
            ViewData["ImagenColeccionCharms"] = RutaImagenSiExiste("coleccion-charms.jpeg");
            ViewData["ImagenColeccionLen"] = RutaImagenSiExiste("coleccion-len.jpeg");

            return View(modelo);
        }


        private string? RutaImagenSiExiste(string archivo)
        {
            var ruta = Path.Combine(_entorno.WebRootPath, "imagenes", archivo);
            return System.IO.File.Exists(ruta) ? archivo : null;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Pronostico()
        {
            var cliente = _http.CreateClient();
            var url = _config.GetValue<string>("Valores:UrlAPI") + "WeatherForecast";

            var datos = await cliente.GetFromJsonAsync<List<PronosticoViewModel>>(url);

            return View(datos);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
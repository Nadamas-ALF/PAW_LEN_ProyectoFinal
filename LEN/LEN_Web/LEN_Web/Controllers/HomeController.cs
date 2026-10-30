using LEN_Web.Models;
using LEN_Web.Utilidades;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LEN_Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _entorno;

        public HomeController(IWebHostEnvironment entorno)
        {
            _entorno = entorno;
        }

        public IActionResult Index()
        {
            ViewData["ImagenMarca"] = DatosEjemplo.Imagen(_entorno, "marca-hero.jpeg");
            ViewData["ImagenColeccionCharms"] = DatosEjemplo.Imagen(_entorno, "coleccion-charms.jpeg");
            ViewData["ImagenColeccionLen"] = DatosEjemplo.Imagen(_entorno, "coleccion-len.jpeg");

            return View(DatosEjemplo.Portada(_entorno));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

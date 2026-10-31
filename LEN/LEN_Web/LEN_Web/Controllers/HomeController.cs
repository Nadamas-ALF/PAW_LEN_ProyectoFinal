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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

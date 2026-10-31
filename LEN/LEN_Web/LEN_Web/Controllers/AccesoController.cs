using System.Security.Claims;
using LEN_Web.Models;
using LEN_Web.Utilidades;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Controllers
{
    [AllowAnonymous]
    public class AccesoController : Controller
    {
        // Las acciones que todavia no hablan con el API devuelven a la pantalla con un
        // aviso en vez de fingir que guardaron algo, que es lo que pide RG-123
        private const string Pendiente =
            "Esta acci&oacute;n todav&iacute;a no est&aacute; conectada al servidor.";

        private const string ClaveRetorno = "RetornoInicioSesion";

        private readonly IClienteApi _api;

        public AccesoController(IClienteApi api)
        {
            _api = api;
        }

        // El formulario manda a /Acceso/IniciarSesion sin query, asi que la ruta a la que
        // volver se guarda aqui entre el GET y el POST
        [HttpGet]
        public IActionResult IniciarSesion(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl)) TempData[ClaveRetorno] = returnUrl;

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IniciarSesion(LoginViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                TempData.Keep(ClaveRetorno);
                return View(modelo);
            }

            var resultado = await _api.Enviar<SesionApi>(HttpMethod.Post, "acceso/iniciar-sesion",
                new { modelo.Correo, modelo.Contrasena });

            if (resultado.Tipo == TipoResultadoApi.Invalido)
            {
                // el mensaje no distingue correo de contrasena a proposito (RQNF-SEC-005)
                ModelState.AddModelError(string.Empty, resultado.Mensaje ?? "El correo o la contraseña no son correctos.");
                TempData.Keep(ClaveRetorno);
                return View(new LoginViewModel { Correo = modelo.Correo });
            }

            if (!resultado.EsExito || resultado.Datos is null) return this.PaginaError(resultado);

            await AbrirSesion(resultado.Datos);

            var retorno = TempData[ClaveRetorno] as string;
            if (Url.IsLocalUrl(retorno)) return LocalRedirect(retorno!);

            return resultado.Datos.IdRol == 1
                ? RedirectToAction("Tablero", "Admin")
                : RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Registro()
        {
            return View(new RegistroViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegistroViewModel modelo)
        {
            if (!ModelState.IsValid) return View(modelo);

            // Terminos lo valida el Web y no viaja: el procedimiento no lo guarda
            var resultado = await _api.Enviar(HttpMethod.Post, "acceso/registro", new
            {
                modelo.Nombre,
                modelo.Apellido,
                modelo.Cedula,
                modelo.Telefono,
                modelo.Correo,
                modelo.Contrasena,
                modelo.ConfirmarContrasena
            });

            if (resultado.Tipo == TipoResultadoApi.Invalido)
            {
                // se queda en la pantalla con lo que escribio, menos las contrasenas
                TempData["MensajeError"] = resultado.MensajeHtml;
                modelo.Contrasena = string.Empty;
                modelo.ConfirmarContrasena = string.Empty;
                return View(modelo);
            }

            if (!resultado.EsExito) return this.PaginaError(resultado);

            TempData["MensajeExito"] = "Su cuenta fue creada. Ya puede iniciar sesi&oacute;n.";
            return RedirectToAction(nameof(IniciarSesion));
        }

        [HttpGet]
        public IActionResult RecuperarContrasena()
        {
            return View(new RecuperarContrasenaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RecuperarContrasena(RecuperarContrasenaViewModel modelo)
        {
            TempData["MensajePendiente"] = Pendiente;
            return RedirectToAction(nameof(RecuperarContrasena));
        }

        // Solo del Web: sin cookie no hay token que mandar, asi que no hace falta endpoint
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CerrarSesion()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["MensajeExito"] = "Cerr&oacute; sesi&oacute;n correctamente.";
            return RedirectToAction("Index", "Home");
        }

        private async Task AbrirSesion(SesionApi sesion)
        {
            var identidad = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, sesion.IdUsuario.ToString()),
                // Name es lo que saluda la cabecera: "Hola, Nombre"
                new Claim(ClaimTypes.Name, sesion.Nombre),
                new Claim(ClaimTypes.Email, sesion.Correo),
                new Claim(ClaimTypes.Role, sesion.NombreRol),
                new Claim("IdRol", sesion.IdRol.ToString()),
                new Claim("TieneContrasenaTemporal", sesion.TieneContrasenaTemporal ? "true" : "false")
            }, CookieAuthenticationDefaults.AuthenticationScheme);

            // la cookie vence junto con el token: no tiene sentido una sesion sin JWT valido
            var propiedades = new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = sesion.Expira
            };
            propiedades.StoreTokens(new[] { new AuthenticationToken { Name = "access_token", Value = sesion.Token } });

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidad), propiedades);
        }
    }
}

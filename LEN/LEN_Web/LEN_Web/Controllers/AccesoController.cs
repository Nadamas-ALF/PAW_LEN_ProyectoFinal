using System.Security.Claims;
using LEN_Web.Models;
using LEN_Web.Utilidades;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Controllers
{
    // Sin atributo en la clase a proposito: [AllowAnonymous] gana sobre cualquier [Authorize],
    // asi que cada accion publica lo lleva y las demas quedan protegidas por la politica de respaldo
    public class AccesoController : Controller
    {
        private const string ClaveRetorno = "RetornoInicioSesion";

        private readonly IClienteApi _api;

        public AccesoController(IClienteApi api)
        {
            _api = api;
        }

        // El formulario manda a /Acceso/IniciarSesion sin query, asi que la ruta a la que
        // volver se guarda aqui entre el GET y el POST
        [HttpGet]
        [AllowAnonymous]
        public IActionResult IniciarSesion(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl)) TempData[ClaveRetorno] = returnUrl;

            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
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

            // HU-ACC-005: entro con una temporal vigente, asi que solo puede ir al cambio obligatorio
            if (resultado.Datos.TieneContrasenaTemporal)
                return RedirectToAction(nameof(CambiarContrasenaTemporal));

            var retorno = TempData[ClaveRetorno] as string;
            if (Url.IsLocalUrl(retorno)) return LocalRedirect(retorno!);

            return resultado.Datos.IdRol == 1
                ? RedirectToAction("Tablero", "Admin")
                : RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Registro()
        {
            return View(new RegistroViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
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
        [AllowAnonymous]
        public IActionResult RecuperarContrasena()
        {
            return View(new RecuperarContrasenaViewModel());
        }

        // El aviso es siempre el mismo: exista el correo o no, y salga el envio o no. El
        // API tambien responde 204 siempre (RQNF-SEC-005)
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecuperarContrasena(RecuperarContrasenaViewModel modelo)
        {
            if (!ModelState.IsValid) return View(modelo);

            var resultado = await _api.Enviar(HttpMethod.Post, "acceso/recuperacion", new { modelo.Correo });

            if (!resultado.EsExito) return this.PaginaError(resultado);

            TempData["MensajeExito"] = "Si el correo est&aacute; registrado, en unos minutos va a recibir un mensaje con una contrase&ntilde;a temporal para entrar.";
            return RedirectToAction(nameof(RecuperarContrasena));
        }

        // HU-ACC-006. Cualquier rol: es lo unico que una sesion con temporal pendiente puede abrir
        [HttpGet]
        [Authorize]
        public IActionResult CambiarContrasenaTemporal()
        {
            // quien no tiene temporal pendiente no tiene nada que hacer aca
            if (User.FindFirst("TieneContrasenaTemporal")?.Value != "true")
                return RedirectToAction("Index", "Home");

            return View(new CambiarContrasenaTemporalViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarContrasenaTemporal(CambiarContrasenaTemporalViewModel modelo)
        {
            if (!ModelState.IsValid) return View(new CambiarContrasenaTemporalViewModel());

            var resultado = await _api.Enviar<SesionApi>(HttpMethod.Post, "acceso/cambiar-contrasena",
                new { modelo.Contrasena, modelo.ConfirmarContrasena });

            if (resultado.Tipo == TipoResultadoApi.Invalido)
            {
                TempData["MensajeError"] = resultado.MensajeHtml;
                return View(new CambiarContrasenaTemporalViewModel());
            }

            if (resultado.Tipo == TipoResultadoApi.SinPermiso) return await this.SinPermiso(resultado);
            if (!resultado.EsExito || resultado.Datos is null) return this.PaginaError(resultado);

            // el token viejo todavia dice que la contrasena es temporal: se cambia por el nuevo
            await AbrirSesion(resultado.Datos);

            TempData["MensajeExito"] = "Su contrase&ntilde;a fue actualizada.";
            return resultado.Datos.IdRol == 1
                ? RedirectToAction("Tablero", "Admin")
                : RedirectToAction("Index", "Home");
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

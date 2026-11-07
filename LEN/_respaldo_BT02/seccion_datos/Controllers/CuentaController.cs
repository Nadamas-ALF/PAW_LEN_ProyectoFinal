using System.Security.Claims;
using LEN_Web.Utilidades;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Controllers
{
    // El cliente sobre sus propios datos. No tiene vistas: las pantallas son
    // MiCuenta y EditarCuenta de ClienteController
    //
    // BT-02: ya conectadas al API. Ninguna manda IdUsuario: el API lo saca del
    // token, asi que aunque alguien agregue un IdUsuario al formulario no sirve
    [Authorize(Roles = "Cliente")]
    public class CuentaController : Controller
    {
        private readonly IClienteApi _api;

        public CuentaController(IClienteApi api)
        {
            _api = api;
        }

        // SP_Usuario_Actualizar por PUT api/usuarios/yo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarDatos(
            string? nombre,
            string? apellido,
            string? cedula,
            string? telefono,
            string? correo)
        {
            // el API las repite: esto solo evita un viaje
            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(apellido) ||
                string.IsNullOrWhiteSpace(correo))
            {
                TempData["MensajeError"] = "El nombre, el apellido y el correo son obligatorios.";
                return Volver();
            }

            var resultado = await _api.Enviar(HttpMethod.Put, "usuarios/yo",
                new { nombre, apellido, cedula, telefono, correo });

            if (resultado.EsExito)
            {
                // la cabecera saluda con el nombre de la cookie: sin esto seguiria
                // diciendo el viejo hasta volver a iniciar sesion
                await RefrescarSesion(nombre, correo);
            }

            return await Terminar(resultado, "Sus datos fueron actualizados.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarContrasena(string? contrasenaActual, string? contrasena, string? confirmarContrasena)
        {
            // el formulario no tiene modelo, asi que las reglas de forma se revisan a mano.
            // El API las repite: esto solo evita un viaje
            var error =
                string.IsNullOrEmpty(contrasenaActual) ? "Escriba su contrase&ntilde;a actual."
                : string.IsNullOrEmpty(contrasena) || contrasena.Length < 8 ? "La nueva contrase&ntilde;a debe tener al menos 8 caracteres."
                : contrasena != confirmarContrasena ? "Las dos contrase&ntilde;as no coinciden."
                : null;

            if (error is not null)
            {
                TempData["MensajeError"] = error;
                return RedirectToAction("EditarCuenta", "Cliente");
            }

            var resultado = await _api.Enviar(HttpMethod.Post, "acceso/cambiar-contrasena",
                new { contrasenaActual, contrasena, confirmarContrasena });

            if (resultado.Tipo == TipoResultadoApi.Invalido)
            {
                TempData["MensajeError"] = resultado.MensajeHtml;
                return RedirectToAction("EditarCuenta", "Cliente");
            }

            if (resultado.Tipo == TipoResultadoApi.SinPermiso) return await this.SinPermiso(resultado);
            if (!resultado.EsExito) return this.PaginaError(resultado);

            // la sesion sigue valida: el token no depende de la contrasena
            TempData["MensajeExito"] = "Su contrase&ntilde;a fue actualizada.";
            return RedirectToAction("EditarCuenta", "Cliente");
        }

        // Provincia y canton no llegan: solo filtran la cascada. Lo unico que se
        // guarda es IdDistrito
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarDireccion(
            int idDistrito,
            string? nombreDestinatario,
            string? direccionExacta,
            string? referencia,
            string? telefonoContacto,
            bool esPrincipal = false)
        {
            var error = RevisarDireccion(idDistrito, nombreDestinatario, direccionExacta, telefonoContacto);
            if (error is not null)
            {
                TempData["MensajeError"] = error;
                return Volver(direcciones: true);
            }

            var resultado = await _api.Enviar(HttpMethod.Post, "direcciones/mias", new
            {
                idDistrito,
                nombreDestinatario,
                direccionExacta,
                referencia,
                telefonoContacto,
                esPrincipal
            });

            return await Terminar(resultado, "Direcci&oacute;n registrada correctamente.", direcciones: true);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarDireccion(
            int idDireccion,
            int idDistrito,
            string? nombreDestinatario,
            string? direccionExacta,
            string? referencia,
            string? telefonoContacto,
            bool esPrincipal = false)
        {
            var error = idDireccion <= 0
                ? "No se indic&oacute; qu&eacute; direcci&oacute;n editar."
                : RevisarDireccion(idDistrito, nombreDestinatario, direccionExacta, telefonoContacto);

            if (error is not null)
            {
                TempData["MensajeError"] = error;
                return Volver(direcciones: true);
            }

            var resultado = await _api.Enviar(HttpMethod.Put, $"direcciones/mias/{idDireccion}", new
            {
                idDistrito,
                nombreDestinatario,
                direccionExacta,
                referencia,
                telefonoContacto,
                esPrincipal
            });

            return await Terminar(resultado, "Direcci&oacute;n actualizada correctamente.", direcciones: true);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesactivarDireccion(int idDireccion)
        {
            var resultado = await _api.Enviar(HttpMethod.Patch, $"direcciones/mias/{idDireccion}/estado", new { });

            return await Terminar(resultado, "Direcci&oacute;n desactivada correctamente.", direcciones: true);
        }

        // BT-02. SP_Direccion_MarcarPrincipal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarPrincipal(int idDireccion)
        {
            var resultado = await _api.Enviar(HttpMethod.Patch, $"direcciones/mias/{idDireccion}/principal", new { });

            return await Terminar(resultado, "La direcci&oacute;n qued&oacute; como principal.", direcciones: true);
        }

        // ------------------------------------------------------------

        private static string? RevisarDireccion(int idDistrito, string? destinatario, string? exacta, string? telefono)
        {
            if (idDistrito <= 0)
                return "Seleccione la provincia, el cant&oacute;n y el distrito.";

            if (string.IsNullOrWhiteSpace(destinatario) ||
                string.IsNullOrWhiteSpace(exacta) ||
                string.IsNullOrWhiteSpace(telefono))
                return "El destinatario, la direcci&oacute;n exacta y el tel&eacute;fono son obligatorios.";

            return null;
        }

        // El mismo cierre para todas: 400 se muestra, 401/403 a SinPermiso, lo
        // demas a la pagina de error con el codigo de referencia
        private async Task<IActionResult> Terminar(ResultadoApi resultado, string mensajeExito, bool direcciones = false)
        {
            if (resultado.Tipo == TipoResultadoApi.Invalido)
            {
                TempData["MensajeError"] = resultado.MensajeHtml;
                return Volver(direcciones);
            }

            if (resultado.Tipo == TipoResultadoApi.NoEncontrado)
            {
                TempData["MensajeError"] = "No se encontr&oacute; el registro indicado.";
                return Volver(direcciones);
            }

            if (resultado.Tipo == TipoResultadoApi.SinPermiso) return await this.SinPermiso(resultado);
            if (!resultado.EsExito) return this.PaginaError(resultado);

            TempData["MensajeExito"] = mensajeExito;
            return Volver(direcciones);
        }

        // Las acciones de direcciones vuelven a la vista de solo direcciones, que
        // es desde donde se llamaron
        private IActionResult Volver(bool direcciones = false) =>
            direcciones
                ? RedirectToAction("EditarCuenta", "Cliente", new { seccion = "direcciones" })
                : RedirectToAction("EditarCuenta", "Cliente");

        // Rehace la cookie con el nombre y el correo nuevos. Las propiedades se
        // reusan tal cual, asi el JWT guardado y el vencimiento no cambian
        private async Task RefrescarSesion(string nombre, string correo)
        {
            var actual = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!actual.Succeeded || actual.Principal is null || actual.Properties is null)
                return;

            var claims = actual.Principal.Claims
                .Where(c => c.Type != ClaimTypes.Name && c.Type != ClaimTypes.Email)
                .Append(new Claim(ClaimTypes.Name, nombre.Trim()))
                .Append(new Claim(ClaimTypes.Email, correo.Trim().ToLowerInvariant()));

            var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidad), actual.Properties);
        }
    }
}

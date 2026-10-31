using LEN_Web.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Controllers
{
    // El cliente sobre sus propios datos. No tiene vistas: las pantallas son
    // MiCuenta y EditarCuenta de ClienteController
    //
    // Los cinco son stubs: vuelven a la pantalla con un aviso de que falta el
    // API. Ninguno finge haber guardado algo
    [Authorize(Roles = "Cliente")]
    public class CuentaController : Controller
    {
        private const string Pendiente =
            "Esta acci&oacute;n todav&iacute;a no est&aacute; conectada: falta el servidor.";

        private readonly IClienteApi _api;

        public CuentaController(IClienteApi api)
        {
            _api = api;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActualizarDatos()
        {
            TempData["MensajePendiente"] = Pendiente;
            return RedirectToAction("EditarCuenta", "Cliente");
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarDireccion()
        {
            TempData["MensajePendiente"] = Pendiente;
            return RedirectToAction("EditarCuenta", "Cliente");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActualizarDireccion()
        {
            TempData["MensajePendiente"] = Pendiente;
            return RedirectToAction("EditarCuenta", "Cliente");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DesactivarDireccion()
        {
            TempData["MensajePendiente"] = Pendiente;
            return RedirectToAction("EditarCuenta", "Cliente");
        }
    }
}

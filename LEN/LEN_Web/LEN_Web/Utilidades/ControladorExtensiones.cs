using System.Diagnostics;
using LEN_Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Utilidades
{
    public static class ControladorExtensiones
    {
        // Pagina de error para un Fallo del API. Muestra solo el codigo de referencia
        // (el IdError de TB_ERROR): ni el mensaje, ni la consulta, ni el rastro de pila
        public static IActionResult PaginaError(this Controller controlador, ResultadoApi resultado)
        {
            controlador.Response.StatusCode = StatusCodes.Status500InternalServerError;

            return controlador.View("Error", new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? controlador.HttpContext.TraceIdentifier,
                CodigoReferencia = resultado.CodigoReferencia?.ToString()
            });
        }

        // Para un SinPermiso del API. Un 401 con la sesion abierta es un token vencido o una
        // llave cambiada: se cierra la sesion y se manda a entrar de nuevo. Un 403 es un rol que no alcanza
        public static async Task<IActionResult> SinPermiso(this Controller controlador, ResultadoApi resultado)
        {
            if (resultado.Estado != StatusCodes.Status401Unauthorized)
                return controlador.Forbid();

            await controlador.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            controlador.TempData["MensajeAviso"] = "Su sesi&oacute;n venci&oacute;. Inicie sesi&oacute;n de nuevo.";
            return controlador.RedirectToAction("IniciarSesion", "Acceso");
        }
    }
}

using System.Diagnostics;
using LEN_Web.Models;
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
    }
}

using Microsoft.AspNetCore.Mvc.Controllers;

namespace LEN_Web.Utilidades
{
    // HU-ACC-006: con una contrasena temporal pendiente, ninguna pantalla se abre salvo el
    // cambio obligatorio. Si se pudiera saltar, la temporal seria una contrasena normal.
    // Va despues de UseAuthentication. El API aplica la misma regla por su lado
    public class MiddlewareContrasenaTemporal
    {
        private static readonly (string Controlador, string Accion)[] Permitidas =
        {
            ("Acceso", "CambiarContrasenaTemporal"),
            ("Acceso", "CerrarSesion"),
            ("Home", "Estado"),
            ("Home", "Error")
        };

        private readonly RequestDelegate _siguiente;

        public MiddlewareContrasenaTemporal(RequestDelegate siguiente)
        {
            _siguiente = siguiente;
        }

        public async Task Invoke(HttpContext contexto)
        {
            // solo las acciones de controlador: los archivos estaticos no tienen descriptor
            var accion = contexto.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();

            if (accion is not null
                && contexto.User.Identity?.IsAuthenticated == true
                && contexto.User.FindFirst("TieneContrasenaTemporal")?.Value == "true"
                && !Permitidas.Contains((accion.ControllerName, accion.ActionName)))
            {
                contexto.Response.Redirect("/Acceso/CambiarContrasenaTemporal");
                return;
            }

            await _siguiente(contexto);
        }
    }
}

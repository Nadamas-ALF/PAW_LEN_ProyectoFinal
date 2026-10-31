using System.Net;
using LEN_Web.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Controllers
{
    // El administrador sobre los usuarios. Sin vistas: las pantallas son
    // Admin/Usuarios y Admin/UsuarioDetalle
    //
    // Se llama Usuario y no Cuenta a proposito: Cuenta es el cliente sobre sus
    // propios datos y vive en CuentaController. Aca el IdUsuario viaja en el
    // formulario porque se actua sobre otra persona
    [Authorize(Roles = "Administrador")]
    public class UsuarioController : Controller
    {
        private const string Pendiente =
            "Esta acci&oacute;n todav&iacute;a no est&aacute; conectada: falta el servidor.";

        private readonly IClienteApi _api;

        public UsuarioController(IClienteApi api)
        {
            _api = api;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear()
        {
            TempData["MensajePendiente"] = Pendiente;
            return RedirectToAction("Usuarios", "Admin");
        }

        // Se llama desde la modal de la tabla y desde el detalle. Con IdUsuario
        // se vuelve al detalle, y sin el a la tabla
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int idUsuario)
        {
            TempData["MensajePendiente"] = Pendiente;
            return Volver(idUsuario);
        }

        // HU-USR-005. El administrador no pide la contrasena actual: esta reponiendo el
        // acceso de otra persona. Lo que escribe se guarda como TEMPORAL, no como definitiva,
        // asi que nunca llega a conocer la contrasena de nadie (DEC-004)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestablecerContrasena(int idUsuario, string? contrasena, string? confirmarContrasena)
        {
            if (string.IsNullOrEmpty(contrasena) || contrasena.Length < 8)
            {
                TempData["MensajeError"] = "La contrase&ntilde;a debe tener al menos 8 caracteres.";
                return Volver(idUsuario);
            }

            if (contrasena != confirmarContrasena)
            {
                TempData["MensajeError"] = "Las dos contrase&ntilde;as no coinciden.";
                return Volver(idUsuario);
            }

            var resultado = await _api.Enviar<ContrasenaTemporalApi>(HttpMethod.Post,
                $"usuarios/{idUsuario}/contrasena-temporal", new { contrasena, confirmarContrasena });

            return await Terminar(resultado, idUsuario, mostrarContrasena: false);
        }

        // El boton que estaba deshabilitado. El API genera la contrasena y la devuelve una sola vez
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerarContrasenaTemporal(int idUsuario)
        {
            var resultado = await _api.Enviar<ContrasenaTemporalApi>(HttpMethod.Post,
                $"usuarios/{idUsuario}/contrasena-temporal", new { });

            return await Terminar(resultado, idUsuario, mostrarContrasena: true);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarDireccion(int idUsuario)
        {
            TempData["MensajePendiente"] = Pendiente;
            return Volver(idUsuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ActualizarDireccion(int idUsuario)
        {
            TempData["MensajePendiente"] = Pendiente;
            return Volver(idUsuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DesactivarDireccion(int idUsuario)
        {
            TempData["MensajePendiente"] = Pendiente;
            return Volver(idUsuario);
        }

        private async Task<IActionResult> Terminar(ResultadoApi<ContrasenaTemporalApi> resultado, int idUsuario, bool mostrarContrasena)
        {
            if (resultado.Tipo == TipoResultadoApi.Invalido)
            {
                TempData["MensajeError"] = resultado.MensajeHtml;
                return Volver(idUsuario);
            }

            if (resultado.Tipo == TipoResultadoApi.SinPermiso) return await this.SinPermiso(resultado);
            if (!resultado.EsExito || resultado.Datos is null) return this.PaginaError(resultado);

            var datos = resultado.Datos;
            var hasta = WebUtility.HtmlEncode(datos.Vigencia.ToString("dd/MM/yyyy HH:mm"));

            // la generada se muestra aca y en ningun otro lado: no se guarda ni se vuelve a pedir
            var generada = mostrarContrasena && datos.ContrasenaGenerada is not null
                ? $" Contrase&ntilde;a: <code class=\"LenCodigo\">{WebUtility.HtmlEncode(datos.ContrasenaGenerada)}</code>."
                : string.Empty;

            if (datos.CorreoEnviado)
            {
                TempData["MensajeExito"] = $"Contrase&ntilde;a temporal asignada y enviada por correo. Sirve hasta el {hasta}.{generada}";
            }
            else
            {
                // el administrador esta presente y puede dictarla: decir solo "listo" seria fingir (RG-123)
                TempData["MensajeAviso"] = $"Se asign&oacute; la contrase&ntilde;a temporal, pero no se pudo enviar el correo. Sirve hasta el {hasta}.{generada}"
                    + (mostrarContrasena ? string.Empty : " Comun&iacute;quesela al usuario por otro medio.");
            }

            return Volver(idUsuario);
        }

        private IActionResult Volver(int idUsuario)
        {
            if (idUsuario > 0)
            {
                return RedirectToAction("UsuarioDetalle", "Admin", new { idUsuario });
            }

            return RedirectToAction("Usuarios", "Admin");
        }
    }
}

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



        // BT-02. La cascada es la misma para el cliente y el administrador (CA-5),
        // pero la clase pide Administrador. AllowAnonymous quita esa exigencia solo
        // en estas tres y la sesion se revisa a mano: sin sesion no hay token para el API
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Provincias()
        {
            if (User.Identity?.IsAuthenticated != true) return Unauthorized();

            var resultado = await _api.Obtener<List<ProvinciaApi>>(
                "direcciones/provincias");

            if (resultado.Tipo == TipoResultadoApi.SinPermiso)
                return await this.SinPermiso(resultado);

            if (!resultado.EsExito || resultado.Datos is null)
                return StatusCode(502);

            return Json(resultado.Datos.Select(p => new
            {
                id = p.IdProvincia,
                nombre = p.Nombre
            }));
        }

        // BT-02. CA-1: solo los cantones de la provincia elegida
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Cantones(int idProvincia)
        {
            if (User.Identity?.IsAuthenticated != true) return Unauthorized();

            var resultado = await _api.Obtener<List<CantonApi>>(
                $"direcciones/cantones/{idProvincia}");

            if (resultado.Tipo == TipoResultadoApi.SinPermiso)
                return await this.SinPermiso(resultado);

            if (!resultado.EsExito || resultado.Datos is null)
                return StatusCode(502);

            return Json(resultado.Datos.Select(c => new
            {
                id = c.IdCanton,
                nombre = c.Nombre
            }));
        }

        // BT-02. CA-2 y CA-4: solo los distritos del canton elegido, nunca los 494
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Distritos(int idCanton)
        {
            if (User.Identity?.IsAuthenticated != true) return Unauthorized();

            var resultado = await _api.Obtener<List<DistritoApi>>(
                $"direcciones/distritos/{idCanton}");

            if (resultado.Tipo == TipoResultadoApi.SinPermiso)
                return await this.SinPermiso(resultado);

            if (!resultado.EsExito || resultado.Datos is null)
                return StatusCode(502);

            return Json(resultado.Datos.Select(d => new
            {
                id = d.IdDistrito,
                nombre = d.Nombre
            }));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
    string nombre,
    string apellido,
    string? cedula,
    string? telefono,
    string correo,
    string contrasena,
    int idRol,
    bool activo = true)
        {
            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(apellido) ||
                string.IsNullOrWhiteSpace(correo) ||
                string.IsNullOrWhiteSpace(contrasena))
            {
                TempData["MensajeError"] = "Completa los campos obligatorios.";
                return RedirectToAction("Usuarios", "Admin");
            }

            var datos = new
            {
                Nombre = nombre,
                Apellido = apellido,
                Cedula = cedula,
                Telefono = telefono,
                Correo = correo,
                Contrasena = contrasena,
                IdRol = idRol,
                Activo = activo
            };

            var resultado = await _api.Enviar(
                HttpMethod.Post,
                "usuarios",
                datos);

            if (resultado.Tipo == TipoResultadoApi.Invalido)
            {
                TempData["MensajeError"] = resultado.MensajeHtml;
                return RedirectToAction("Usuarios", "Admin");
            }

            if (resultado.Tipo == TipoResultadoApi.SinPermiso)
                return await this.SinPermiso(resultado);

            if (!resultado.EsExito)
                return this.PaginaError(resultado);

            TempData["MensajeExito"] = "Usuario registrado correctamente.";

            return RedirectToAction("Usuarios", "Admin");
        }

        // Se llama desde la modal de la tabla y desde el detalle. Con IdUsuario
        // se vuelve al detalle, y sin el a la tabla
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
    int idUsuario,
    string nombre,
    string apellido,
    string? cedula,
    string? telefono,
    string correo,
    int idRol,
    bool activo = false)
        {
            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(apellido) ||
                string.IsNullOrWhiteSpace(correo))
            {
                TempData["MensajeError"] = "Completa los campos obligatorios.";
                return RedirectToAction("Usuarios", "Admin");
            }

            var datos = new
            {
                Nombre = nombre,
                Apellido = apellido,
                Cedula = cedula,
                Telefono = telefono,
                Correo = correo,
                IdRol = idRol,
                Activo = activo
            };

            var resultado = await _api.Enviar(
                HttpMethod.Put,
                $"usuarios/{idUsuario}",
                datos);

            if (resultado.Tipo == TipoResultadoApi.Invalido)
            {
                TempData["MensajeError"] = resultado.MensajeHtml;
                return RedirectToAction("Usuarios", "Admin");
            }

            if (resultado.Tipo == TipoResultadoApi.SinPermiso)
                return await this.SinPermiso(resultado);

            if (!resultado.EsExito)
                return this.PaginaError(resultado);

            TempData["MensajeExito"] = "Usuario actualizado correctamente.";

            return RedirectToAction("Usuarios", "Admin");
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
        public async Task<IActionResult> AgregarDireccion(
            int idUsuario,
            int idDistrito,
            string nombreDestinatario,
            string direccionExacta,
            string? referencia,
            string telefonoContacto,
            bool esPrincipal = false)
        {
            var datos = new
            {
                IdDistrito = idDistrito,
                NombreDestinatario = nombreDestinatario,
                DireccionExacta = direccionExacta,
                Referencia = referencia,
                TelefonoContacto = telefonoContacto,
                EsPrincipal = esPrincipal
            };

            var resultado = await _api.Enviar(
                HttpMethod.Post,
                $"direcciones/usuarios/{idUsuario}",
                datos);

            if (resultado.Tipo == TipoResultadoApi.Invalido)
            {
                TempData["MensajeError"] = resultado.MensajeHtml;
                return Volver(idUsuario);
            }

            if (resultado.Tipo == TipoResultadoApi.SinPermiso)
                return await this.SinPermiso(resultado);

            if (!resultado.EsExito)
                return this.PaginaError(resultado);

            TempData["MensajeExito"] = "Dirección registrada correctamente.";

            return Volver(idUsuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarDireccion(
            int idUsuario,
            int idDireccion,
            int idDistrito,
            string nombreDestinatario,
            string direccionExacta,
            string? referencia,
            string telefonoContacto,
            bool esPrincipal = false)
        {
            var datos = new
            {
                IdDistrito = idDistrito,
                NombreDestinatario = nombreDestinatario,
                DireccionExacta = direccionExacta,
                Referencia = referencia,
                TelefonoContacto = telefonoContacto,
                EsPrincipal = esPrincipal
            };

            var resultado = await _api.Enviar(
                HttpMethod.Put,
                $"direcciones/usuarios/{idUsuario}/{idDireccion}",
                datos);

            if (resultado.Tipo == TipoResultadoApi.Invalido)
            {
                TempData["MensajeError"] = resultado.MensajeHtml;
                return Volver(idUsuario);
            }

            if (resultado.Tipo == TipoResultadoApi.SinPermiso)
                return await this.SinPermiso(resultado);

            if (!resultado.EsExito)
                return this.PaginaError(resultado);

            TempData["MensajeExito"] = "Dirección actualizada correctamente.";

            return Volver(idUsuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesactivarDireccion(
            int idUsuario,
            int idDireccion)
        {
            var resultado = await _api.Enviar(
                HttpMethod.Patch,
                $"direcciones/usuarios/{idUsuario}/{idDireccion}/estado",
                new { });

            if (resultado.Tipo == TipoResultadoApi.Invalido)
            {
                TempData["MensajeError"] = resultado.MensajeHtml;
                return Volver(idUsuario);
            }

            if (resultado.Tipo == TipoResultadoApi.SinPermiso)
                return await this.SinPermiso(resultado);

            if (!resultado.EsExito)
                return this.PaginaError(resultado);

            TempData["MensajeExito"] = "Dirección desactivada correctamente.";

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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarActivo(int idUsuario)
        {
            var resultado = await _api.Enviar(
                HttpMethod.Patch,
                $"usuarios/{idUsuario}/estado",
                new { }
            );

            if (resultado.Tipo == TipoResultadoApi.Invalido)
            {
                TempData["MensajeError"] = resultado.MensajeHtml;
                return RedirectToAction("Usuarios", "Admin");
            }

            if (resultado.Tipo == TipoResultadoApi.SinPermiso)
                return await this.SinPermiso(resultado);

            if (!resultado.EsExito)
                return this.PaginaError(resultado);

            TempData["MensajeExito"] = "Estado del usuario actualizado correctamente.";

            return RedirectToAction("Usuarios", "Admin");
        }


        private class ProvinciaApi
        {
            public int IdProvincia { get; set; }
            public string Nombre { get; set; } = string.Empty;
        }

        private class CantonApi
        {
            public int IdCanton { get; set; }
            public string Nombre { get; set; } = string.Empty;
        }

        private class DistritoApi
        {
            public int IdDistrito { get; set; }
            public string Nombre { get; set; } = string.Empty;
        }


    }


}

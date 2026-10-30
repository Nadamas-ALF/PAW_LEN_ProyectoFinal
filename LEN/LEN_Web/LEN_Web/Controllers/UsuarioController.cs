using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Controllers
{
    // El administrador sobre los usuarios. Sin vistas: las pantallas son
    // Admin/Usuarios y Admin/UsuarioDetalle
    //
    // Se llama Usuario y no Cuenta a proposito: Cuenta es el cliente sobre sus
    // propios datos y vive en CuentaController. Aca el IdUsuario viaja en el
    // formulario porque se actua sobre otra persona
    public class UsuarioController : Controller
    {
        private const string Pendiente =
            "Esta acci&oacute;n todav&iacute;a no est&aacute; conectada: falta el servidor.";

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

        // El administrador no pide la contrasena actual: esta reponiendo el
        // acceso de otra persona, no cambiando la suya
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RestablecerContrasena(int idUsuario)
        {
            TempData["MensajePendiente"] = Pendiente;
            return Volver(idUsuario);
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

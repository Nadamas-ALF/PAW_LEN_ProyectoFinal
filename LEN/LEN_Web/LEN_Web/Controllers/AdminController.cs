using Microsoft.AspNetCore.Authorization;
using LEN_Web.Utilidades;
using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Controllers
{
    // Pantallas del panel. Solo del rol Administrador
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly IWebHostEnvironment _entorno;

        public AdminController(IWebHostEnvironment entorno)
        {
            _entorno = entorno;
        }

        public IActionResult Tablero()
        {
            return View(DatosEjemplo.Tablero());
        }

        public IActionResult Productos(string? busqueda, int? idCategoria, int pagina = 1)
        {
            return View(DatosEjemplo.ProductosAdmin(_entorno, busqueda, idCategoria, pagina));
        }

        public IActionResult ProductoCrear()
        {
            return View(DatosEjemplo.FormularioNuevo());
        }

        public IActionResult ProductoEditar(int idProducto)
        {
            var formulario = DatosEjemplo.FormularioDe(idProducto);

            if (formulario is null)
            {
                TempData["MensajeError"] = "El producto indicado no existe.";
                return RedirectToAction(nameof(Productos));
            }

            return View(formulario);
        }

        public IActionResult ProductoImagenes(int idProducto)
        {
            var imagenes = DatosEjemplo.ImagenesDe(_entorno, idProducto);

            if (imagenes is null)
            {
                TempData["MensajeError"] = "El producto indicado no existe.";
                return RedirectToAction(nameof(Productos));
            }

            return View(imagenes);
        }

        public IActionResult Categorias()
        {
            return View(DatosEjemplo.CategoriasAdmin());
        }

        public IActionResult Pedidos(
            string? busqueda,
            int? idEstado,
            string? fechaInicio,
            string? fechaFin,
            int pagina = 1)
        {
            return View(DatosEjemplo.PedidosAdmin(busqueda, idEstado, fechaInicio, fechaFin, pagina));
        }

        // Aqui no se filtra por usuario: el administrador ve cualquier pedido
        public IActionResult PedidoDetalle(int idPedido)
        {
            var pedido = DatosEjemplo.DetallePedidoAdmin(_entorno, idPedido);

            if (pedido is null)
            {
                TempData["MensajeError"] = "No fue posible abrir el pedido solicitado.";
                return RedirectToAction(nameof(Pedidos));
            }

            return View(pedido);
        }

        public IActionResult Pagos(
            string? busqueda,
            int? idEstadoPago,
            string? fechaInicio,
            string? fechaFin,
            int pagina = 1)
        {
            return View(DatosEjemplo.Pagos(busqueda, idEstadoPago, fechaInicio, fechaFin, pagina));
        }

        public IActionResult Facturas(string? busqueda, string? fechaInicio, string? fechaFin, int pagina = 1)
        {
            return View(DatosEjemplo.Facturas(busqueda, fechaInicio, fechaFin, pagina));
        }

        public IActionResult Usuarios(string? busqueda, int? idRol, bool? activo, int pagina = 1)
        {
            return View(DatosEjemplo.UsuariosAdmin(busqueda, idRol, activo, pagina));
        }

        // Es tambien el Perfil del menu de la barra de arriba, con el Id del
        // usuario de la sesion
        public IActionResult UsuarioDetalle(int idUsuario)
        {
            var usuario = DatosEjemplo.DetalleUsuario(idUsuario);

            if (usuario is null)
            {
                TempData["MensajeError"] = "El usuario indicado no existe.";
                return RedirectToAction(nameof(Usuarios));
            }

            return View(usuario);
        }

        // Las dos fechas son obligatorias. Sin ellas se propone un rango en
        // lugar de responder con un error: la pantalla tiene que abrir
        public IActionResult Reportes(string? fechaInicio, string? fechaFin)
        {
            return View(DatosEjemplo.Reporte(fechaInicio, fechaFin));
        }

        public IActionResult FacturaDetalle(int idFactura)
        {
            var factura = DatosEjemplo.DetalleFactura(_entorno, idFactura);

            if (factura is null)
            {
                TempData["MensajeError"] = "No fue posible abrir la factura solicitada.";
                return RedirectToAction(nameof(Facturas));
            }

            return View(factura);
        }
    }
}

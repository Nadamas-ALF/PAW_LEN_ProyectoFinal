using LEN_Web.Utilidades;
using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Controllers
{
    // Pantallas del panel. Son solo del rol Administrador, pero el atributo
    // Authorize entra cuando se configure la autenticacion: hoy no hay esquema
    // registrado y poner Authorize reventaria la vista
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

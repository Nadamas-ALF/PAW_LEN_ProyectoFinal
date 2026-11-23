using System.Text.Json;
using LEN_Web.Models;
using Microsoft.AspNetCore.Authorization;
using LEN_Web.Utilidades;
using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Controllers
{
    // Pantallas del cliente sobre su propia compra y su cuenta. Solo del rol Cliente
    [Authorize(Roles = "Cliente")]
    public class ClienteController : Controller
    {
        private readonly IWebHostEnvironment _entorno;
        private readonly IClienteApi _api;

        public ClienteController(IWebHostEnvironment entorno, IClienteApi api)
        {
            _entorno = entorno;
            _api = api;
        }

        public IActionResult Carrito()
        {
            return View(DatosEjemplo.Carrito(_entorno));
        }

        public IActionResult Checkout()
        {
            return View(DatosEjemplo.Checkout(_entorno));
        }

        // Se llega despues de confirmar la compra. El procedimiento recibe IdPedido
        // y IdUsuario los dos a proposito: sin el IdUsuario cualquiera veria la
        // confirmacion de otro cambiando el numero
        public IActionResult ConfirmacionPedido(int idPedido)
        {
            return View(DatosEjemplo.Confirmacion(_entorno));
        }

        // BT-10. HU-PED-006. El API solo devuelve los pedidos del usuario del
        // token (CA-1), ya con el estado traducido para el cliente (CA-5)
        private const int PedidosPorPagina = 10;

        public async Task<IActionResult> MisPedidos(int pagina = 1)
        {
            if (pagina < 1) pagina = 1;

            var resultado = await _api.Obtener<List<PedidoResumen>>(
                $"pedidos/mios?pagina={pagina}&tamanoPagina={PedidosPorPagina}");

            if (resultado.Tipo == TipoResultadoApi.SinPermiso) return await this.SinPermiso(resultado);
            if (!resultado.EsExito || resultado.Datos is null) return this.PaginaError(resultado);

            // Una pagina que ya no existe vuelve a la primera
            if (resultado.Datos.Count == 0 && pagina > 1)
                return RedirectToAction(nameof(MisPedidos));

            foreach (var pedido in resultado.Datos)
            {
                // un pedido sin factura todavia no tiene numero: se muestra el Id
                if (string.IsNullOrEmpty(pedido.NumeroFactura))
                    pedido.NumeroFactura = $"Pedido #{pedido.IdPedido}";
            }

            // CA-2. Paginado con el TotalFilas que manda el procedimiento
            var totalFilas = resultado.Datos.FirstOrDefault()?.TotalFilas ?? 0;

            return View(new HistorialPedidosViewModel
            {
                Pedidos = resultado.Datos,
                PaginaActual = pagina,
                TotalPaginas = Math.Max(1, (int)Math.Ceiling(totalFilas / (double)PedidosPorPagina))
            });
        }

        // BT-10. HU-PED-007. El procedimiento recibe IdPedido y el IdUsuario del
        // token los dos (CA-1): con solo el IdPedido cualquiera veria el pedido de
        // otro cambiando el numero. Si no es suyo se vuelve al historial con aviso,
        // no se muestra un 404 que ya confirmaria que el pedido existe (CA-2)
        public async Task<IActionResult> PedidoDetalle(int idPedido)
        {
            var resultado = await _api.Obtener<DetallePedidoViewModel>($"pedidos/mios/{idPedido}");

            if (resultado.Tipo == TipoResultadoApi.NoEncontrado)
            {
                TempData["MensajeError"] = "No fue posible abrir el pedido solicitado.";
                return RedirectToAction(nameof(MisPedidos));
            }

            if (resultado.Tipo == TipoResultadoApi.SinPermiso) return await this.SinPermiso(resultado);
            if (!resultado.EsExito || resultado.Datos is null) return this.PaginaError(resultado);

            var pedido = resultado.Datos;

            if (string.IsNullOrEmpty(pedido.NumeroFactura))
                pedido.NumeroFactura = $"Pedido #{pedido.IdPedido}";

            // CA-4. TB_ESTADO_PAGO no guarda clase CSS: se elige aca segun el
            // estado. Un pago anulado no se pinta como pagado
            if (string.IsNullOrEmpty(pedido.EstadoPago))
            {
                pedido.EstadoPago = "Sin registrar";
                pedido.NombreMetodoPago = "—";
            }

            pedido.ClaseCssEstadoPago = pedido.EstadoPago switch
            {
                "Pagado" => "LenBadge--Exito",
                "Pendiente" => "LenBadge--Proceso",
                "Anulado" => "LenBadge--Negativo",
                _ => "LenBadge--Neutro"
            };

            return View(pedido);
        }

        // BT-07. Muestra solo los favoritos del usuario de la sesion.
        // La API obtiene IdUsuario del JWT y el SP excluye productos inactivos.
        public async Task<IActionResult> Favoritos()
        {
            var resultado = await _api.Obtener<List<ProductoResumen>>("favoritos/mios");
            if (resultado.Tipo == TipoResultadoApi.SinPermiso)
                return await this.SinPermiso(resultado);
            if (!resultado.EsExito || resultado.Datos is null)
                return this.PaginaError(resultado);

            // La tarjeta comparte el mismo modelo del catalogo.
            foreach (var producto in resultado.Datos)
                producto.Disponible = producto.Stock > 0;

            return View(resultado.Datos);
        }

        // BT-02. Solo lectura: datos personales y direcciones activas.
        // Los JSON del API se leen directo a UsuarioCuenta y DireccionEnvio
        public async Task<IActionResult> MiCuenta()
        {
            var cuenta = await _api.Obtener<UsuarioCuenta>("usuarios/yo");
            if (cuenta.Tipo == TipoResultadoApi.SinPermiso) return await this.SinPermiso(cuenta);
            if (!cuenta.EsExito || cuenta.Datos is null) return this.PaginaError(cuenta);

            var direcciones = await _api.Obtener<List<DireccionEnvio>>("direcciones/mias");
            if (direcciones.Tipo == TipoResultadoApi.SinPermiso) return await this.SinPermiso(direcciones);
            if (!direcciones.EsExito || direcciones.Datos is null) return this.PaginaError(direcciones);

            return View(new CuentaViewModel
            {
                Usuario = cuenta.Datos,
                Direcciones = direcciones.Datos
            });
        }

        // BT-02. Datos, contrasena y direcciones (activas e inactivas). Solo se
        // mandan las 7 provincias: cantones y distritos los pide len.js cuando
        // hace falta (CA-4). Con seccion=direcciones o seccion=datos se muestra solo esa parte
        public async Task<IActionResult> EditarCuenta(string? seccion)
        {
            var cuenta = await _api.Obtener<UsuarioCuenta>("usuarios/yo");
            if (cuenta.Tipo == TipoResultadoApi.SinPermiso) return await this.SinPermiso(cuenta);
            if (!cuenta.EsExito || cuenta.Datos is null) return this.PaginaError(cuenta);

            var direcciones = await _api.Obtener<List<DireccionEnvio>>("direcciones/mias?incluirInactivas=true");
            if (direcciones.Tipo == TipoResultadoApi.SinPermiso) return await this.SinPermiso(direcciones);
            if (!direcciones.EsExito || direcciones.Datos is null) return this.PaginaError(direcciones);

            // El API manda idProvincia y nombre; OpcionUbicacion usa Id. Se lee como
            // JsonElement para no crear una clase de contrato solo para esto
            var provincias = await _api.Obtener<List<JsonElement>>("direcciones/provincias");
            if (provincias.Tipo == TipoResultadoApi.SinPermiso) return await this.SinPermiso(provincias);
            if (!provincias.EsExito || provincias.Datos is null) return this.PaginaError(provincias);

            return View(new EditarCuentaViewModel
            {
                Usuario = cuenta.Datos,
                Direcciones = direcciones.Datos,
                SoloDirecciones = seccion == "direcciones",
                SoloDatos = seccion == "datos",
                Provincias = provincias.Datos.Select(p => new OpcionUbicacion
                {
                    Id = p.GetProperty("idProvincia").GetInt32(),
                    Nombre = p.GetProperty("nombre").GetString() ?? string.Empty
                }).ToList()
            });
        }
    }
}
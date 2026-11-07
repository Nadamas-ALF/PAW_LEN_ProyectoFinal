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

        public IActionResult MisPedidos(int pagina = 1)
        {
            return View(DatosEjemplo.HistorialPedidos(pagina));
        }

        // El procedimiento recibe IdPedido y IdUsuario los dos: con solo el
        // IdPedido cualquiera veria el pedido de otro cambiando el numero. Si no
        // es suyo se vuelve al historial con aviso, no se muestra un 404 que ya
        // confirmaria que el pedido existe
        public IActionResult PedidoDetalle(int idPedido)
        {
            var pedido = DatosEjemplo.DetallePedido(_entorno, idPedido);

            if (pedido is null)
            {
                TempData["MensajeError"] = "No fue posible abrir el pedido solicitado.";
                return RedirectToAction(nameof(MisPedidos));
            }

            return View(pedido);
        }

        public IActionResult Favoritos()
        {
            return View(DatosEjemplo.Favoritos(_entorno));
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

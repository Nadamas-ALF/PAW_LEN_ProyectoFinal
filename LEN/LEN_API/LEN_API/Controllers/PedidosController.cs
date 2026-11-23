using LEN_API.Dtos;
using LEN_API.Infraestructura;
using LEN_API.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LEN_API.Controllers;

// Pedidos. El rol se pide en cada accion y no en la clase: BT-10 es del
// Cliente y BT-11 (administracion de pedidos) va a agregar aqui las del
// Administrador.
//
// BT-10: el IdUsuario sale siempre del token, nunca de la URL. Asi un cliente
// solo ve sus pedidos (HU-PED-006 CA-1) y no puede abrir el de otro cambiando
// el numero (HU-PED-007 CA-1 y CA-2)
[ApiController]
[Authorize]
[Route("api/pedidos")]
public class PedidosController : ControllerBase
{
    private readonly IPedidoServicio _servicio;

    public PedidosController(IPedidoServicio servicio)
    {
        _servicio = servicio;
    }

    // HU-PED-006. Cada fila trae TotalFilas para paginar (CA-2)
    [Authorize(Roles = "Cliente")]
    [HttpGet("mios")]
    public async Task<IActionResult> ListarMios(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10)
    {
        return Ok(await _servicio.ListarPorUsuario(User.IdUsuario(), pagina, tamanoPagina));
    }

    // HU-PED-007. 404 si no existe o si es de otro usuario: no se dice cual de
    // los dos, para no confirmar que el pedido existe
    [Authorize(Roles = "Cliente")]
    [HttpGet("mios/{idPedido:int}")]
    public async Task<IActionResult> ObtenerMio(int idPedido)
    {
        var pedido = await _servicio.ObtenerDetalle(idPedido, User.IdUsuario());

        if (pedido is null)
        {
            return NotFound(new
            {
                mensaje = "No se encontró el pedido solicitado."
            });
        }

        return Ok(pedido);
    }

    // ------------------------------------------------------------
    // BT-11. Administracion de pedidos: solo Administrador
    // ------------------------------------------------------------

    // HU-ADM-PED-001. Busqueda por cliente, correo o factura (CA-1), estado y
    // rango de fechas (CA-2), paginado con TotalFilas en cada fila (CA-3)
    [Authorize(Roles = "Administrador")]
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busqueda,
        [FromQuery] int? idEstadoPedido,
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10)
    {
        return Ok(await _servicio.ListarAdministracion(
            busqueda, idEstadoPedido, fechaInicio, fechaFin, pagina, tamanoPagina));
    }

    // Para el filtro de estado de la bandeja
    [Authorize(Roles = "Administrador")]
    [HttpGet("estados")]
    public async Task<IActionResult> ListarEstados()
    {
        return Ok(await _servicio.ListarEstados());
    }

    // HU-ADM-PED-002. Cualquier pedido, de cualquier cliente (CA-1)
    [Authorize(Roles = "Administrador")]
    [HttpGet("{idPedido:int}")]
    public async Task<IActionResult> ObtenerDetalleAdministracion(int idPedido)
    {
        var pedido = await _servicio.ObtenerDetalleAdministracion(idPedido);

        if (pedido is null)
        {
            return NotFound(new
            {
                mensaje = "El pedido indicado no existe."
            });
        }

        return Ok(pedido);
    }

    // HU-ADM-PED-003 y 004. 400 si la transicion no es valida o falta el motivo
    [Authorize(Roles = "Administrador")]
    [HttpPatch("{idPedido:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int idPedido, [FromBody] CambiarEstadoPedidoDto datos)
    {
        await _servicio.CambiarEstado(User.IdUsuario(), idPedido, datos);

        return Ok(new
        {
            mensaje = datos.IdEstadoNuevo == 4
                ? "El pedido fue cancelado. Se devolvió el stock y el pago y la factura quedaron anulados."
                : "Estado del pedido actualizado correctamente."
        });
    }
}

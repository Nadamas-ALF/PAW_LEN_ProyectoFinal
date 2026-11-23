using LEN_API.Dtos;

namespace LEN_API.Servicios;

// Pedidos. BT-10 (cliente) por ahora; BT-09 y BT-11 agregan aqui lo suyo
public interface IPedidoServicio
{
    // HU-PED-006. Solo los pedidos del usuario, paginados
    Task<List<PedidoResumenDto>> ListarPorUsuario(int idUsuario, int pagina, int tamanoPagina);

    // HU-PED-007. Null si el pedido no existe o es de otro usuario: no se distingue
    Task<PedidoDetalleDto?> ObtenerDetalle(int idPedido, int idUsuario);

    // ------------------------------------------------------------
    // BT-11. Administracion
    // ------------------------------------------------------------

    // HU-ADM-PED-001. Pedidos de todos los clientes, con busqueda y filtros
    Task<List<PedidoAdminResumenDto>> ListarAdministracion(
        string? busqueda,
        int? idEstadoPedido,
        DateTime? fechaInicio,
        DateTime? fechaFin,
        int pagina,
        int tamanoPagina);

    // HU-ADM-PED-002, 003 y 005. Cualquier pedido, con los estados permitidos
    // desde el actual y el historial. Null si no existe
    Task<PedidoAdminDetalleDto?> ObtenerDetalleAdministracion(int idPedido);

    Task<List<EstadoPedidoDto>> ListarEstados();

    // HU-ADM-PED-003 y 004
    Task CambiarEstado(int idUsuarioAccion, int idPedido, CambiarEstadoPedidoDto datos);
}

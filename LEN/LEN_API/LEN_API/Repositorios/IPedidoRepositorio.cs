using LEN_API.Dtos;

namespace LEN_API.Repositorios;

// Pedidos. BT-10 (cliente) por ahora; BT-09 y BT-11 agregan aqui lo suyo
public interface IPedidoRepositorio
{
    // SP_Pedido_ListarPorUsuario
    Task<List<PedidoResumenDto>> ListarPorUsuario(int idUsuario, int pagina, int tamanoPagina);

    // SP_Pedido_ObtenerDetalle. Lista vacia si el pedido no existe o es de otro usuario
    Task<List<PedidoDetalleFilaDto>> ObtenerDetalle(int idPedido, int idUsuario);

    // ------------------------------------------------------------
    // BT-11. Administracion
    // ------------------------------------------------------------

    // SP_Pedido_Listar
    Task<List<PedidoAdminResumenDto>> ListarAdministracion(
        string? busqueda,
        int? idEstadoPedido,
        DateTime? fechaInicio,
        DateTime? fechaFin,
        int pagina,
        int tamanoPagina);

    // SP_Pedido_ObtenerDetalleAdministracion. Lista vacia si no existe
    Task<List<PedidoAdminDetalleFilaDto>> ObtenerDetalleAdministracion(int idPedido);

    // SP_EstadoPedido_Listar
    Task<List<EstadoPedidoDto>> ListarEstados();

    // SP_Pedido_ListarHistorial
    Task<List<HistorialPedidoDto>> ListarHistorial(int idPedido);

    // SP_Pedido_CambiarEstado. Al cancelar devuelve stock y anula pago y factura
    Task CambiarEstado(int idUsuarioAccion, int idPedido, int idEstadoNuevo, string? motivoCancelacion);
}

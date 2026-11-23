namespace LEN_API.Dtos;

// BT-11. El detalle que ve el administrador: el mismo del cliente (BT-10)
// mas el cliente, los estados a los que se puede pasar y el historial
public class PedidoAdminDetalleDto : PedidoDetalleDto
{
    public int IdUsuario { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public string CorreoCliente { get; set; } = string.Empty;
    public string? TelefonoCliente { get; set; }

    // HU-ADM-PED-003 CA-4. Solo los permitidos desde el estado actual. Vacio
    // si el pedido esta Entregado o Cancelado (CA-3)
    public List<EstadoPedidoDto> EstadosSiguientes { get; set; } = new();

    // HU-ADM-PED-005. El mas reciente primero
    public List<HistorialPedidoDto> Historial { get; set; } = new();
}

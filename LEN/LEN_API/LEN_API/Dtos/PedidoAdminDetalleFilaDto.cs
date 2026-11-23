namespace LEN_API.Dtos;

// BT-11. HU-ADM-PED-002. Una fila de SP_Pedido_ObtenerDetalleAdministracion.
// Tiene todo lo de la fila del cliente (BT-10) mas los datos del cliente:
// por eso hereda, y el servicio arma los dos detalles con el mismo codigo
public class PedidoAdminDetalleFilaDto : PedidoDetalleFilaDto
{
    public int IdUsuario { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public string CorreoCliente { get; set; } = string.Empty;
    public string? TelefonoCliente { get; set; }
}

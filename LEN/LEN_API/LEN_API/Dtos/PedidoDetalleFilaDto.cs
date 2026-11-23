namespace LEN_API.Dtos;

// BT-10. HU-PED-007. Una fila de SP_Pedido_ObtenerDetalle: el procedimiento
// devuelve una fila por producto con los datos del pedido repetidos. Solo lo
// usa el repositorio; el servicio lo agrupa en PedidoDetalleDto
public class PedidoDetalleFilaDto
{
    public int IdPedido { get; set; }
    public string? NumeroFactura { get; set; }
    public DateTime FechaPedido { get; set; }
    public decimal Subtotal { get; set; }
    public decimal CostoEnvio { get; set; }
    public decimal Total { get; set; }
    public int IdEstadoPedido { get; set; }
    public string NombreEstado { get; set; } = string.Empty;
    public string? ClaseCssEstado { get; set; }
    public string EstadoVisible { get; set; } = string.Empty;
    public string? MotivoCancelacion { get; set; }

    // Pago: LEFT JOIN, todo null si el pedido no tiene pago
    public DateTime? FechaPago { get; set; }
    public string? NombreMetodoPago { get; set; }
    public string? ComprobantePago { get; set; }
    public string? EstadoPago { get; set; }

    // Direccion de envio
    public string NombreDestinatario { get; set; } = string.Empty;
    public string TelefonoContacto { get; set; } = string.Empty;
    public string DireccionExacta { get; set; } = string.Empty;
    public string? Referencia { get; set; }
    public string NombreDistrito { get; set; } = string.Empty;
    public string NombreCanton { get; set; } = string.Empty;
    public string NombreProvincia { get; set; } = string.Empty;

    // Linea
    public int IdProducto { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal SubtotalLinea { get; set; }
    public string? RutaImagen { get; set; }
}

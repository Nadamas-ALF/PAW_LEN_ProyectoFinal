namespace LEN_API.Dtos;

// BT-10. HU-PED-007. El detalle de un pedido ya agrupado: los datos del pedido
// una sola vez y sus productos en Lineas. La direccion reusa DireccionAdminDto
public class PedidoDetalleDto
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

    // CA-5. Solo viene cuando el pedido esta cancelado
    public string? MotivoCancelacion { get; set; }

    // CA-4. Pagado, Pendiente o Anulado (TB_ESTADO_PAGO). Null si no hay pago
    public string? EstadoPago { get; set; }
    public DateTime? FechaPago { get; set; }
    public string? NombreMetodoPago { get; set; }
    public string? ComprobantePago { get; set; }

    public DireccionAdminDto Direccion { get; set; } = new();

    // CA-3. Con el precio historico de TB_DETALLE_PEDIDO
    public List<LineaPedidoDto> Lineas { get; set; } = new();
}

public class LineaPedidoDto
{
    public int IdProducto { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string? RutaImagen { get; set; }
    public int Cantidad { get; set; }

    // El precio congelado al comprar, no el actual del producto
    public decimal PrecioUnitario { get; set; }
    public decimal SubtotalLinea { get; set; }
}

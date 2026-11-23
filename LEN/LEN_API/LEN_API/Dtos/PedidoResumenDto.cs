namespace LEN_API.Dtos;

// BT-10. HU-PED-006. Una fila de SP_Pedido_ListarPorUsuario
public class PedidoResumenDto
{
    public int IdPedido { get; set; }

    // LEFT JOIN a TB_FACTURA: null si el pedido todavia no tiene factura
    public string? NumeroFactura { get; set; }

    public DateTime FechaPedido { get; set; }

    public decimal Total { get; set; }

    public int IdEstadoPedido { get; set; }

    // Nombre interno (Pagado, En preparación...). El cliente ve EstadoVisible
    public string NombreEstado { get; set; } = string.Empty;

    public string? ClaseCssEstado { get; set; }

    // CA-5. Comprada, En camino, Recibido o Cancelado
    public string EstadoVisible { get; set; } = string.Empty;

    // CA-4. Suma de unidades, no de lineas
    public int CantidadProductos { get; set; }

    // El mismo en todas las filas: el total sin paginar (CA-2)
    public int TotalFilas { get; set; }
}

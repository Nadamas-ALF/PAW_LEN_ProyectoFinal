namespace LEN_API.Dtos;

// BT-11. HU-ADM-PED-001. Una fila de SP_Pedido_Listar: la bandeja del panel,
// con los pedidos de todos los clientes
public class PedidoAdminResumenDto
{
    public int IdPedido { get; set; }
    public string? NumeroFactura { get; set; }
    public DateTime FechaPedido { get; set; }
    public decimal Subtotal { get; set; }
    public decimal CostoEnvio { get; set; }
    public decimal Total { get; set; }
    public int IdEstadoPedido { get; set; }

    // El nombre de TB_ESTADO_PEDIDO: el panel no usa las palabras del cliente
    public string NombreEstado { get; set; } = string.Empty;
    public string? ClaseCssEstado { get; set; }

    public int IdUsuario { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public string CorreoCliente { get; set; } = string.Empty;
    public string? NombreMetodoPago { get; set; }

    // Suma de unidades, no de lineas
    public int CantidadProductos { get; set; }

    // El mismo en todas las filas: el total sin paginar (CA-3)
    public int TotalFilas { get; set; }
}

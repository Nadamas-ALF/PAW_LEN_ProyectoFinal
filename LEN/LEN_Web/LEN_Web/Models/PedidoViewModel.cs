namespace LEN_Web.Models
{
    // Los pedidos vistos por el cliente. El panel tiene sus propias pantallas y
    // usa otras palabras para los mismos estados

    public class PedidoResumen
    {
        public int IdPedido { get; set; }

        public string NumeroFactura { get; set; } = string.Empty;

        public DateTime FechaPedido { get; set; }

        public decimal Total { get; set; }

        public int IdEstadoPedido { get; set; }

        // El cliente ve otras palabras: Pagado es Comprada, En preparacion es
        // En camino y Entregado es Recibido
        public string EstadoVisible { get; set; } = string.Empty;

        // Sale de TB_ESTADO_PEDIDO.ClaseCss, no lo elige la vista
        public string ClaseCssEstado { get; set; } = string.Empty;

        // Suma de unidades, no de lineas
        public int CantidadProductos { get; set; }
    }

    public class HistorialPedidosViewModel
    {
        public List<PedidoResumen> Pedidos { get; set; } = new();

        public int PaginaActual { get; set; } = 1;

        public int TotalPaginas { get; set; } = 1;

        public bool TienePedidos => Pedidos.Count > 0;
    }

    // Repite campos de ConfirmacionPedidoViewModel a proposito: son dos
    // procedimientos distintos y este trae datos que la confirmacion no puede
    // tener todavia, como el motivo de cancelacion o el pago anulado
    public class DetallePedidoViewModel
    {
        public int IdPedido { get; set; }

        public string NumeroFactura { get; set; } = string.Empty;

        public DateTime FechaPedido { get; set; }

        public decimal Subtotal { get; set; }

        public decimal CostoEnvio { get; set; }

        public decimal Total { get; set; }

        public int IdEstadoPedido { get; set; }

        public string EstadoVisible { get; set; } = string.Empty;

        public string ClaseCssEstado { get; set; } = string.Empty;

        // El pedido cancelado siempre trae motivo: lo exige SP_Pedido_CambiarEstado
        public string? MotivoCancelacion { get; set; }

        public DireccionEnvio Direccion { get; set; } = new();

        public string NombreMetodoPago { get; set; } = string.Empty;

        public string? ComprobantePago { get; set; }

        public DateTime? FechaPago { get; set; }

        public string EstadoPago { get; set; } = string.Empty;

        // Un pago anulado no se pinta como pagado. La clase sale del estado
        public string ClaseCssEstadoPago { get; set; } = string.Empty;

        public List<LineaPedido> Lineas { get; set; } = new();

        // 4 es Cancelado en TB_ESTADO_PEDIDO
        public bool EstaCancelado => IdEstadoPedido == 4;
    }
}

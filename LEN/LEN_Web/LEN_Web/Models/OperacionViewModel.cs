namespace LEN_Web.Models
{
    // La operacion diaria del panel: pedidos, pagos y facturas. Los tres salen
    // del mismo pedido, asi que el total que ve el administrador en las tres
    // pantallas es el mismo numero

    public class EstadoPedidoOpcion
    {
        public int IdEstadoPedido { get; set; }

        // El nombre de TB_ESTADO_PEDIDO, que es lo que ve el panel. El cliente
        // ve otras palabras y esas viven en EstadoVisible de PedidoResumen
        public string Nombre { get; set; } = string.Empty;

        public string ClaseCss { get; set; } = string.Empty;
    }

    public class PedidoAdminResumen
    {
        public int IdPedido { get; set; }

        // Puede venir vacio: sin factura la columna dice Pedido # mas el Id
        public string? NumeroFactura { get; set; }

        public DateTime FechaPedido { get; set; }

        public decimal Total { get; set; }

        public int IdEstadoPedido { get; set; }

        public string NombreEstado { get; set; } = string.Empty;

        public string ClaseCssEstado { get; set; } = string.Empty;

        public int IdUsuario { get; set; }

        public string NombreCliente { get; set; } = string.Empty;

        public string CorreoCliente { get; set; } = string.Empty;

        public string NombreMetodoPago { get; set; } = string.Empty;

        // Suma de unidades, no de lineas
        public int CantidadProductos { get; set; }

        public string FacturaVisible => string.IsNullOrWhiteSpace(NumeroFactura)
            ? $"Pedido #{IdPedido}"
            : NumeroFactura;
    }

    public class ListadoPedidosAdminViewModel
    {
        public List<PedidoAdminResumen> Pedidos { get; set; } = new();

        public List<EstadoPedidoOpcion> Estados { get; set; } = new();

        public string? Busqueda { get; set; }

        public int? IdEstado { get; set; }

        // Llegan como yyyy-MM-dd desde los campos date
        public string? FechaInicio { get; set; }

        public string? FechaFin { get; set; }

        public int PaginaActual { get; set; } = 1;

        public int TotalPaginas { get; set; } = 1;
    }

    public class DetallePedidoAdminViewModel
    {
        public int IdPedido { get; set; }

        public string? NumeroFactura { get; set; }

        public DateTime FechaPedido { get; set; }

        public decimal Subtotal { get; set; }

        public decimal CostoEnvio { get; set; }

        public decimal Total { get; set; }

        public int IdEstadoPedido { get; set; }

        public string NombreEstado { get; set; } = string.Empty;

        public string ClaseCssEstado { get; set; } = string.Empty;

        public string? MotivoCancelacion { get; set; }

        public string NombreCliente { get; set; } = string.Empty;

        public string CorreoCliente { get; set; } = string.Empty;

        public string? TelefonoCliente { get; set; }

        public DireccionEnvio Direccion { get; set; } = new();

        public string? NombreMetodoPago { get; set; }

        public string? ComprobantePago { get; set; }

        public List<LineaPedido> Lineas { get; set; } = new();

        // Solo los estados a los que se puede pasar desde el actual. Los calcula
        // el servicio, no la vista: Pagado va a En preparacion o Cancelado,
        // En preparacion a Entregado o Cancelado, y los otros dos son finales
        public List<EstadoPedidoOpcion> EstadosSiguientes { get; set; } = new();

        public bool PuedeCambiarEstado => EstadosSiguientes.Count > 0;

        public string FacturaVisible => string.IsNullOrWhiteSpace(NumeroFactura)
            ? $"Pedido #{IdPedido}"
            : NumeroFactura;
    }

    public class PagoAdmin
    {
        public int IdPago { get; set; }

        public int IdPedido { get; set; }

        public string? NumeroFactura { get; set; }

        public string NombreCliente { get; set; } = string.Empty;

        public string CorreoCliente { get; set; } = string.Empty;

        public string NombreMetodoPago { get; set; } = string.Empty;

        public decimal Monto { get; set; }

        public DateTime FechaPago { get; set; }

        // Sin comprobante del cliente, SP_Pedido_Confirmar genera uno SIM-
        public string? Comprobante { get; set; }

        public int IdEstadoPago { get; set; }

        public string NombreEstado { get; set; } = string.Empty;

        // 3 es Anulado en TB_ESTADO_PAGO
        public bool EstaAnulado => IdEstadoPago == 3;
    }

    public class ListadoPagosViewModel
    {
        public List<PagoAdmin> Pagos { get; set; } = new();

        // Todos, anulados incluidos
        public int TotalPagos { get; set; }

        // Solo los que estan en Pagado
        public decimal TotalRecaudado { get; set; }

        public int PagosHoy { get; set; }

        public decimal RecaudadoHoy { get; set; }

        public int PagosAnulados { get; set; }

        public string? Busqueda { get; set; }

        public int? IdEstadoPago { get; set; }

        public string? FechaInicio { get; set; }

        public string? FechaFin { get; set; }

        public int PaginaActual { get; set; } = 1;

        public int TotalPaginas { get; set; } = 1;
    }

    public class FacturaAdmin
    {
        public int IdFactura { get; set; }

        public int IdPedido { get; set; }

        public string NumeroFactura { get; set; } = string.Empty;

        public DateTime FechaFactura { get; set; }

        public decimal Total { get; set; }

        public int IdEstadoFactura { get; set; }

        // Emitida o Anulada. No son los estados del pago
        public string NombreEstado { get; set; } = string.Empty;

        public string NombreCliente { get; set; } = string.Empty;

        public string CorreoCliente { get; set; } = string.Empty;

        public string NombreMetodoPago { get; set; } = string.Empty;

        public string? Comprobante { get; set; }

        // 2 es Anulada en TB_ESTADO_FACTURA
        public bool EstaAnulada => IdEstadoFactura == 2;
    }

    public class ListadoFacturasViewModel
    {
        public List<FacturaAdmin> Facturas { get; set; } = new();

        public string? Busqueda { get; set; }

        public string? FechaInicio { get; set; }

        public string? FechaFin { get; set; }

        public int PaginaActual { get; set; } = 1;

        public int TotalPaginas { get; set; } = 1;
    }

    // El procedimiento devuelve una fila por producto con el encabezado repetido
    // y el servicio lo agrupa en estas dos partes
    public class EncabezadoFactura
    {
        public int IdFactura { get; set; }

        public int IdPedido { get; set; }

        public string NumeroFactura { get; set; } = string.Empty;

        public DateTime FechaFactura { get; set; }

        public decimal Subtotal { get; set; }

        public decimal CostoEnvio { get; set; }

        public decimal Total { get; set; }

        public int IdEstadoFactura { get; set; }

        public string NombreEstado { get; set; } = string.Empty;

        public string NombreCliente { get; set; } = string.Empty;

        public string CorreoCliente { get; set; } = string.Empty;

        public string? TelefonoCliente { get; set; }

        // Opcional en TB_USUARIO: puede venir vacia
        public string? CedulaCliente { get; set; }

        public DireccionEnvio Direccion { get; set; } = new();

        public string? NombreMetodoPago { get; set; }

        public string? Comprobante { get; set; }

        public DateTime? FechaPago { get; set; }

        public bool EstaAnulada => IdEstadoFactura == 2;
    }

    public class DetalleFacturaViewModel
    {
        public EncabezadoFactura Encabezado { get; set; } = new();

        public List<LineaPedido> Lineas { get; set; } = new();
    }
}

namespace LEN_Web.Models
{
    // Carrito, checkout y confirmacion. Las tres comparten la idea de linea pero
    // no el tipo: la del carrito es editable y la del pedido ya esta congelada

    public class LineaCarrito
    {
        public int IdProducto { get; set; }

        public string NombreProducto { get; set; } = string.Empty;

        public string? RutaImagen { get; set; }

        public decimal PrecioUnitario { get; set; }

        public int Cantidad { get; set; }

        public int StockDisponible { get; set; }

        public bool EsPiezaUnica { get; set; }

        public decimal SubtotalLinea => PrecioUnitario * Cantidad;

        // Una linea deja de poderse comprar cuando se pidio mas de lo que queda
        public bool Disponible => Cantidad <= StockDisponible;

        // Una pieza unica nunca admite mas de una
        public int MaximoCantidad => EsPiezaUnica ? 1 : StockDisponible;
    }

    public class CarritoViewModel
    {
        public List<LineaCarrito> Lineas { get; set; } = new();

        // Suma de unidades, no de lineas: es lo que ve el globito de la cabecera
        public int CantidadItems => Lineas.Sum(linea => linea.Cantidad);

        public decimal Subtotal => Lineas.Sum(linea => linea.SubtotalLinea);

        public bool EstaVacio => Lineas.Count == 0;

        // Con una sola linea sin stock el pago queda bloqueado
        public bool TieneLineasNoDisponibles => Lineas.Any(linea => !linea.Disponible);
    }

    public class DireccionEnvio
    {
        public int IdDireccion { get; set; }

        public string NombreDestinatario { get; set; } = string.Empty;

        public string DireccionExacta { get; set; } = string.Empty;

        public string Distrito { get; set; } = string.Empty;

        public string Canton { get; set; } = string.Empty;

        public string Provincia { get; set; } = string.Empty;

        public string? TelefonoContacto { get; set; }

        public bool EsPrincipal { get; set; }

        public string? Referencia { get; set; }

        // Baja logica. La direccion inactiva sigue visible en EditarCuenta y lo
        // unico que pierde es el boton de desactivar
        public bool Activo { get; set; } = true;

        // Los tres hacen falta para rellenar la cascada al editar la direccion
        public int IdProvincia { get; set; }

        public int IdCanton { get; set; }

        public int IdDistrito { get; set; }

        public string Ubicacion => $"{Distrito}, {Canton}, {Provincia}";
    }

    public class MetodoPago
    {
        public int IdMetodoPago { get; set; }

        public string Nombre { get; set; } = string.Empty;
    }

    public class CheckoutViewModel
    {
        public List<LineaCarrito> Lineas { get; set; } = new();

        public List<DireccionEnvio> Direcciones { get; set; } = new();

        public List<MetodoPago> MetodosPago { get; set; } = new();

        // Lo calcula la aplicacion segun la provincia elegida. No esta en la base
        public decimal CostoEnvio { get; set; }

        public decimal Subtotal => Lineas.Sum(linea => linea.SubtotalLinea);

        public decimal Total => Subtotal + CostoEnvio;

        // Sin direcciones no se puede comprar y el boton queda deshabilitado
        public bool TieneDirecciones => Direcciones.Count > 0;
    }

    public class LineaPedido
    {
        public int IdProducto { get; set; }

        public string NombreProducto { get; set; } = string.Empty;

        public string? RutaImagen { get; set; }

        public int Cantidad { get; set; }

        // El precio congelado en TB_DETALLE_PEDIDO, no el actual del producto. Si el
        // precio sube maniana esta factura sigue diciendo lo mismo
        public decimal PrecioUnitario { get; set; }

        public decimal SubtotalLinea => PrecioUnitario * Cantidad;
    }

    public class ConfirmacionPedidoViewModel
    {
        public int IdPedido { get; set; }

        public string NumeroFactura { get; set; } = string.Empty;

        public DateTime FechaPedido { get; set; }

        public decimal Subtotal { get; set; }

        public decimal CostoEnvio { get; set; }

        public decimal Total { get; set; }

        // El cliente ve otras palabras que el panel: Pagado sale como Comprada
        public string EstadoVisible { get; set; } = string.Empty;

        // Sale de TB_ESTADO_PEDIDO.ClaseCss. La vista no decide el color
        public string ClaseCssEstado { get; set; } = string.Empty;

        public string NombreMetodoPago { get; set; } = string.Empty;

        public string? ComprobantePago { get; set; }

        public DireccionEnvio Direccion { get; set; } = new();

        public List<LineaPedido> Lineas { get; set; } = new();
    }
}

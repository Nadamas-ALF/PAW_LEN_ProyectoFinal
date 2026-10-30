namespace LEN_Web.Models
{
    // Reporte de ventas por rango de fechas. Los tres bloques salen de tres
    // procedimientos distintos con el mismo rango
    //
    // Cuentan como venta los pedidos Pagado, En preparacion o Entregado. Los
    // cancelados no suman

    public class ResumenReporte
    {
        public decimal TotalVentas { get; set; }

        public int CantidadPedidos { get; set; }

        // Dice si conviene subir precios o vender mas unidades
        public decimal TicketPromedio => CantidadPedidos == 0
            ? 0m
            : TotalVentas / CantidadPedidos;

        // Suma de unidades, no de lineas
        public int ProductosVendidos { get; set; }
    }

    public class VentaPorDia
    {
        public DateTime Fecha { get; set; }

        public int CantidadPedidos { get; set; }

        public decimal TotalVentas { get; set; }
    }

    public class ProductoVendido
    {
        public int IdProducto { get; set; }

        public string NombreProducto { get; set; } = string.Empty;

        public string NombreCategoria { get; set; } = string.Empty;

        public int UnidadesVendidas { get; set; }

        public decimal TotalVendido { get; set; }
    }

    public class ReporteVentasViewModel
    {
        // Las dos son obligatorias. Los procedimientos rechazan con THROW 50000
        // si falta una o si el inicio es posterior al fin
        public string FechaInicio { get; set; } = string.Empty;

        public string FechaFin { get; set; } = string.Empty;

        public ResumenReporte Resumen { get; set; } = new();

        // Solo los dias con ventas: un dia sin nada no sale como fila en cero
        public List<VentaPorDia> VentasPorDia { get; set; } = new();

        // De mayor a menor por unidades
        public List<ProductoVendido> ProductosMasVendidos { get; set; } = new();
    }
}

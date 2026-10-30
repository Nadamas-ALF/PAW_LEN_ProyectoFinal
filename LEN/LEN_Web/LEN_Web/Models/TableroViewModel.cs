namespace LEN_Web.Models
{
    // Panel principal del administrador. Son tres consultas distintas:
    // SP_Tablero_ConsultarResumen, SP_Tablero_ConsultarVentasMensuales y
    // SP_Tablero_ConsultarActividadReciente

    public class VentaMensual
    {
        // Las tres letras que se pintan debajo de la barra
        public string Mes { get; set; } = string.Empty;

        public decimal Total { get; set; }

        // Lo calcula el servicio contra el mes mas alto y lo redondea a pasos de
        // 5, con minimo 5 si hubo alguna venta: asi un mes flojo no se ve igual
        // que un mes en cero
        public int Porcentaje { get; set; }
    }

    public class ActividadReciente
    {
        public string Titulo { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        public string Tipo { get; set; } = string.Empty;

        // Sale de TB_ESTADO_PEDIDO.ClaseCss y pinta el punto
        public string ClaseCssEstado { get; set; } = string.Empty;
    }

    public class TableroViewModel
    {
        // Las ventas no cuentan pedidos cancelados
        public decimal TotalVentas { get; set; }

        public int TotalPedidos { get; set; }

        // Pagados que nadie ha empezado a preparar
        public int PedidosPorPreparar { get; set; }

        public int TotalClientesActivos { get; set; }

        // Activos con stock en cero
        public int TotalProductosAgotados { get; set; }

        // Siempre vienen los doce meses, con cero donde no hubo ventas
        public List<VentaMensual> VentasMensuales { get; set; } = new();

        public List<ActividadReciente> ActividadesRecientes { get; set; } = new();
    }
}

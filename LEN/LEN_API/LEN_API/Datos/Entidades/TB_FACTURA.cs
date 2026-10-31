using System;
using System.Collections.Generic;

namespace LEN_API.Datos.Entidades;

public partial class TB_FACTURA
{
    public int IdFactura { get; set; }

    public int IdPedido { get; set; }

    public int IdEstadoFactura { get; set; }

    public string NumeroFactura { get; set; } = null!;

    public DateTime FechaFactura { get; set; }

    public decimal Subtotal { get; set; }

    public decimal CostoEnvio { get; set; }

    public decimal Total { get; set; }

    public virtual TB_ESTADO_FACTURA IdEstadoFacturaNavigation { get; set; } = null!;

    public virtual TB_PEDIDO IdPedidoNavigation { get; set; } = null!;
}

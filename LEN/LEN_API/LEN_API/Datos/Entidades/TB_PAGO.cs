using System;
using System.Collections.Generic;

namespace LEN_API.Datos.Entidades;

public partial class TB_PAGO
{
    public int IdPago { get; set; }

    public int IdPedido { get; set; }

    public int IdMetodoPago { get; set; }

    public int IdEstadoPago { get; set; }

    public decimal Monto { get; set; }

    public DateTime FechaPago { get; set; }

    public string? Comprobante { get; set; }

    public virtual TB_ESTADO_PAGO IdEstadoPagoNavigation { get; set; } = null!;

    public virtual TB_METODO_PAGO IdMetodoPagoNavigation { get; set; } = null!;

    public virtual TB_PEDIDO IdPedidoNavigation { get; set; } = null!;
}

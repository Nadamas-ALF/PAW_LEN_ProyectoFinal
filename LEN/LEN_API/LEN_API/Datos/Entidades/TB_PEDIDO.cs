using System;
using System.Collections.Generic;

namespace LEN_API.Datos.Entidades;

public partial class TB_PEDIDO
{
    public int IdPedido { get; set; }

    public int IdUsuario { get; set; }

    public int IdDireccion { get; set; }

    public int IdEstadoPedido { get; set; }

    public DateTime FechaPedido { get; set; }

    public decimal Subtotal { get; set; }

    public decimal CostoEnvio { get; set; }

    public decimal Total { get; set; }

    public string? MotivoCancelacion { get; set; }

    public virtual TB_DIRECCION_ENVIO IdDireccionNavigation { get; set; } = null!;

    public virtual TB_ESTADO_PEDIDO IdEstadoPedidoNavigation { get; set; } = null!;

    public virtual TB_USUARIO IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<TB_DETALLE_PEDIDO> TB_DETALLE_PEDIDO { get; set; } = new List<TB_DETALLE_PEDIDO>();

    public virtual TB_FACTURA? TB_FACTURA { get; set; }

    public virtual ICollection<TB_PAGO> TB_PAGO { get; set; } = new List<TB_PAGO>();
}

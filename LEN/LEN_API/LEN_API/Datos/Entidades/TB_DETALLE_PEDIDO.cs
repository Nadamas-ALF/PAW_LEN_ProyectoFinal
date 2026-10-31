using System;
using System.Collections.Generic;

namespace LEN_API.Datos.Entidades;

public partial class TB_DETALLE_PEDIDO
{
    public int IdDetallePedido { get; set; }

    public int IdPedido { get; set; }

    public int IdProducto { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal SubtotalLinea { get; set; }

    public virtual TB_PEDIDO IdPedidoNavigation { get; set; } = null!;

    public virtual TB_PRODUCTO IdProductoNavigation { get; set; } = null!;
}

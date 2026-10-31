using System;
using System.Collections.Generic;

namespace LEN_API.Datos.Entidades;

public partial class TB_DETALLE_CARRITO
{
    public int IdDetalleCarrito { get; set; }

    public int IdCarrito { get; set; }

    public int IdProducto { get; set; }

    public int Cantidad { get; set; }

    public virtual TB_CARRITO IdCarritoNavigation { get; set; } = null!;

    public virtual TB_PRODUCTO IdProductoNavigation { get; set; } = null!;
}

using System;
using System.Collections.Generic;

namespace LEN_API.Datos.Entidades;

public partial class TB_PRODUCTO
{
    public int IdProducto { get; set; }

    public int IdCategoria { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public int Stock { get; set; }

    public bool EsPiezaUnica { get; set; }

    public bool Destacado { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual TB_CATEGORIA IdCategoriaNavigation { get; set; } = null!;

    public virtual ICollection<TB_DETALLE_CARRITO> TB_DETALLE_CARRITO { get; set; } = new List<TB_DETALLE_CARRITO>();

    public virtual ICollection<TB_DETALLE_PEDIDO> TB_DETALLE_PEDIDO { get; set; } = new List<TB_DETALLE_PEDIDO>();

    public virtual ICollection<TB_FAVORITO> TB_FAVORITO { get; set; } = new List<TB_FAVORITO>();

    public virtual TB_IMAGEN_PRODUCTO? TB_IMAGEN_PRODUCTO { get; set; }

    public virtual ICollection<TB_VARIANTE_PRODUCTO> TB_VARIANTE_PRODUCTO { get; set; } = new List<TB_VARIANTE_PRODUCTO>();
}

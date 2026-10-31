using System;
using System.Collections.Generic;

namespace LEN_API.Datos.Entidades;

public partial class TB_CARRITO
{
    public int IdCarrito { get; set; }

    public int IdUsuario { get; set; }

    public DateTime FechaCreacion { get; set; }

    public bool Activo { get; set; }

    public virtual TB_USUARIO IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<TB_DETALLE_CARRITO> TB_DETALLE_CARRITO { get; set; } = new List<TB_DETALLE_CARRITO>();
}

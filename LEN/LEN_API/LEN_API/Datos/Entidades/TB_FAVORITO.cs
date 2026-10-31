using System;
using System.Collections.Generic;

namespace LEN_API.Datos.Entidades;

public partial class TB_FAVORITO
{
    public int IdFavorito { get; set; }

    public int IdUsuario { get; set; }

    public int IdProducto { get; set; }

    public DateTime FechaAgregado { get; set; }

    public virtual TB_PRODUCTO IdProductoNavigation { get; set; } = null!;

    public virtual TB_USUARIO IdUsuarioNavigation { get; set; } = null!;
}

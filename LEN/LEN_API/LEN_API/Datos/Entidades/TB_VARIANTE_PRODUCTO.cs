using System;
using System.Collections.Generic;

namespace LEN_API.Datos.Entidades;

public partial class TB_VARIANTE_PRODUCTO
{
    public int IdVariante { get; set; }

    public int IdProducto { get; set; }

    public string? Color { get; set; }

    public string? Talla { get; set; }

    public string? Modelo { get; set; }

    public int Stock { get; set; }

    public bool Activo { get; set; }

    public virtual TB_PRODUCTO IdProductoNavigation { get; set; } = null!;
}

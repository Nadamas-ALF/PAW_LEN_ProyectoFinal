using System;
using System.Collections.Generic;

namespace LEN_API.Datos.Entidades;

public partial class TB_IMAGEN_PRODUCTO
{
    public int IdImagen { get; set; }

    public int IdProducto { get; set; }

    public string RutaImagen { get; set; } = null!;

    public string? TextoAlternativo { get; set; }

    public bool EsPrincipal { get; set; }

    public bool Activo { get; set; }

    public virtual TB_PRODUCTO IdProductoNavigation { get; set; } = null!;
}

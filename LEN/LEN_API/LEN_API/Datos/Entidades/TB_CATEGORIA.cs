using System;
using System.Collections.Generic;

namespace LEN_API.Datos.Entidades;

public partial class TB_CATEGORIA
{
    public int IdCategoria { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<TB_PRODUCTO> TB_PRODUCTO { get; set; } = new List<TB_PRODUCTO>();
}

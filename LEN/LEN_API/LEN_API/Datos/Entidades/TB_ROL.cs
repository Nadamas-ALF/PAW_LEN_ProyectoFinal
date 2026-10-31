using System;
using System.Collections.Generic;

namespace LEN_API.Datos.Entidades;

public partial class TB_ROL
{
    public int IdRol { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<TB_USUARIO> TB_USUARIO { get; set; } = new List<TB_USUARIO>();
}

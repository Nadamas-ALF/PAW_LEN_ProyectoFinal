using System;
using System.Collections.Generic;

namespace LEN_API.Datos.Entidades;

public partial class TB_PROVINCIA
{
    public int IdProvincia { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<TB_CANTON> TB_CANTON { get; set; } = new List<TB_CANTON>();
}

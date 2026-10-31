using System;
using System.Collections.Generic;

namespace LEN_API.Datos.Entidades;

public partial class TB_CANTON
{
    public int IdCanton { get; set; }

    public int IdProvincia { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual TB_PROVINCIA IdProvinciaNavigation { get; set; } = null!;

    public virtual ICollection<TB_DISTRITO> TB_DISTRITO { get; set; } = new List<TB_DISTRITO>();
}

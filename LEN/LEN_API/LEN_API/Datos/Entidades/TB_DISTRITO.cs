using System;
using System.Collections.Generic;

namespace LEN_API.Datos.Entidades;

public partial class TB_DISTRITO
{
    public int IdDistrito { get; set; }

    public int IdCanton { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual TB_CANTON IdCantonNavigation { get; set; } = null!;

    public virtual ICollection<TB_DIRECCION_ENVIO> TB_DIRECCION_ENVIO { get; set; } = new List<TB_DIRECCION_ENVIO>();
}

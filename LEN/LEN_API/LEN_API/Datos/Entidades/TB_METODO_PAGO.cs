using System;
using System.Collections.Generic;

namespace LEN_API.Datos.Entidades;

public partial class TB_METODO_PAGO
{
    public int IdMetodoPago { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? EtiquetaDatoPago { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<TB_PAGO> TB_PAGO { get; set; } = new List<TB_PAGO>();
}

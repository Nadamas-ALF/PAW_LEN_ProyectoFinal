using System;
using System.Collections.Generic;

namespace LEN_API.Datos.Entidades;

public partial class TB_ESTADO_PAGO
{
    public int IdEstadoPago { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<TB_PAGO> TB_PAGO { get; set; } = new List<TB_PAGO>();
}

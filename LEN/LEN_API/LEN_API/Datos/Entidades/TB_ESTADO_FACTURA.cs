using System;
using System.Collections.Generic;

namespace LEN_API.Datos.Entidades;

public partial class TB_ESTADO_FACTURA
{
    public int IdEstadoFactura { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<TB_FACTURA> TB_FACTURA { get; set; } = new List<TB_FACTURA>();
}

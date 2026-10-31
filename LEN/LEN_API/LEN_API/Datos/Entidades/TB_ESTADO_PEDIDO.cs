using System;
using System.Collections.Generic;

namespace LEN_API.Datos.Entidades;

public partial class TB_ESTADO_PEDIDO
{
    public int IdEstadoPedido { get; set; }

    public string Nombre { get; set; } = null!;

    public int Orden { get; set; }

    public string? ClaseCss { get; set; }

    public virtual ICollection<TB_PEDIDO> TB_PEDIDO { get; set; } = new List<TB_PEDIDO>();
}

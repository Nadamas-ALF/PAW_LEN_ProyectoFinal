using System;
using System.Collections.Generic;

namespace LEN_API.Datos.Entidades;

public partial class TB_BITACORA
{
    public long IdBitacora { get; set; }

    public int? IdUsuario { get; set; }

    public string Entidad { get; set; } = null!;

    public int? IdRegistro { get; set; }

    public string Accion { get; set; } = null!;

    public string? ValorAnterior { get; set; }

    public string? ValorNuevo { get; set; }

    public DateTime FechaHora { get; set; }

    public virtual TB_USUARIO? IdUsuarioNavigation { get; set; }
}

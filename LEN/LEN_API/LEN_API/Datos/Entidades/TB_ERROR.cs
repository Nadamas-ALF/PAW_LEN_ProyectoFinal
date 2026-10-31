using System;
using System.Collections.Generic;

namespace LEN_API.Datos.Entidades;

public partial class TB_ERROR
{
    public int IdError { get; set; }

    public DateTime FechaError { get; set; }

    public string? Origen { get; set; }

    public string? Metodo { get; set; }

    public string MensajeError { get; set; } = null!;

    public string? DetalleError { get; set; }

    public int? LineaError { get; set; }

    public string? UsuarioSistema { get; set; }

    public string? Url { get; set; }

    public string? RastroPila { get; set; }
}

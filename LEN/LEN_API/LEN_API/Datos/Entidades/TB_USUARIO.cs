using System;
using System.Collections.Generic;

namespace LEN_API.Datos.Entidades;

public partial class TB_USUARIO
{
    public int IdUsuario { get; set; }

    public int IdRol { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string? Cedula { get; set; }

    public string? Telefono { get; set; }

    public string Correo { get; set; } = null!;

    public string ContrasenaHash { get; set; } = null!;

    public bool TieneContrasenaTemporal { get; set; }

    public DateTime? VigenciaContrasenaTemporal { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual TB_ROL IdRolNavigation { get; set; } = null!;

    public virtual ICollection<TB_BITACORA> TB_BITACORA { get; set; } = new List<TB_BITACORA>();

    public virtual TB_CARRITO? TB_CARRITO { get; set; }

    public virtual TB_DIRECCION_ENVIO? TB_DIRECCION_ENVIO { get; set; }

    public virtual ICollection<TB_FAVORITO> TB_FAVORITO { get; set; } = new List<TB_FAVORITO>();

    public virtual ICollection<TB_PEDIDO> TB_PEDIDO { get; set; } = new List<TB_PEDIDO>();
}

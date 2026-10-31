using System;
using System.Collections.Generic;

namespace LEN_API.Datos.Entidades;

public partial class TB_DIRECCION_ENVIO
{
    public int IdDireccion { get; set; }

    public int IdUsuario { get; set; }

    public int IdDistrito { get; set; }

    public string NombreDestinatario { get; set; } = null!;

    public string DireccionExacta { get; set; } = null!;

    public string? Referencia { get; set; }

    public string TelefonoContacto { get; set; } = null!;

    public bool EsPrincipal { get; set; }

    public bool Activo { get; set; }

    public virtual TB_DISTRITO IdDistritoNavigation { get; set; } = null!;

    public virtual TB_USUARIO IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<TB_PEDIDO> TB_PEDIDO { get; set; } = new List<TB_PEDIDO>();
}

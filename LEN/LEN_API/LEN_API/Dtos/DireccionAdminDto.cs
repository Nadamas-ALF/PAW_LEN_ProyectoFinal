namespace LEN_API.Dtos;

public class DireccionAdminDto
{
    public int IdDireccion { get; set; }

    public int IdUsuario { get; set; }

    public int IdDistrito { get; set; }

    public string NombreProvincia { get; set; } = string.Empty;

    public string NombreCanton { get; set; } = string.Empty;

    public string NombreDistrito { get; set; } = string.Empty;

    public string NombreDestinatario { get; set; } = string.Empty;

    public string DireccionExacta { get; set; } = string.Empty;

    public string? Referencia { get; set; }

    public string TelefonoContacto { get; set; } = string.Empty;

    public bool EsPrincipal { get; set; }

    public bool Activo { get; set; }

    public int IdProvincia { get; set; }
    public int IdCanton { get; set; }
}
namespace LEN_API.Dtos;

public class UsuarioAdminDto
{
    public int IdUsuario { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string? Cedula { get; set; }

    public string? Telefono { get; set; }

    public string Correo { get; set; } = string.Empty;

    public int IdRol { get; set; }

    public string NombreRol { get; set; } = string.Empty;

    public bool Activo { get; set; }

    public DateTime FechaRegistro { get; set; }
}
namespace LEN_API.Dtos;

// Lo que el Web necesita para armar la cookie: el token y los datos de la cabecera.
// Nunca lleva el hash ni nada de la contrasena
public class SesionDto
{
    public string Token { get; set; } = string.Empty;

    public DateTime Expira { get; set; }

    public int IdUsuario { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public int IdRol { get; set; }

    public string NombreRol { get; set; } = string.Empty;

    public bool TieneContrasenaTemporal { get; set; }
}

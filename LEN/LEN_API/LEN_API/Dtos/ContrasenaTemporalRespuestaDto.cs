namespace LEN_API.Dtos;

public class ContrasenaTemporalRespuestaDto
{
    public DateTime Vigencia { get; set; }

    // False si la contrasena quedo asignada pero el correo no salio. El administrador esta
    // presente y puede dictarla, por eso se le dice (DEC-L-03)
    public bool CorreoEnviado { get; set; }

    // Solo viene cuando la genero el API, y es la unica vez que existe en claro fuera del correo
    public string? ContrasenaGenerada { get; set; }
}

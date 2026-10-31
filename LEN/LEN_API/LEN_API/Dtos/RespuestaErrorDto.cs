namespace LEN_API.Dtos;

// Cuerpo de todo 500. CodigoReferencia es el IdError de TB_ERROR; viene nulo
// solo si ni siquiera se pudo escribir el registro.
public class RespuestaErrorDto
{
    public string Mensaje { get; set; } = string.Empty;
    public int? CodigoReferencia { get; set; }
}

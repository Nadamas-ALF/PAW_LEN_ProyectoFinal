namespace LEN_API.Infraestructura;

public interface IRegistroErrores
{
    // Escribe TB_ERROR y devuelve el IdError, o null si no se pudo escribir.
    Task<int?> Registrar(Exception excepcion, string url, string? usuario);
}

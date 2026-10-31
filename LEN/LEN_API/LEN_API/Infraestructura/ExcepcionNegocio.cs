namespace LEN_API.Infraestructura;

// Una regla de negocio que se valida en C# en vez de en un procedimiento. El middleware
// la responde como 400 con este mensaje, igual que un THROW 50000. El mensaje lo lee una persona
public class ExcepcionNegocio : Exception
{
    public ExcepcionNegocio(string mensaje) : base(mensaje)
    {
    }
}

namespace LEN_API.Infraestructura;

public interface IServicioCorreo
{
    // Envia un correo de texto plano y espera a que el servidor lo acepte. Si no sale,
    // lanza: quien llama decide que hacer (el orden de cada flujo esta en DEC-L-03)
    Task Enviar(string destino, string asunto, string cuerpo);
}

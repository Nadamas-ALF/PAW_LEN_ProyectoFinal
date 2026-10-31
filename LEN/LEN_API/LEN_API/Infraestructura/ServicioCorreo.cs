using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;

namespace LEN_API.Infraestructura;

public class ServicioCorreo : IServicioCorreo
{
    private readonly OpcionesCorreo _opciones;
    private readonly IWebHostEnvironment _entorno;
    private readonly ILogger<ServicioCorreo> _log;

    public ServicioCorreo(IOptions<OpcionesCorreo> opciones, IWebHostEnvironment entorno, ILogger<ServicioCorreo> log)
    {
        _opciones = opciones.Value;
        _entorno = entorno;
        _log = log;
    }

    public async Task Enviar(string destino, string asunto, string cuerpo)
    {
        if (!_opciones.EstaConfigurado)
        {
            // Sin cuenta de envio solo se tolera en desarrollo. Escribe el cuerpo, que lleva
            // la contrasena temporal, y es la unica excepcion a no ponerla en un log (DEC-L-03)
            if (!_entorno.IsDevelopment())
                throw new InvalidOperationException("Falta configurar la seccion Correo.");

            _log.LogWarning("Correo no configurado. Se habria enviado a {Destino}:\n{Asunto}\n{Cuerpo}", destino, asunto, cuerpo);
            return;
        }

        using var mensaje = new MailMessage(_opciones.Remitente, destino)
        {
            Subject = asunto,
            Body = cuerpo,
            IsBodyHtml = false,
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8
        };

        using var cliente = new SmtpClient(_opciones.Servidor, _opciones.Puerto)
        {
            EnableSsl = _opciones.UsarSsl,
            Timeout = 10_000
        };

        if (!string.IsNullOrEmpty(_opciones.Usuario))
            cliente.Credentials = new NetworkCredential(_opciones.Usuario, _opciones.Contrasena);

        await cliente.SendMailAsync(mensaje);
    }
}

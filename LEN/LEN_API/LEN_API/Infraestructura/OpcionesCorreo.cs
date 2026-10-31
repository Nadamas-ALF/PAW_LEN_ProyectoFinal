namespace LEN_API.Infraestructura;

// Seccion "Correo" de la configuracion. Los valores reales viven en User Secrets
public class OpcionesCorreo
{
    // Si la contrasena sigue empezando asi, nadie configuro la cuenta de envio
    public const string PrefijoPlantilla = "FALSO-";

    public string Servidor { get; set; } = string.Empty;

    public int Puerto { get; set; } = 587;

    public string Remitente { get; set; } = string.Empty;

    public string Usuario { get; set; } = string.Empty;

    public string Contrasena { get; set; } = string.Empty;

    public bool UsarSsl { get; set; } = true;

    // Enlace a la pantalla de inicio de sesion del Web, que va dentro del correo
    public string UrlInicioSesion { get; set; } = string.Empty;

    public bool EstaConfigurado =>
        !string.IsNullOrWhiteSpace(Servidor)
        && !Contrasena.StartsWith(PrefijoPlantilla, StringComparison.Ordinal);
}

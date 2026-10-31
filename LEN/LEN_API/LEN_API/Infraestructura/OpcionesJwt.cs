namespace LEN_API.Infraestructura;

// Seccion "Jwt" de la configuracion. La llave real vive en User Secrets
public class OpcionesJwt
{
    // Si la llave sigue empezando asi, nadie la reemplazo y el API no arranca
    public const string PrefijoPlantilla = "LLAVE-DE-PLANTILLA";

    public string Llave { get; set; } = string.Empty;

    public string Emisor { get; set; } = "LEN_API";

    public string Audiencia { get; set; } = "LEN_Web";

    public int Minutos { get; set; } = 60;

    public void Validar()
    {
        if (Llave.StartsWith(PrefijoPlantilla, StringComparison.Ordinal))
            throw new InvalidOperationException("Falta Jwt:Llave. Ver CONTRATO_API.md, seccion 5.");

        if (Llave.Length < 32)
            throw new InvalidOperationException("Jwt:Llave tiene que tener al menos 32 caracteres.");
    }
}

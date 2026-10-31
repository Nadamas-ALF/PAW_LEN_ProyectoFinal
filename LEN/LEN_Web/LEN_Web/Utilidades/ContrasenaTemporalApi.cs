namespace LEN_Web.Utilidades
{
    // Lo que devuelve api/usuarios/{id}/contrasena-temporal
    public class ContrasenaTemporalApi
    {
        public DateTime Vigencia { get; set; }

        public bool CorreoEnviado { get; set; }

        // Solo viene cuando la genero el API
        public string? ContrasenaGenerada { get; set; }
    }
}

namespace LEN_Web.Utilidades
{
    // Lo que devuelve api/acceso/iniciar-sesion. Es el contrato del API, no una entidad del Web
    public class SesionApi
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
}

namespace LEN_Web.Models
{
    // La cuenta del cliente: lo que MiCuenta muestra de solo lectura y lo que
    // EditarCuenta cambia. El cliente no manda rol ni estado: esos los toma el
    // servicio de la base

    public class UsuarioCuenta
    {
        public int IdUsuario { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public string? Telefono { get; set; }

        public string? Cedula { get; set; }

        public DateTime FechaRegistro { get; set; }

        public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
    }

    public class CuentaViewModel
    {
        public UsuarioCuenta Usuario { get; set; } = new();

        // Solo las activas, que es lo que devuelve SP_Direccion_ListarPorUsuario
        // con SoloActivas en 1
        public List<DireccionEnvio> Direcciones { get; set; } = new();
    }

    public class OpcionUbicacion
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;
    }

    public class EditarCuentaViewModel
    {
        public UsuarioCuenta Usuario { get; set; } = new();

        // Aqui si vienen las inactivas: se siguen pudiendo editar, lo que no se
        // puede es volver a desactivarlas
        public List<DireccionEnvio> Direcciones { get; set; } = new();

        public List<OpcionUbicacion> Provincias { get; set; } = new();
    }
}

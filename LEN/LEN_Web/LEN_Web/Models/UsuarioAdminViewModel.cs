namespace LEN_Web.Models
{
    // Las cuentas del sistema vistas por el administrador. Son las internas y
    // las de clientes en la misma tabla, que es como estan en TB_USUARIO

    public class RolOpcion
    {
        public int IdRol { get; set; }

        public string Nombre { get; set; } = string.Empty;
    }

    public class UsuarioAdmin
    {
        public int IdUsuario { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        // Los dos son opcionales en TB_USUARIO
        public string? Cedula { get; set; }

        public string? Telefono { get; set; }

        public int IdRol { get; set; }

        public string NombreRol { get; set; } = string.Empty;

        public bool Activo { get; set; }

        public DateTime FechaRegistro { get; set; }

        public string NombreCompleto => $"{Nombre} {Apellido}".Trim();

        // 1 es Administrador en TB_ROL
        public bool EsAdministrador => IdRol == 1;

        // Las iniciales del avatar. Van aca y no en la vista: una vista no
        // deberia estar partiendo cadenas
        public string Iniciales
        {
            get
            {
                var primera = Nombre.Length > 0 ? Nombre[0] : ' ';
                var segunda = Apellido.Length > 0 ? Apellido[0] : ' ';
                return $"{primera}{segunda}".Trim().ToUpperInvariant();
            }
        }
    }

    public class ListadoUsuariosViewModel
    {
        public List<UsuarioAdmin> Usuarios { get; set; } = new();

        public List<RolOpcion> Roles { get; set; } = new();

        public string? Busqueda { get; set; }

        public int? IdRol { get; set; }

        public bool? Activo { get; set; }

        public int PaginaActual { get; set; } = 1;

        public int TotalPaginas { get; set; } = 1;

        // Los cuatro indicadores. Miran toda la historia, no la pagina abierta
        public int TotalUsuarios { get; set; }

        public int TotalActivos { get; set; }

        public int TotalInactivos { get; set; }

        public int TotalAdministradores { get; set; }
    }

    public class DetalleUsuarioViewModel
    {
        public UsuarioAdmin Usuario { get; set; } = new();

        // Todas, activas e inactivas: el procedimiento se llama con SoloActivas
        // en 0 porque el administrador tiene que poder ver las dadas de baja
        public List<DireccionEnvio> Direcciones { get; set; } = new();

        public List<OpcionUbicacion> Provincias { get; set; } = new();

        public List<RolOpcion> Roles { get; set; } = new();

        // Al ultimo administrador activo no se le puede cambiar el rol ni
        // desactivar la cuenta: el procedimiento responde con THROW 50000. La
        // pantalla lo avisa de antemano
        public bool EsUnicoAdministradorActivo { get; set; }
    }
}

using LEN_API.Dtos;


namespace LEN_API.Servicios;

public interface IUsuarioServicio
{
    // HU-USR-005. Asigna una contrasena temporal a otro usuario y despues intenta mandarle el
    // correo. Si el envio falla la contrasena queda asignada y se avisa en la respuesta
    Task<ContrasenaTemporalRespuestaDto> AsignarContrasenaTemporal(int idAdministrador, int idUsuario, ContrasenaTemporalDto datos);

    Task<List<UsuarioAdminDto>> Listar();

    Task<int> RegistrarDesdeAdministracion(
    int idUsuarioAccion,
    CrearUsuarioAdminDto datos);

    Task ActualizarDesdeAdministracion(
    int idUsuarioAccion,
    int idUsuario,
    ActualizarUsuarioAdminDto datos);


    Task CambiarActivo(int idUsuarioAccion, int idUsuario);

    // BT-02. La cuenta del propio cliente, o null si ya no existe
    Task<UsuarioAdminDto?> ObtenerCuenta(int idUsuario);

    // BT-02. El cliente cambia sus datos personales. Rol y estado no se tocan
    Task ActualizarCuenta(int idUsuario, ActualizarCuentaDto datos);
}

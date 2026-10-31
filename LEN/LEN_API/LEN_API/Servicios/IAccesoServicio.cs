using LEN_API.Dtos;

namespace LEN_API.Servicios;

public interface IAccesoServicio
{
    // Calcula el hash y registra un cliente. Devuelve el IdUsuario
    Task<int> Registrar(RegistroDto datos);

    // Devuelve la sesion, o null si el correo o la contrasena no corresponden.
    // Un usuario inactivo y uno inexistente dan el mismo null (RQNF-SEC-005)
    Task<SesionDto?> IniciarSesion(IniciarSesionDto datos);
}

using LEN_API.Dtos;

namespace LEN_API.Servicios;

public interface IAccesoServicio
{
    // Calcula el hash y registra un cliente. Devuelve el IdUsuario
    Task<int> Registrar(RegistroDto datos);

    // Devuelve la sesion, o null si el correo o la contrasena no corresponden, la cuenta esta
    // inactiva o la temporal esta vencida. Todos dan el mismo null (RQNF-SEC-005)
    Task<SesionDto?> IniciarSesion(IniciarSesionDto datos);

    // HU-ACC-004. No devuelve nada a proposito: exista el correo o no, falle el envio o no,
    // el que llama responde lo mismo. Un fallo de envio queda en TB_ERROR y no cambia nada
    Task Recuperar(string correo);

    // HU-ACC-006 y HU-CLI-003. Devuelve una sesion nueva: el token viejo todavia dice que la
    // contrasena es temporal
    Task<SesionDto> CambiarContrasena(int idUsuario, CambiarContrasenaDto datos);
}

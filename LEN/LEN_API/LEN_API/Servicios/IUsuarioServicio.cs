using LEN_API.Dtos;

namespace LEN_API.Servicios;

public interface IUsuarioServicio
{
    // HU-USR-005. Asigna una contrasena temporal a otro usuario y despues intenta mandarle el
    // correo. Si el envio falla la contrasena queda asignada y se avisa en la respuesta
    Task<ContrasenaTemporalRespuestaDto> AsignarContrasenaTemporal(int idAdministrador, int idUsuario, ContrasenaTemporalDto datos);
}

using LEN_API.Datos.Entidades;

namespace LEN_API.Infraestructura;

public interface IEmisorToken
{
    // Firma un JWT con los claims del contrato y devuelve cuando vence
    (string Token, DateTime Expira) Emitir(TB_USUARIO usuario);
}

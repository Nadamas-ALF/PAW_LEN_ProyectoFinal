using LEN_API.Dtos;

namespace LEN_API.Servicios;

public interface IDiagnosticoServicio
{
    Task<EstadoBaseDto> ObtenerEstado();
    Task ProvocarErrorDeNegocio();
    Task ProvocarErrorInterno();
    Task<string> EnviarCorreoDePrueba(string destino);
}

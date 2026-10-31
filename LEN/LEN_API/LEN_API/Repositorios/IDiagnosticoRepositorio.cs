using LEN_API.Dtos;

namespace LEN_API.Repositorios;

public interface IDiagnosticoRepositorio
{
    Task<EstadoBaseDto> ObtenerEstado();
    Task ProvocarErrorDeNegocio();
    Task ProvocarErrorInterno();
}

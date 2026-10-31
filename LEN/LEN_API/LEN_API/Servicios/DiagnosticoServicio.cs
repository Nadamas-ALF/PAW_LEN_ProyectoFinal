using LEN_API.Dtos;
using LEN_API.Repositorios;

namespace LEN_API.Servicios;

public class DiagnosticoServicio : IDiagnosticoServicio
{
    private readonly IDiagnosticoRepositorio _repositorio;

    public DiagnosticoServicio(IDiagnosticoRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public Task<EstadoBaseDto> ObtenerEstado() => _repositorio.ObtenerEstado();
    public Task ProvocarErrorDeNegocio() => _repositorio.ProvocarErrorDeNegocio();
    public Task ProvocarErrorInterno() => _repositorio.ProvocarErrorInterno();
}

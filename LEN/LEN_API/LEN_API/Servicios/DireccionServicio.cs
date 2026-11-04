
using LEN_API.Dtos;
using LEN_API.Repositorios;

namespace LEN_API.Servicios;

public class DireccionServicio : IDireccionServicio
{
    private readonly IDireccionRepositorio _repositorio;

    public DireccionServicio(IDireccionRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<List<DireccionAdminDto>> ListarPorUsuario(
        int idUsuario,
        bool incluirInactivas)
    {
        return await _repositorio.ListarPorUsuario(
            idUsuario,
            incluirInactivas);
    }

    public async Task<DireccionAdminDto?> ObtenerPorId(
        int idDireccion,
        int idUsuario)
    {
        return await _repositorio.ObtenerPorId(
            idDireccion,
            idUsuario);
    }

    public async Task<List<ProvinciaDto>> ListarProvincias()
    {
        return await _repositorio.ListarProvincias();
    }

    public async Task<List<CantonDto>> ListarCantones(int idProvincia)
    {
        return await _repositorio.ListarCantones(idProvincia);
    }

    public async Task<List<DistritoDto>> ListarDistritos(int idCanton)
    {
        return await _repositorio.ListarDistritos(idCanton);
    }

    public async Task<int> Registrar(
    int idUsuarioAccion,
    CrearDireccionDto datos)
    {
        return await _repositorio.Registrar(
            idUsuarioAccion,
            datos);
    }

    public async Task Actualizar(
        int idUsuarioAccion,
        int idDireccion,
        int idUsuario,
        ActualizarDireccionDto datos)
    {
        await _repositorio.Actualizar(
            idUsuarioAccion,
            idDireccion,
            idUsuario,
            datos);
    }

    public async Task Desactivar(
        int idUsuarioAccion,
        int idDireccion,
        int idUsuario)
    {
        await _repositorio.Desactivar(
            idUsuarioAccion,
            idDireccion,
            idUsuario);
    }

}

using LEN_API.Dtos;

namespace LEN_API.Repositorios;

public interface IDireccionRepositorio
{
    Task<List<DireccionAdminDto>> ListarPorUsuario(
        int idUsuario,
        bool incluirInactivas);

    Task<DireccionAdminDto?> ObtenerPorId(
        int idDireccion,
        int idUsuario);

    Task<List<ProvinciaDto>> ListarProvincias();

    Task<List<CantonDto>> ListarCantones(int idProvincia);

    Task<List<DistritoDto>> ListarDistritos(int idCanton);

    Task<int> Registrar(
    int idUsuarioAccion,
    CrearDireccionDto datos);

    Task Actualizar(
        int idUsuarioAccion,
        int idDireccion,
        int idUsuario,
        ActualizarDireccionDto datos);

    Task Desactivar(
        int idUsuarioAccion,
        int idDireccion,
        int idUsuario);

    // BT-02. SP_Direccion_MarcarPrincipal
    Task MarcarPrincipal(
        int idUsuarioAccion,
        int idDireccion,
        int idUsuario);
}

using LEN_API.Dtos;

namespace LEN_API.Servicios;

public interface ICategoriaServicio
{
    Task<List<CategoriaAdminDto>> Listar();

    // BT-05. Solo las activas, para la tienda (HU-PUB-007)
    Task<List<CategoriaAdminDto>> ListarActivas();

    Task<int> Registrar(int idUsuarioAccion, CategoriaRegistrarDto datos);


    Task Actualizar(
        int idUsuarioAccion,
        int idCategoria,
        CategoriaActualizarDto datos);


    Task CambiarActivo(int idUsuarioAccion, int idCategoria);


}

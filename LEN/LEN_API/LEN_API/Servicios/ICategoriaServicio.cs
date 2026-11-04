
using LEN_API.Dtos;

namespace LEN_API.Servicios;

public interface ICategoriaServicio
{
    Task<List<CategoriaAdminDto>> Listar();

    Task<int> Registrar(int idUsuarioAccion, CategoriaRegistrarDto datos);


    Task Actualizar(
        int idUsuarioAccion,
        int idCategoria,
        CategoriaActualizarDto datos);


    Task CambiarActivo(int idUsuarioAccion, int idCategoria);


}

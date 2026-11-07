
using LEN_API.Dtos;

namespace LEN_API.Repositorios;

public interface ICategoriaRepositorio
{
    Task<List<CategoriaAdminDto>> Listar();

    Task<int> Registrar(int idUsuarioAccion, CategoriaRegistrarDto datos);


    Task Actualizar(
        int idUsuarioAccion,
        int idCategoria,
        CategoriaActualizarDto datos);


    Task CambiarActivo(int idUsuarioAccion, int idCategoria);


}

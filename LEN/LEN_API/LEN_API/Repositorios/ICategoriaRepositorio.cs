
using LEN_API.Dtos;

namespace LEN_API.Repositorios;

public interface ICategoriaRepositorio
{
    Task<List<CategoriaAdminDto>> Listar();

    // BT-05. SP_Categoria_ListarActivas, para el menu y el filtro de la tienda
    Task<List<CategoriaAdminDto>> ListarActivas();

    Task<int> Registrar(int idUsuarioAccion, CategoriaRegistrarDto datos);


    Task Actualizar(
        int idUsuarioAccion,
        int idCategoria,
        CategoriaActualizarDto datos);


    Task CambiarActivo(int idUsuarioAccion, int idCategoria);


}

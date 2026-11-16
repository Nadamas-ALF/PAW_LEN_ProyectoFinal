using LEN_API.Dtos;

namespace LEN_API.Repositorios;

public interface IFavoritoRepositorio
{
    Task Agregar(int idUsuario, int idProducto);
    Task Eliminar(int idUsuario, int idProducto);
    Task<List<FavoritoProductoDto>> ListarPorUsuario(int idUsuario);
}
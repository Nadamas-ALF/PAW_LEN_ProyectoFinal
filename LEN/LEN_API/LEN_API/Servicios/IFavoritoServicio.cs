
using LEN_API.Dtos;

namespace LEN_API.Servicios;

public interface IFavoritoServicio
{
    Task Agregar(int idUsuario, int idProducto);
    Task Eliminar(int idUsuario, int idProducto);
    Task<List<FavoritoProductoDto>> ListarPorUsuario(int idUsuario);
}
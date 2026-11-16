using LEN_API.Dtos;
using LEN_API.Infraestructura;
using LEN_API.Repositorios;

namespace LEN_API.Servicios;

public class FavoritoServicio : IFavoritoServicio
{
    private readonly IFavoritoRepositorio _repositorio;

    public FavoritoServicio(IFavoritoRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task Agregar(int idUsuario, int idProducto)
    {
        ValidarProducto(idProducto);
        await _repositorio.Agregar(idUsuario, idProducto);
    }

    public async Task Eliminar(int idUsuario, int idProducto)
    {
        ValidarProducto(idProducto);
        await _repositorio.Eliminar(idUsuario, idProducto);
    }

    public async Task<List<FavoritoProductoDto>> ListarPorUsuario(int idUsuario)
    {
        return await _repositorio.ListarPorUsuario(idUsuario);
    }

    private static void ValidarProducto(int idProducto)
    {
        if (idProducto <= 0)
            throw new ExcepcionNegocio("Debe seleccionar un producto válido.");
    }
}//Fin clase
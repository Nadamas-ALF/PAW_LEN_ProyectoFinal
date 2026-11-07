
using LEN_API.Dtos;
using LEN_API.Repositorios;

namespace LEN_API.Servicios;

public class CategoriaServicio : ICategoriaServicio
{
    private readonly ICategoriaRepositorio _categoriaRepositorio;

    public CategoriaServicio(ICategoriaRepositorio categoriaRepositorio)
    {
        _categoriaRepositorio = categoriaRepositorio;
    }

    public async Task<List<CategoriaAdminDto>> Listar()
    {
        return await _categoriaRepositorio.Listar();
    }

    public async Task<List<CategoriaAdminDto>> ListarActivas()
    {
        return await _categoriaRepositorio.ListarActivas();
    }


    public async Task<int> Registrar(
        int idUsuarioAccion,
        CategoriaRegistrarDto datos)
    {
        return await _categoriaRepositorio.Registrar(idUsuarioAccion, datos);
    }


    public async Task Actualizar(
        int idUsuarioAccion,
        int idCategoria,
        CategoriaActualizarDto datos)
    {
        await _categoriaRepositorio.Actualizar(
            idUsuarioAccion,
            idCategoria,
            datos);
    }



    public async Task CambiarActivo(
        int idUsuarioAccion,
        int idCategoria)
    {
        await _categoriaRepositorio.CambiarActivo(
            idUsuarioAccion,
            idCategoria);
    }


}

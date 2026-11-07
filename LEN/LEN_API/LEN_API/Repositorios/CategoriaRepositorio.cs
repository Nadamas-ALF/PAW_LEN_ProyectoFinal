
using LEN_API.Datos;
using LEN_API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace LEN_API.Repositorios;

public class CategoriaRepositorio : ICategoriaRepositorio
{
    private readonly ContextoLen _contexto;

    public CategoriaRepositorio(ContextoLen contexto)
    {
        _contexto = contexto;
    }

    public async Task<List<CategoriaAdminDto>> Listar()
    {
        return await _contexto.Database
            .SqlQuery<CategoriaAdminDto>($@"
                EXEC dbo.SP_Categoria_Listar")
            .ToListAsync();
    }

    public async Task<List<CategoriaAdminDto>> ListarActivas()
    {
        return await _contexto.Database
            .SqlQuery<CategoriaAdminDto>($@"
                EXEC dbo.SP_Categoria_ListarActivas")
            .ToListAsync();
    }


    public async Task<int> Registrar(
        int idUsuarioAccion,
        CategoriaRegistrarDto datos)
    {
        var resultado = await _contexto.Database
            .SqlQuery<int>($@"
            EXEC dbo.SP_Categoria_Registrar
                @IdUsuarioAccion = {idUsuarioAccion},
                @Nombre = {datos.Nombre},
                @Descripcion = {datos.Descripcion}")
            .ToListAsync();

        return resultado.Single();
    }


    public async Task Actualizar(
        int idUsuarioAccion,
        int idCategoria,
        CategoriaActualizarDto datos)
    {
        await _contexto.Database.ExecuteSqlAsync($@"
        EXEC dbo.SP_Categoria_Actualizar
            @IdUsuarioAccion = {idUsuarioAccion},
            @IdCategoria = {idCategoria},
            @Nombre = {datos.Nombre},
            @Descripcion = {datos.Descripcion}");
    }




    public async Task CambiarActivo(
        int idUsuarioAccion,
        int idCategoria)
    {
        await _contexto.Database.ExecuteSqlAsync($@"
        EXEC dbo.SP_Categoria_CambiarActivo
            @IdUsuarioAccion = {idUsuarioAccion},
            @IdCategoria = {idCategoria}");
    }





}

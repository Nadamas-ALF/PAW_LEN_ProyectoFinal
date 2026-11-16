using LEN_API.Datos;
using LEN_API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace LEN_API.Repositorios;

public class FavoritoRepositorio : IFavoritoRepositorio
{
    private readonly ContextoLen _contexto;

    public FavoritoRepositorio(ContextoLen contexto)
    {
        _contexto = contexto;
    }

    // Valida que el producto en la BD este activo y no se duplique.
    public async Task Agregar(int idUsuario, int idProducto)
    {
        await _contexto.Database.ExecuteSqlAsync($@"
            EXEC dbo.SP_Favorito_Agregar
                @IdUsuario = {idUsuario},
                @IdProducto = {idProducto}");
    }

    public async Task Eliminar(int idUsuario, int idProducto)
    {
        await _contexto.Database.ExecuteSqlAsync($@"
            EXEC dbo.SP_Favorito_Eliminar
                @IdUsuario = {idUsuario},
                @IdProducto = {idProducto}");
    }

    // Excluye aquellos productos inactivos y ordena por FechaAgregado.
    public async Task<List<FavoritoProductoDto>> ListarPorUsuario(int idUsuario)
    {
        return await _contexto.Database
            .SqlQuery<FavoritoProductoDto>($@"
                EXEC dbo.SP_Favorito_ListarPorUsuario
                    @IdUsuario = {idUsuario}")
            .ToListAsync();
    }
}//Fin de clase
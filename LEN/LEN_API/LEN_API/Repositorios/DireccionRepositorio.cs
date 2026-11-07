
using LEN_API.Datos;
using LEN_API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace LEN_API.Repositorios;

public class DireccionRepositorio : IDireccionRepositorio
{
    private readonly ContextoLen _contexto;

    public DireccionRepositorio(ContextoLen contexto)
    {
        _contexto = contexto;
    }

    public async Task<List<DireccionAdminDto>> ListarPorUsuario(
        int idUsuario,
        bool incluirInactivas)
    {
        return await _contexto.Database
            .SqlQuery<DireccionAdminDto>($@"
                EXEC dbo.SP_Direccion_ListarPorUsuario
                    @IdUsuario = {idUsuario},
                    @SoloActivas = {!incluirInactivas}")
            .ToListAsync();
    }

    public async Task<DireccionAdminDto?> ObtenerPorId(
        int idDireccion,
        int idUsuario)
    {
        var filas = await _contexto.Database
            .SqlQuery<DireccionAdminDto>($@"
                EXEC dbo.SP_Direccion_ObtenerPorId
                    @IdDireccion = {idDireccion},
                    @IdUsuario = {idUsuario}")
            .ToListAsync();

        return filas.SingleOrDefault();
    }

    public async Task<List<ProvinciaDto>> ListarProvincias()
    {
        return await _contexto.Database
            .SqlQuery<ProvinciaDto>($@"
                EXEC dbo.SP_Provincia_Listar")
            .ToListAsync();
    }

    public async Task<List<CantonDto>> ListarCantones(int idProvincia)
    {
        return await _contexto.Database
            .SqlQuery<CantonDto>($@"
                EXEC dbo.SP_Canton_ListarPorProvincia
                    @IdProvincia = {idProvincia}")
            .ToListAsync();
    }

    public async Task<List<DistritoDto>> ListarDistritos(int idCanton)
    {
        return await _contexto.Database
            .SqlQuery<DistritoDto>($@"
                EXEC dbo.SP_Distrito_ListarPorCanton
                    @IdCanton = {idCanton}")
            .ToListAsync();
    }


    public async Task<int> Registrar(
    int idUsuarioAccion,
    CrearDireccionDto datos)
    {
        var resultado = await _contexto.Database
            .SqlQuery<int>($@"
            EXEC dbo.SP_Direccion_Registrar
                @IdUsuarioAccion = {idUsuarioAccion},
                @IdUsuario = {datos.IdUsuario},
                @IdDistrito = {datos.IdDistrito},
                @NombreDestinatario = {datos.NombreDestinatario},
                @DireccionExacta = {datos.DireccionExacta},
                @Referencia = {datos.Referencia},
                @TelefonoContacto = {datos.TelefonoContacto},
                @EsPrincipal = {datos.EsPrincipal}")
            .ToListAsync();

        return resultado.Single();
    }

    public async Task Actualizar(
        int idUsuarioAccion,
        int idDireccion,
        int idUsuario,
        ActualizarDireccionDto datos)
    {
        await _contexto.Database.ExecuteSqlAsync($@"
        EXEC dbo.SP_Direccion_Actualizar
            @IdUsuarioAccion = {idUsuarioAccion},
            @IdDireccion = {idDireccion},
            @IdUsuario = {idUsuario},
            @IdDistrito = {datos.IdDistrito},
            @NombreDestinatario = {datos.NombreDestinatario},
            @DireccionExacta = {datos.DireccionExacta},
            @Referencia = {datos.Referencia},
            @TelefonoContacto = {datos.TelefonoContacto},
            @EsPrincipal = {datos.EsPrincipal}");
    }

    public async Task Desactivar(
        int idUsuarioAccion,
        int idDireccion,
        int idUsuario)
    {
        await _contexto.Database.ExecuteSqlAsync($@"
        EXEC dbo.SP_Direccion_Desactivar
            @IdUsuarioAccion = {idUsuarioAccion},
            @IdDireccion = {idDireccion},
            @IdUsuario = {idUsuario}");
    }

    public async Task MarcarPrincipal(
        int idUsuarioAccion,
        int idDireccion,
        int idUsuario)
    {
        await _contexto.Database.ExecuteSqlAsync($@"
        EXEC dbo.SP_Direccion_MarcarPrincipal
            @IdUsuarioAccion = {idUsuarioAccion},
            @IdDireccion = {idDireccion},
            @IdUsuario = {idUsuario}");
    }
}

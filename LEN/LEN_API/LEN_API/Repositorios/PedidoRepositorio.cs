using LEN_API.Datos;
using LEN_API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace LEN_API.Repositorios;

public class PedidoRepositorio : IPedidoRepositorio
{
    private readonly ContextoLen _contexto;

    public PedidoRepositorio(ContextoLen contexto)
    {
        _contexto = contexto;
    }

    public async Task<List<PedidoResumenDto>> ListarPorUsuario(int idUsuario, int pagina, int tamanoPagina)
    {
        return await _contexto.Database
            .SqlQuery<PedidoResumenDto>($@"
                EXEC dbo.SP_Pedido_ListarPorUsuario
                    @IdUsuario = {idUsuario},
                    @Pagina = {pagina},
                    @TamanoPagina = {tamanoPagina}")
            .ToListAsync();
    }

    public async Task<List<PedidoDetalleFilaDto>> ObtenerDetalle(int idPedido, int idUsuario)
    {
        return await _contexto.Database
            .SqlQuery<PedidoDetalleFilaDto>($@"
                EXEC dbo.SP_Pedido_ObtenerDetalle
                    @IdPedido = {idPedido},
                    @IdUsuario = {idUsuario}")
            .ToListAsync();
    }

    // ------------------------------------------------------------
    // BT-11. Administracion
    // ------------------------------------------------------------

    public async Task<List<PedidoAdminResumenDto>> ListarAdministracion(
        string? busqueda,
        int? idEstadoPedido,
        DateTime? fechaInicio,
        DateTime? fechaFin,
        int pagina,
        int tamanoPagina)
    {
        return await _contexto.Database
            .SqlQuery<PedidoAdminResumenDto>($@"
                EXEC dbo.SP_Pedido_Listar
                    @Busqueda = {busqueda},
                    @IdEstadoPedido = {idEstadoPedido},
                    @FechaInicio = {fechaInicio},
                    @FechaFin = {fechaFin},
                    @Pagina = {pagina},
                    @TamanoPagina = {tamanoPagina}")
            .ToListAsync();
    }

    public async Task<List<PedidoAdminDetalleFilaDto>> ObtenerDetalleAdministracion(int idPedido)
    {
        return await _contexto.Database
            .SqlQuery<PedidoAdminDetalleFilaDto>($@"
                EXEC dbo.SP_Pedido_ObtenerDetalleAdministracion
                    @IdPedido = {idPedido}")
            .ToListAsync();
    }

    public async Task<List<EstadoPedidoDto>> ListarEstados()
    {
        return await _contexto.Database
            .SqlQuery<EstadoPedidoDto>($@"
                EXEC dbo.SP_EstadoPedido_Listar")
            .ToListAsync();
    }

    public async Task<List<HistorialPedidoDto>> ListarHistorial(int idPedido)
    {
        return await _contexto.Database
            .SqlQuery<HistorialPedidoDto>($@"
                EXEC dbo.SP_Pedido_ListarHistorial
                    @IdPedido = {idPedido}")
            .ToListAsync();
    }

    public async Task CambiarEstado(int idUsuarioAccion, int idPedido, int idEstadoNuevo, string? motivoCancelacion)
    {
        await _contexto.Database.ExecuteSqlAsync($@"
            EXEC dbo.SP_Pedido_CambiarEstado
                @IdUsuarioAccion = {idUsuarioAccion},
                @IdPedido = {idPedido},
                @IdEstadoNuevo = {idEstadoNuevo},
                @MotivoCancelacion = {motivoCancelacion}");
    }
}

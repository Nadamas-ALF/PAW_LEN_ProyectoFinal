using LEN_API.Dtos;
using LEN_API.Infraestructura;
using LEN_API.Repositorios;

namespace LEN_API.Servicios;

public class PedidoServicio : IPedidoServicio
{
    private readonly IPedidoRepositorio _repositorio;

    public PedidoServicio(IPedidoRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<List<PedidoResumenDto>> ListarPorUsuario(int idUsuario, int pagina, int tamanoPagina)
    {
        // el procedimiento corrige pagina y tamano invalidos; aca solo se pone techo
        return await _repositorio.ListarPorUsuario(idUsuario, pagina, Math.Clamp(tamanoPagina, 1, 50));
    }

    public async Task<PedidoDetalleDto?> ObtenerDetalle(int idPedido, int idUsuario)
    {
        var filas = await _repositorio.ObtenerDetalle(idPedido, idUsuario);

        if (filas.Count == 0)
            return null;

        var detalle = new PedidoDetalleDto();
        Armar(detalle, filas);
        return detalle;
    }

    // Los dos detalles (cliente y panel) devuelven una fila por producto con
    // el pedido repetido. Aca se toma el pedido de la primera fila y las lineas
    // de todas. Las filas del panel heredan de las del cliente, por eso sirve
    // para los dos
    private static void Armar(PedidoDetalleDto destino, IReadOnlyList<PedidoDetalleFilaDto> filas)
    {
        var p = filas[0];

        destino.IdPedido = p.IdPedido;
        destino.NumeroFactura = p.NumeroFactura;
        destino.FechaPedido = p.FechaPedido;
        destino.Subtotal = p.Subtotal;
        destino.CostoEnvio = p.CostoEnvio;
        destino.Total = p.Total;
        destino.IdEstadoPedido = p.IdEstadoPedido;
        destino.NombreEstado = p.NombreEstado;
        destino.ClaseCssEstado = p.ClaseCssEstado;
        destino.EstadoVisible = p.EstadoVisible;
        destino.MotivoCancelacion = p.MotivoCancelacion;
        destino.EstadoPago = p.EstadoPago;
        destino.FechaPago = p.FechaPago;
        destino.NombreMetodoPago = p.NombreMetodoPago;
        destino.ComprobantePago = p.ComprobantePago;
        destino.Direccion = new DireccionAdminDto
        {
            NombreDestinatario = p.NombreDestinatario,
            TelefonoContacto = p.TelefonoContacto,
            DireccionExacta = p.DireccionExacta,
            Referencia = p.Referencia,
            NombreDistrito = p.NombreDistrito,
            NombreCanton = p.NombreCanton,
            NombreProvincia = p.NombreProvincia
        };
        destino.Lineas = filas.Select(f => new LineaPedidoDto
        {
            IdProducto = f.IdProducto,
            NombreProducto = f.NombreProducto,
            RutaImagen = f.RutaImagen,
            Cantidad = f.Cantidad,
            PrecioUnitario = f.PrecioUnitario,
            SubtotalLinea = f.SubtotalLinea
        }).ToList();
    }

    // ------------------------------------------------------------
    // BT-11. Administracion
    // ------------------------------------------------------------

    // 4 es Cancelado en TB_ESTADO_PEDIDO
    private const int EstadoCancelado = 4;

    // HU-ADM-PED-003. Las mismas transiciones que valida SP_Pedido_CambiarEstado
    // (CA-5): Pagado -> En preparacion o Cancelado; En preparacion -> Entregado
    // o Cancelado; Entregado y Cancelado no tienen salida (CA-3). Aca solo se
    // usan para que la pantalla ofrezca las permitidas (CA-4): la regla que
    // manda es la del procedimiento
    private static readonly Dictionary<int, int[]> Transiciones = new()
    {
        [1] = [2, EstadoCancelado],
        [2] = [3, EstadoCancelado]
    };

    public async Task<List<PedidoAdminResumenDto>> ListarAdministracion(
        string? busqueda,
        int? idEstadoPedido,
        DateTime? fechaInicio,
        DateTime? fechaFin,
        int pagina,
        int tamanoPagina)
    {
        if (fechaInicio.HasValue && fechaFin.HasValue && fechaInicio.Value.Date > fechaFin.Value.Date)
            throw new ExcepcionNegocio("La fecha de inicio no puede ser posterior a la de fin.");

        // CA-4. El procedimiento incluye el ultimo dia completo (< FechaFin + 1)
        return await _repositorio.ListarAdministracion(
            string.IsNullOrWhiteSpace(busqueda) ? null : busqueda.Trim(),
            idEstadoPedido,
            fechaInicio?.Date,
            fechaFin?.Date,
            pagina,
            Math.Clamp(tamanoPagina, 1, 50));
    }

    public async Task<PedidoAdminDetalleDto?> ObtenerDetalleAdministracion(int idPedido)
    {
        var filas = await _repositorio.ObtenerDetalleAdministracion(idPedido);

        if (filas.Count == 0)
            return null;

        var detalle = new PedidoAdminDetalleDto();
        Armar(detalle, filas);

        var cliente = filas[0];
        detalle.IdUsuario = cliente.IdUsuario;
        detalle.NombreCliente = cliente.NombreCliente;
        detalle.CorreoCliente = cliente.CorreoCliente;
        detalle.TelefonoCliente = cliente.TelefonoCliente;

        if (Transiciones.TryGetValue(detalle.IdEstadoPedido, out var siguientes))
        {
            var estados = await _repositorio.ListarEstados();
            detalle.EstadosSiguientes = estados
                .Where(e => siguientes.Contains(e.IdEstadoPedido))
                .ToList();
        }

        detalle.Historial = await _repositorio.ListarHistorial(idPedido);

        return detalle;
    }

    public async Task<List<EstadoPedidoDto>> ListarEstados()
    {
        return await _repositorio.ListarEstados();
    }

    public async Task CambiarEstado(int idUsuarioAccion, int idPedido, CambiarEstadoPedidoDto datos)
    {
        var motivo = string.IsNullOrWhiteSpace(datos.MotivoCancelacion) ? null : datos.MotivoCancelacion.Trim();

        // HU-ADM-PED-004 CA-1. El procedimiento lo vuelve a revisar
        if (datos.IdEstadoNuevo == EstadoCancelado && motivo is null)
            throw new ExcepcionNegocio("Para cancelar un pedido hay que indicar el motivo.");

        // el motivo solo se guarda al cancelar (CK_Pedido_MotivoSoloCancelado)
        await _repositorio.CambiarEstado(
            idUsuarioAccion,
            idPedido,
            datos.IdEstadoNuevo,
            datos.IdEstadoNuevo == EstadoCancelado ? motivo : null);
    }
}

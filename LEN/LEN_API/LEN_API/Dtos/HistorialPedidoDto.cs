namespace LEN_API.Dtos;

// BT-11. HU-ADM-PED-005. Una fila de SP_Pedido_ListarHistorial (TB_BITACORA)
public class HistorialPedidoDto
{
    public long IdBitacora { get; set; }
    public DateTime FechaHora { get; set; }
    public string Accion { get; set; } = string.Empty;
    public int? IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string? CorreoUsuario { get; set; }

    // Sacados del JSON de la bitacora. Null en las acciones que no cambian estado
    public string? EstadoAnterior { get; set; }
    public string? EstadoNuevo { get; set; }
    public string? MotivoCancelacion { get; set; }

    // El JSON completo, por si la accion no es un cambio de estado
    public string? ValorAnterior { get; set; }
    public string? ValorNuevo { get; set; }
}

namespace LEN_API.Dtos;

// BT-11. Una fila de SP_EstadoPedido_Listar
public class EstadoPedidoDto
{
    public int IdEstadoPedido { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; }
    public string? ClaseCss { get; set; }
}

namespace LEN_API.Dtos;

public class ProductoBajoStockDto
{
    public int IdProducto { get; set; }

    public string NombreProducto { get; set; } = string.Empty;

    public int Stock { get; set; }

    public bool EsPiezaUnica { get; set; }

    public int IdCategoria { get; set; }

    public string NombreCategoria { get; set; } = string.Empty;

    public string NombreEstado { get; set; } = string.Empty;
}
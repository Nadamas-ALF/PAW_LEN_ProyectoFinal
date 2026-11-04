
namespace LEN_API.Dtos;

public class ProductoAdminDto
{
    public int IdProducto { get; set; }

    public string NombreProducto { get; set; } = string.Empty;

    public decimal Precio { get; set; }

    public int Stock { get; set; }

    public bool EsPiezaUnica { get; set; }

    public bool Destacado { get; set; }

    public bool Activo { get; set; }

    public int IdCategoria { get; set; }

    public string NombreCategoria { get; set; } = string.Empty;

    public string NombreEstado { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; }

    public string? RutaImagen { get; set; }

    public int CantidadImagenes { get; set; }

    public int TotalFilas { get; set; }
}

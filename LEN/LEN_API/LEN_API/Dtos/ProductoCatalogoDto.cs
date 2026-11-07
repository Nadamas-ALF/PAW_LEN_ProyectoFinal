namespace LEN_API.Dtos;

// BT-05. Una fila de SP_Catalogo_Listar o de SP_Producto_ListarDestacados, que
// devuelven las mismas columnas. No se reusa ProductoAdminDto porque ese es la
// fila de SP_Producto_Listar (panel) y trae otras columnas
public class ProductoCatalogoDto
{
    public int IdProducto { get; set; }

    public string NombreProducto { get; set; } = string.Empty;

    public decimal Precio { get; set; }

    public int Stock { get; set; }

    public bool EsPiezaUnica { get; set; }

    public bool Destacado { get; set; }

    public int IdCategoria { get; set; }

    public string NombreCategoria { get; set; } = string.Empty;

    // Disponible o Agotado, calculado del stock en el procedimiento
    public string NombreEstado { get; set; } = string.Empty;

    // De la imagen principal. Null si el producto no tiene imagenes
    public string? RutaImagen { get; set; }

    public string? TextoAlternativo { get; set; }

    public bool EsFavorito { get; set; }

    // El mismo valor en todas las filas: el total sin paginar (HU-PUB-004 CA-1)
    public int TotalFilas { get; set; }
}

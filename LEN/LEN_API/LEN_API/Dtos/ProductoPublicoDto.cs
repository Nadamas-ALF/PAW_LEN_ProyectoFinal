namespace LEN_API.Dtos;

// BT-05. SP_Producto_ObtenerDetalle: el detalle que ve la tienda. No se reusa
// ProductoDetalleAdminDto porque ese trae Activo, Destacado y FechaCreacion,
// que el procedimiento publico no devuelve
public class ProductoPublicoDto
{
    public int IdProducto { get; set; }

    public string NombreProducto { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public int Stock { get; set; }

    public bool EsPiezaUnica { get; set; }

    public int IdCategoria { get; set; }

    public string NombreCategoria { get; set; } = string.Empty;

    public string NombreEstado { get; set; } = string.Empty;

    public bool EsFavorito { get; set; }
}

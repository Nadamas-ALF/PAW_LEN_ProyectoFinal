namespace LEN_API.Dtos;

// BT-05. Una imagen de producto. Mismas columnas en SP_Catalogo_ListarImagenes
// (tienda), SP_ImagenProducto_ListarPorProducto (panel) y en lo que devuelve
// SP_ImagenProducto_Registrar
public class ImagenProductoDto
{
    public int IdImagen { get; set; }

    public int IdProducto { get; set; }

    // Relativa a wwwroot/imagenes del Web, por ejemplo productos/3f2a...c1.webp
    public string RutaImagen { get; set; } = string.Empty;

    public string? TextoAlternativo { get; set; }

    public bool EsPrincipal { get; set; }
}

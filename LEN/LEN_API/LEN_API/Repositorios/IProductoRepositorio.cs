
using LEN_API.Dtos;

namespace LEN_API.Repositorios;

public interface IProductoRepositorio
{
    Task<List<ProductoAdminDto>> Listar(
        string? busqueda,
        int? idCategoria,
        bool soloActivos,
        int pagina,
        int tamanoPagina);



    Task<int> Registrar(int idUsuarioAccion, ProductoRegistrarDto datos);
    Task Actualizar(
    int idUsuarioAccion,
    int idProducto,
    ProductoActualizarDto datos);


    Task CambiarActivo(int idUsuarioAccion, int idProducto);

    Task<ProductoDetalleAdminDto?> ObtenerPorId(int idProducto);

    Task ActualizarStock(int idUsuarioAccion, int idProducto, ProductoActualizarStockDto datos);

    Task<List<ProductoBajoStockDto>> ListarBajoStock(int stockMinimo);

    // ------------------------------------------------------------
    // BT-05. Catalogo publico
    // ------------------------------------------------------------

    // SP_Catalogo_Listar. idUsuarioActual null si nadie inicio sesion
    Task<List<ProductoCatalogoDto>> ListarCatalogo(
        string? busqueda,
        int? idCategoria,
        string? color,
        int? idUsuarioActual,
        int pagina,
        int tamanoPagina);

    // SP_Producto_ListarDestacados
    Task<List<ProductoCatalogoDto>> ListarDestacados(int cantidad);

    // SP_Producto_ObtenerDetalle. Null si no existe o esta inactivo
    Task<ProductoPublicoDto?> ObtenerDetallePublico(int idProducto, int? idUsuarioActual);

    // SP_Catalogo_ListarImagenes
    Task<List<ImagenProductoDto>> ListarImagenesPublicas(int idProducto);

    // ------------------------------------------------------------
    // BT-05. Imagenes (administracion)
    // ------------------------------------------------------------

    // SP_ImagenProducto_ListarPorProducto
    Task<List<ImagenProductoDto>> ListarImagenes(int idProducto);

    // SP_ImagenProducto_Registrar. Devuelve la fila creada
    Task<ImagenProductoDto> RegistrarImagen(
        int idUsuarioAccion,
        int idProducto,
        string rutaImagen,
        string? textoAlternativo);

    // SP_ImagenProducto_MarcarPrincipal
    Task MarcarImagenPrincipal(int idUsuarioAccion, int idImagen);

    // SP_ImagenProducto_Desactivar. Baja logica: el archivo se conserva (DEC-006)
    Task DesactivarImagen(int idUsuarioAccion, int idImagen);
}

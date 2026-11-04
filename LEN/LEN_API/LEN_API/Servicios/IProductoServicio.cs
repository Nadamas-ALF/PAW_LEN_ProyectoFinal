
using LEN_API.Dtos;

namespace LEN_API.Servicios;

public interface IProductoServicio
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
}

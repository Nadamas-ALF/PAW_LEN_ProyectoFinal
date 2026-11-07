
using LEN_API.Dtos;
using LEN_API.Repositorios;

namespace LEN_API.Servicios;

public class ProductoServicio : IProductoServicio
{
    private readonly IProductoRepositorio _productoRepositorio;

    public ProductoServicio(IProductoRepositorio productoRepositorio)
    {
        _productoRepositorio = productoRepositorio;
    }

    public async Task<List<ProductoAdminDto>> Listar(
        string? busqueda,
        int? idCategoria,
        bool soloActivos,
        int pagina,
        int tamanoPagina)
    {
        return await _productoRepositorio.Listar(
            busqueda,
            idCategoria,
            soloActivos,
            pagina,
            tamanoPagina);
    }


    public async Task<int> Registrar(
    int idUsuarioAccion,
    ProductoRegistrarDto datos)
    {
        return await _productoRepositorio.Registrar(
            idUsuarioAccion,
            datos);
    }



    public async Task Actualizar(
    int idUsuarioAccion,
    int idProducto,
    ProductoActualizarDto datos)
    {
        await _productoRepositorio.Actualizar(
            idUsuarioAccion,
            idProducto,
            datos);
    }


    public async Task CambiarActivo(int idUsuarioAccion, int idProducto)
    {
        await _productoRepositorio.CambiarActivo(idUsuarioAccion, idProducto);
    }


    public async Task<ProductoDetalleAdminDto?> ObtenerPorId(int idProducto)
    {
        return await _productoRepositorio.ObtenerPorId(idProducto);
    }


    public async Task ActualizarStock(
    int idUsuarioAccion,
    int idProducto,
    ProductoActualizarStockDto datos)
    {
        await _productoRepositorio.ActualizarStock(
            idUsuarioAccion,
            idProducto,
            datos);
    }

    public async Task<List<ProductoBajoStockDto>> ListarBajoStock(int stockMinimo)
    {
        return await _productoRepositorio.ListarBajoStock(stockMinimo);
    }




}


using LEN_API.Datos;
using LEN_API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace LEN_API.Repositorios;

public class ProductoRepositorio : IProductoRepositorio
{
    private readonly ContextoLen _contexto;

    public ProductoRepositorio(ContextoLen contexto)
    {
        _contexto = contexto;
    }

    public async Task<List<ProductoAdminDto>> Listar(
        string? busqueda,
        int? idCategoria,
        bool soloActivos,
        int pagina,
        int tamanoPagina)
    {
        return await _contexto.Database
            .SqlQuery<ProductoAdminDto>($@"
                EXEC dbo.SP_Producto_Listar
                    @Busqueda = {busqueda},
                    @IdCategoria = {idCategoria},
                    @SoloActivos = {soloActivos},
                    @Pagina = {pagina},
                    @TamanoPagina = {tamanoPagina}")
            .ToListAsync();
    }


    public async Task<int> Registrar(
        int idUsuarioAccion,
        ProductoRegistrarDto datos)
    {
        var resultado = await _contexto.Database
            .SqlQuery<int>($@"
            EXEC dbo.SP_Producto_Registrar
                @IdUsuarioAccion = {idUsuarioAccion},
                @IdCategoria = {datos.IdCategoria},
                @Nombre = {datos.Nombre},
                @Descripcion = {datos.Descripcion},
                @Precio = {datos.Precio},
                @Stock = {datos.Stock},
                @EsPiezaUnica = {datos.EsPiezaUnica},
                @Destacado = {datos.Destacado}")
            .ToListAsync();

        return resultado.Single();
    }

    public async Task Actualizar(
    int idUsuarioAccion,
    int idProducto,
    ProductoActualizarDto datos)
    {
        await _contexto.Database.ExecuteSqlAsync($@"
        EXEC dbo.SP_Producto_Actualizar
            @IdUsuarioAccion = {idUsuarioAccion},
            @IdProducto = {idProducto},
            @IdCategoria = {datos.IdCategoria},
            @Nombre = {datos.Nombre},
            @Descripcion = {datos.Descripcion},
            @Precio = {datos.Precio},
            @Stock = {datos.Stock},
            @EsPiezaUnica = {datos.EsPiezaUnica},
            @Destacado = {datos.Destacado}");
    }



    public async Task CambiarActivo(int idUsuarioAccion, int idProducto)
    {
        await _contexto.Database.ExecuteSqlAsync($@"
        EXEC dbo.SP_Producto_CambiarActivo
        @IdUsuarioAccion = {idUsuarioAccion},
        @IdProducto = {idProducto}");
    }

    public async Task<ProductoDetalleAdminDto?> ObtenerPorId(int idProducto)
    {
        var resultado = await _contexto.Database
            .SqlQuery<ProductoDetalleAdminDto>($@"
            EXEC dbo.SP_Producto_ObtenerPorId
            @IdProducto = {idProducto}")
            .ToListAsync();

        return resultado.FirstOrDefault();
    }


    public async Task ActualizarStock(
    int idUsuarioAccion,
    int idProducto,
    ProductoActualizarStockDto datos)
    {
        await _contexto.Database.ExecuteSqlAsync($@"
        EXEC dbo.SP_Producto_ActualizarStock
        @IdUsuarioAccion = {idUsuarioAccion},
        @IdProducto = {idProducto},
        @Stock = {datos.Stock}");
    }

    public async Task<List<ProductoBajoStockDto>> ListarBajoStock(int stockMinimo)
    {
        return await _contexto.Database
            .SqlQuery<ProductoBajoStockDto>($@"
            EXEC dbo.SP_Producto_ListarBajoStock
            @StockMinimo = {stockMinimo}")
            .ToListAsync();
    }

    // ------------------------------------------------------------
    // BT-05. Catalogo publico
    // ------------------------------------------------------------

    public async Task<List<ProductoCatalogoDto>> ListarCatalogo(
        string? busqueda,
        int? idCategoria,
        string? color,
        int? idUsuarioActual,
        int pagina,
        int tamanoPagina)
    {
        return await _contexto.Database
            .SqlQuery<ProductoCatalogoDto>($@"
            EXEC dbo.SP_Catalogo_Listar
                @Busqueda = {busqueda},
                @IdCategoria = {idCategoria},
                @Color = {color},
                @IdUsuarioActual = {idUsuarioActual},
                @Pagina = {pagina},
                @TamanoPagina = {tamanoPagina}")
            .ToListAsync();
    }

    public async Task<List<ProductoCatalogoDto>> ListarDestacados(int cantidad)
    {
        return await _contexto.Database
            .SqlQuery<ProductoCatalogoDto>($@"
            EXEC dbo.SP_Producto_ListarDestacados
                @Cantidad = {cantidad}")
            .ToListAsync();
    }

    public async Task<ProductoPublicoDto?> ObtenerDetallePublico(int idProducto, int? idUsuarioActual)
    {
        var filas = await _contexto.Database
            .SqlQuery<ProductoPublicoDto>($@"
            EXEC dbo.SP_Producto_ObtenerDetalle
                @IdProducto = {idProducto},
                @IdUsuarioActual = {idUsuarioActual}")
            .ToListAsync();

        return filas.FirstOrDefault();
    }

    public async Task<List<ImagenProductoDto>> ListarImagenesPublicas(int idProducto)
    {
        return await _contexto.Database
            .SqlQuery<ImagenProductoDto>($@"
            EXEC dbo.SP_Catalogo_ListarImagenes
                @IdProducto = {idProducto}")
            .ToListAsync();
    }

    // ------------------------------------------------------------
    // BT-05. Imagenes (administracion)
    // ------------------------------------------------------------

    public async Task<List<ImagenProductoDto>> ListarImagenes(int idProducto)
    {
        return await _contexto.Database
            .SqlQuery<ImagenProductoDto>($@"
            EXEC dbo.SP_ImagenProducto_ListarPorProducto
                @IdProducto = {idProducto}")
            .ToListAsync();
    }

    public async Task<ImagenProductoDto> RegistrarImagen(
        int idUsuarioAccion,
        int idProducto,
        string rutaImagen,
        string? textoAlternativo)
    {
        var filas = await _contexto.Database
            .SqlQuery<ImagenProductoDto>($@"
            EXEC dbo.SP_ImagenProducto_Registrar
                @IdUsuarioAccion = {idUsuarioAccion},
                @IdProducto = {idProducto},
                @RutaImagen = {rutaImagen},
                @TextoAlternativo = {textoAlternativo}")
            .ToListAsync();

        return filas.Single();
    }

    public async Task MarcarImagenPrincipal(int idUsuarioAccion, int idImagen)
    {
        await _contexto.Database.ExecuteSqlAsync($@"
        EXEC dbo.SP_ImagenProducto_MarcarPrincipal
            @IdUsuarioAccion = {idUsuarioAccion},
            @IdImagen = {idImagen}");
    }

    public async Task DesactivarImagen(int idUsuarioAccion, int idImagen)
    {
        await _contexto.Database.ExecuteSqlAsync($@"
        EXEC dbo.SP_ImagenProducto_Desactivar
            @IdUsuarioAccion = {idUsuarioAccion},
            @IdImagen = {idImagen}");
    }


}


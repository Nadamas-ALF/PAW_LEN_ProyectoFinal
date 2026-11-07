
using LEN_API.Dtos;
using LEN_API.Infraestructura;
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
        // el procedimiento corrige pagina y tamano invalidos; aca solo se pone
        // un techo para que nadie pida el catalogo entero de una vez (HU-PUB-004)
        tamanoPagina = Math.Clamp(tamanoPagina, 1, 48);

        return await _productoRepositorio.ListarCatalogo(
            string.IsNullOrWhiteSpace(busqueda) ? null : busqueda.Trim(),
            idCategoria,
            string.IsNullOrWhiteSpace(color) ? null : color.Trim(),
            idUsuarioActual,
            pagina,
            tamanoPagina);
    }

    public async Task<List<ProductoCatalogoDto>> ListarDestacados(int cantidad)
    {
        return await _productoRepositorio.ListarDestacados(Math.Clamp(cantidad, 1, 12));
    }

    public async Task<ProductoPublicoDto?> ObtenerDetallePublico(int idProducto, int? idUsuarioActual)
    {
        return await _productoRepositorio.ObtenerDetallePublico(idProducto, idUsuarioActual);
    }

    public async Task<List<ImagenProductoDto>> ListarImagenesPublicas(int idProducto)
    {
        return await _productoRepositorio.ListarImagenesPublicas(idProducto);
    }

    // ------------------------------------------------------------
    // BT-05. Imagenes (administracion)
    // ------------------------------------------------------------

    private const long TamanoMaximoImagen = 5 * 1024 * 1024;

    public async Task<List<ImagenProductoDto>> ListarImagenes(int idProducto)
    {
        return await _productoRepositorio.ListarImagenes(idProducto);
    }

    // HU-PRD-007 CA-3: el Web ya valido, pero el API no se fia y vuelve a
    // revisar. El tipo se decide por los primeros bytes del archivo, no por el
    // nombre: un .exe renombrado a .jpg no pasa
    public async Task<ImagenProductoDto> RegistrarImagen(
        int idUsuarioAccion,
        int idProducto,
        IFormFile? archivo,
        string? textoAlternativo)
    {
        if (archivo is null || archivo.Length == 0)
            throw new ExcepcionNegocio("Seleccione una imagen.");

        if (archivo.Length > TamanoMaximoImagen)
            throw new ExcepcionNegocio("La imagen supera el máximo de 5 MB.");

        var extensionNombre = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (extensionNombre is not (".jpg" or ".jpeg" or ".png" or ".webp"))
            throw new ExcepcionNegocio("Solo se aceptan imágenes JPG, PNG o WEBP.");

        var cabecera = new byte[12];
        int leidos;
        await using (var flujo = archivo.OpenReadStream())
        {
            leidos = await flujo.ReadAtLeastAsync(cabecera, cabecera.Length, throwOnEndOfStream: false);
        }

        var extension = ExtensionPorContenido(cabecera, leidos)
            ?? throw new ExcepcionNegocio("El archivo no es una imagen JPG, PNG o WEBP válida.");

        textoAlternativo = string.IsNullOrWhiteSpace(textoAlternativo) ? null : textoAlternativo.Trim();
        if (textoAlternativo is { Length: > 150 })
            throw new ExcepcionNegocio("La descripción de la imagen admite hasta 150 caracteres.");

        // Nombre propio y no el del archivo subido: evita choques y rutas raras.
        // Relativa a wwwroot/imagenes del Web
        var rutaImagen = $"productos/{Guid.NewGuid():N}{extension}";

        return await _productoRepositorio.RegistrarImagen(
            idUsuarioAccion, idProducto, rutaImagen, textoAlternativo);
    }

    // Firmas de los tres formatos aceptados
    private static string? ExtensionPorContenido(byte[] b, int leidos)
    {
        if (leidos >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF)
            return ".jpg";

        if (leidos >= 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47
            && b[4] == 0x0D && b[5] == 0x0A && b[6] == 0x1A && b[7] == 0x0A)
            return ".png";

        if (leidos >= 12 && b[0] == 'R' && b[1] == 'I' && b[2] == 'F' && b[3] == 'F'
            && b[8] == 'W' && b[9] == 'E' && b[10] == 'B' && b[11] == 'P')
            return ".webp";

        return null;
    }

    public async Task MarcarImagenPrincipal(int idUsuarioAccion, int idImagen)
    {
        await _productoRepositorio.MarcarImagenPrincipal(idUsuarioAccion, idImagen);
    }

    public async Task DesactivarImagen(int idUsuarioAccion, int idImagen)
    {
        await _productoRepositorio.DesactivarImagen(idUsuarioAccion, idImagen);
    }




}

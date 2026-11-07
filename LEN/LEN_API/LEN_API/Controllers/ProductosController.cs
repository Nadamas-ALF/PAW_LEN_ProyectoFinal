
using LEN_API.Dtos;
using LEN_API.Infraestructura;
using LEN_API.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LEN_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class ProductosController : ControllerBase
{
    private readonly IProductoServicio _productoServicio;

    public ProductosController(IProductoServicio productoServicio)
    {
        _productoServicio = productoServicio;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busqueda,
        [FromQuery] int? idCategoria,
        [FromQuery] bool soloActivos = false,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10)
    {
        var productos = await _productoServicio.Listar(
            busqueda,
            idCategoria,
            soloActivos,
            pagina,
            tamanoPagina);

        return Ok(productos);
    }


    [HttpPost]
    public async Task<IActionResult> Registrar(
        [FromBody] ProductoRegistrarDto datos)
    {
        var idUsuarioAccion = User.IdUsuario();

        var idProducto = await _productoServicio.Registrar(
            idUsuarioAccion,
            datos);

        return CreatedAtAction(
            nameof(Listar),
            new { idProducto },
            new
            {
                mensaje = "Producto registrado correctamente.",
                idProducto
            });
    }



    [HttpPut("{idProducto:int}")]
    public async Task<IActionResult> Actualizar(
        int idProducto,
        [FromBody] ProductoActualizarDto datos)
    {
        var idUsuarioAccion = User.IdUsuario();

        await _productoServicio.Actualizar(
            idUsuarioAccion,
            idProducto,
            datos);

        return Ok(new
        {
            mensaje = "Producto actualizado correctamente."
        });
    }

    [HttpPatch("{idProducto:int}/estado")]
    public async Task<IActionResult> CambiarActivo(int idProducto)
    {
        var idUsuarioAccion = User.IdUsuario();

        await _productoServicio.CambiarActivo(idUsuarioAccion, idProducto);

        return Ok(new
        {
            mensaje = "Estado del producto actualizado correctamente."
        });
    }

    [HttpGet("{idProducto:int}")]
    public async Task<IActionResult> ObtenerPorId(int idProducto)
    {
        var producto = await _productoServicio.ObtenerPorId(idProducto);

        if (producto == null)
        {
            return NotFound(new
            {
                mensaje = "El producto indicado no existe."
            });
        }

        return Ok(producto);
    }


    [HttpPatch("{idProducto:int}/stock")]
    public async Task<IActionResult> ActualizarStock(
    int idProducto,
    [FromBody] ProductoActualizarStockDto datos)
    {
        var idUsuarioAccion = User.IdUsuario();

        await _productoServicio.ActualizarStock(
            idUsuarioAccion,
            idProducto,
            datos);

        return Ok(new
        {
            mensaje = "Stock del producto actualizado correctamente."
        });
    }



    [HttpGet("bajo-stock")]
    public async Task<IActionResult> ListarBajoStock(
    [FromQuery] int stockMinimo = 3)
    {
        var productos = await _productoServicio.ListarBajoStock(stockMinimo);

        return Ok(productos);
    }

    // ------------------------------------------------------------
    // BT-05. Catalogo publico. AllowAnonymous quita, solo en estas, el rol
    // de Administrador que pide la clase. Si llega un token valido se usa
    // para marcar los favoritos; si no, se responde igual
    // ------------------------------------------------------------

    // HU-PUB-002, 003 y 004. Cada fila trae TotalFilas para paginar
    [AllowAnonymous]
    [HttpGet("catalogo")]
    public async Task<IActionResult> ListarCatalogo(
        [FromQuery] string? busqueda,
        [FromQuery] int? idCategoria,
        [FromQuery] string? color,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 12)
    {
        var productos = await _productoServicio.ListarCatalogo(
            busqueda,
            idCategoria,
            color,
            UsuarioActual(),
            pagina,
            tamanoPagina);

        return Ok(productos);
    }

    // HU-PUB-001. Lista vacia si no hay destacados (CA-4)
    [AllowAnonymous]
    [HttpGet("destacados")]
    public async Task<IActionResult> ListarDestacados([FromQuery] int cantidad = 4)
    {
        return Ok(await _productoServicio.ListarDestacados(cantidad));
    }

    // HU-PUB-005. 404 si no existe o esta inactivo (CA-2)
    [AllowAnonymous]
    [HttpGet("{idProducto:int}/publico")]
    public async Task<IActionResult> ObtenerDetallePublico(int idProducto)
    {
        var producto = await _productoServicio.ObtenerDetallePublico(idProducto, UsuarioActual());

        if (producto == null)
        {
            return NotFound(new
            {
                mensaje = "El producto indicado no existe."
            });
        }

        return Ok(producto);
    }

    // HU-PUB-005 CA-5. Solo imagenes activas, la principal primero
    [AllowAnonymous]
    [HttpGet("{idProducto:int}/galeria")]
    public async Task<IActionResult> ListarImagenesPublicas(int idProducto)
    {
        return Ok(await _productoServicio.ListarImagenesPublicas(idProducto));
    }

    // ------------------------------------------------------------
    // BT-05. Imagenes (solo Administrador, por la clase)
    // ------------------------------------------------------------

    // HU-PRD-007
    [HttpGet("{idProducto:int}/imagenes")]
    public async Task<IActionResult> ListarImagenes(int idProducto)
    {
        return Ok(await _productoServicio.ListarImagenes(idProducto));
    }

    // HU-PRD-007 y HU-PRD-010. multipart/form-data con archivo y
    // textoAlternativo. 201 con la imagen creada (RutaImagen incluida), o 400
    // si el tipo o el tamano no son validos (CA-3)
    [HttpPost("{idProducto:int}/imagenes")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> RegistrarImagen(
        int idProducto,
        IFormFile? archivo,
        [FromForm] string? textoAlternativo)
    {
        var imagen = await _productoServicio.RegistrarImagen(
            User.IdUsuario(),
            idProducto,
            archivo,
            textoAlternativo);

        return StatusCode(StatusCodes.Status201Created, imagen);
    }

    // HU-PRD-008
    [HttpPatch("imagenes/{idImagen:int}/principal")]
    public async Task<IActionResult> MarcarImagenPrincipal(int idImagen)
    {
        await _productoServicio.MarcarImagenPrincipal(User.IdUsuario(), idImagen);

        return Ok(new
        {
            mensaje = "La imagen quedó como principal."
        });
    }

    // HU-PRD-009. Baja logica; si era la principal el SP promueve otra.
    // El archivo fisico se conserva (DEC-006)
    [HttpPatch("imagenes/{idImagen:int}/estado")]
    public async Task<IActionResult> DesactivarImagen(int idImagen)
    {
        await _productoServicio.DesactivarImagen(User.IdUsuario(), idImagen);

        return Ok(new
        {
            mensaje = "La imagen fue desactivada."
        });
    }

    // Con sesion, el IdUsuario del token; sin sesion, null
    private int? UsuarioActual()
    {
        return User.Identity?.IsAuthenticated == true && User.FindFirst("IdUsuario") is not null
            ? User.IdUsuario()
            : null;
    }





}

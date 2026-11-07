
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





}

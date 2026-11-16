using LEN_API.Infraestructura;
using LEN_API.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LEN_API.Controllers;

// BT-07: las operaciones son exclusivamente del cliente autenticado.
// El identificador del usuario sale del JWT, nunca de la URL o del formulario.
[ApiController]
[Authorize(Roles = "Cliente")]
[Route("api/favoritos")]
public class FavoritosController : ControllerBase
{
    private readonly IFavoritoServicio _servicio;

    public FavoritosController(IFavoritoServicio servicio)
    {
        _servicio = servicio;
    }

    [HttpGet("mios")]
    public async Task<IActionResult> ListarMios()
    {
        return Ok(await _servicio.ListarPorUsuario(User.IdUsuario()));
    }

    [HttpPost("{idProducto:int}")]
    public async Task<IActionResult> Agregar(int idProducto)
    {
        await _servicio.Agregar(User.IdUsuario(), idProducto);
        return Ok(new { mensaje = "Producto agregado a favoritos." });
    }

    [HttpDelete("{idProducto:int}")]
    public async Task<IActionResult> Eliminar(int idProducto)
    {
        await _servicio.Eliminar(User.IdUsuario(), idProducto);
        return Ok(new { mensaje = "Producto eliminado de favoritos." });
    }
}
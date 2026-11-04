
using LEN_API.Dtos;
using LEN_API.Infraestructura;
using LEN_API.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LEN_API.Controllers;

[ApiController]
[Authorize(Roles = "Administrador")]
[Route("api/direcciones")]
public class DireccionesController : ControllerBase
{
    private readonly IDireccionServicio _servicio;

    public DireccionesController(IDireccionServicio servicio)
    {
        _servicio = servicio;
    }

    // BT-04. Consultar direcciones de un usuario.
    [HttpGet("usuarios/{idUsuario:int}")]
    public async Task<IActionResult> Listar(
        int idUsuario,
        [FromQuery] bool incluirInactivas = false)
    {
        var direcciones = await _servicio.ListarPorUsuario(
            idUsuario,
            incluirInactivas);

        return Ok(direcciones);
    }

    // BT-04. Consultar una dirección específica.
    [HttpGet("usuarios/{idUsuario:int}/{idDireccion:int}")]
    public async Task<IActionResult> Obtener(
        int idUsuario,
        int idDireccion)
    {
        var direccion = await _servicio.ObtenerPorId(
            idDireccion,
            idUsuario);

        if (direccion == null)
        {
            return NotFound(new
            {
                mensaje = "No se encontró la dirección solicitada."
            });
        }

        return Ok(direccion);
    }

    // BT-04. Listar provincias.
    [HttpGet("provincias")]
    public async Task<IActionResult> ListarProvincias()
    {
        var provincias = await _servicio.ListarProvincias();

        return Ok(provincias);
    }

    // BT-04. Listar cantones por provincia.
    [HttpGet("cantones/{idProvincia:int}")]
    public async Task<IActionResult> ListarCantones(int idProvincia)
    {
        var cantones = await _servicio.ListarCantones(idProvincia);

        return Ok(cantones);
    }

    // BT-04. Listar distritos por cantón.
    [HttpGet("distritos/{idCanton:int}")]
    public async Task<IActionResult> ListarDistritos(int idCanton)
    {
        var distritos = await _servicio.ListarDistritos(idCanton);

        return Ok(distritos);
    }



    // BT-04. Registrar una dirección para un usuario.
    [HttpPost("usuarios/{idUsuario:int}")]
    public async Task<IActionResult> Registrar(
        int idUsuario,
        [FromBody] CrearDireccionDto datos)
    {
        datos.IdUsuario = idUsuario;

        int idAdministrador = User.IdUsuario();

        int idDireccion = await _servicio.Registrar(
            idAdministrador,
            datos);

        return StatusCode(201, new
        {
            mensaje = "Dirección registrada correctamente.",
            idDireccion
        });
    }

    // BT-04. Actualizar una dirección.
    [HttpPut("usuarios/{idUsuario:int}/{idDireccion:int}")]
    public async Task<IActionResult> Actualizar(
        int idUsuario,
        int idDireccion,
        [FromBody] ActualizarDireccionDto datos)
    {
        int idAdministrador = User.IdUsuario();

        await _servicio.Actualizar(
            idAdministrador,
            idDireccion,
            idUsuario,
            datos);

        return Ok(new
        {
            mensaje = "Dirección actualizada correctamente."
        });
    }

    // BT-04. Desactivar una dirección.
    [HttpPatch("usuarios/{idUsuario:int}/{idDireccion:int}/estado")]
    public async Task<IActionResult> Desactivar(
        int idUsuario,
        int idDireccion)
    {
        int idAdministrador = User.IdUsuario();

        await _servicio.Desactivar(
            idAdministrador,
            idDireccion,
            idUsuario);

        return Ok(new
        {
            mensaje = "Dirección desactivada correctamente."
        });
    }

}

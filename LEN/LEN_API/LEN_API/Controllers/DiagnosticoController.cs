using LEN_API.Dtos;
using LEN_API.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LEN_API.Controllers;

// Solo para comprobar el esqueleto en desarrollo. Fuera de Development responde 404.
[ApiController]
[Route("api/diagnostico")]
public class DiagnosticoController : ControllerBase
{
    private readonly IDiagnosticoServicio _servicio;
    private readonly IWebHostEnvironment _entorno;

    public DiagnosticoController(IDiagnosticoServicio servicio, IWebHostEnvironment entorno)
    {
        _servicio = servicio;
        _entorno = entorno;
    }

    [AllowAnonymous]
    [HttpGet("estado")]
    public async Task<IActionResult> Estado()
    {
        if (!_entorno.IsDevelopment()) return NotFound();
        return Ok(await _servicio.ObtenerEstado());
    }

    [AllowAnonymous]
    [HttpGet("error-negocio")]
    public async Task<IActionResult> ErrorNegocio()
    {
        if (!_entorno.IsDevelopment()) return NotFound();
        await _servicio.ProvocarErrorDeNegocio();
        return NoContent();
    }

    [AllowAnonymous]
    [HttpGet("error-interno")]
    public async Task<IActionResult> ErrorInterno()
    {
        if (!_entorno.IsDevelopment()) return NotFound();
        await _servicio.ProvocarErrorInterno();
        return NoContent();
    }

    // Prueba del 403 del API: con un token de Cliente tiene que negarse
    [Authorize(Roles = "Administrador")]
    [HttpGet("solo-administrador")]
    public IActionResult SoloAdministrador()
    {
        if (!_entorno.IsDevelopment()) return NotFound();
        return NoContent();
    }

    // Prueba del SMTP sola, sin pasar por la recuperacion, que calla el fallo a proposito
    [AllowAnonymous]
    [HttpGet("correo")]
    public async Task<IActionResult> Correo([FromQuery] string? destino)
    {
        if (!_entorno.IsDevelopment()) return NotFound();

        if (string.IsNullOrWhiteSpace(destino) || !destino.Contains('@'))
            return BadRequest(new RespuestaMensajeDto { Mensaje = "Indique el parametro destino con un correo valido." });

        return Ok(new RespuestaMensajeDto { Mensaje = await _servicio.EnviarCorreoDePrueba(destino) });
    }
}

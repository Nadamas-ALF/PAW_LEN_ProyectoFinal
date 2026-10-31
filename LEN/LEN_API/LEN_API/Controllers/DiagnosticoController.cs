using LEN_API.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LEN_API.Controllers;

// Solo para comprobar el esqueleto en desarrollo. Fuera de Development responde 404.
[ApiController]
[AllowAnonymous]
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

    [HttpGet("estado")]
    public async Task<IActionResult> Estado()
    {
        if (!_entorno.IsDevelopment()) return NotFound();
        return Ok(await _servicio.ObtenerEstado());
    }

    [HttpGet("error-negocio")]
    public async Task<IActionResult> ErrorNegocio()
    {
        if (!_entorno.IsDevelopment()) return NotFound();
        await _servicio.ProvocarErrorDeNegocio();
        return NoContent();
    }

    [HttpGet("error-interno")]
    public async Task<IActionResult> ErrorInterno()
    {
        if (!_entorno.IsDevelopment()) return NotFound();
        await _servicio.ProvocarErrorInterno();
        return NoContent();
    }
}

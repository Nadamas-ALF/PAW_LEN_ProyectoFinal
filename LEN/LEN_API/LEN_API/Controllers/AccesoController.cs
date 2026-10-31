using LEN_API.Dtos;
using LEN_API.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LEN_API.Controllers;

[ApiController]
[Route("api/acceso")]
public class AccesoController : ControllerBase
{
    private readonly IAccesoServicio _servicio;

    public AccesoController(IAccesoServicio servicio)
    {
        _servicio = servicio;
    }

    // HU-ACC-001. 201 con el IdUsuario, o 400 si una regla del procedimiento no se cumple
    [AllowAnonymous]
    [HttpPost("registro")]
    public async Task<IActionResult> Registro([FromBody] RegistroDto datos)
    {
        var idUsuario = await _servicio.Registrar(datos);
        return StatusCode(StatusCodes.Status201Created, new RegistroRespuestaDto { IdUsuario = idUsuario });
    }

    // HU-ACC-002. 200 con la sesion, o 400 con un mensaje que no dice cual de los dos datos fallo
    [AllowAnonymous]
    [HttpPost("iniciar-sesion")]
    public async Task<IActionResult> IniciarSesion([FromBody] IniciarSesionDto datos)
    {
        var sesion = await _servicio.IniciarSesion(datos);

        if (sesion is null)
            return BadRequest(new RespuestaMensajeDto { Mensaje = "El correo o la contraseña no son correctos." });

        return Ok(sesion);
    }

    // Quien es el dueno del token. Sirve para probar que la autenticacion llega al API
    [Authorize]
    [HttpGet("yo")]
    public IActionResult Yo()
    {
        return Ok(new
        {
            IdUsuario = User.FindFirst("IdUsuario")?.Value,
            NombreRol = User.FindFirst("NombreRol")?.Value
        });
    }
}

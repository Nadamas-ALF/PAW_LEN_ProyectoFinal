using LEN_API.Dtos;
using LEN_API.Infraestructura;
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

    // HU-ACC-004. 204 siempre: exista el correo o no, y salga el envio o no (RQNF-SEC-005)
    [AllowAnonymous]
    [HttpPost("recuperacion")]
    public async Task<IActionResult> Recuperacion([FromBody] RecuperacionDto datos)
    {
        await _servicio.Recuperar(datos.Correo);
        return NoContent();
    }

    // HU-ACC-006 y HU-CLI-003. Lo puede llamar una sesion con contrasena temporal pendiente,
    // que es la unica ruta que esa sesion tiene permitida. 200 con la sesion nueva, o 400
    [Authorize(Policy = "CualquierSesion")]
    [HttpPost("cambiar-contrasena")]
    public async Task<IActionResult> CambiarContrasena([FromBody] CambiarContrasenaDto datos)
    {
        return Ok(await _servicio.CambiarContrasena(User.IdUsuario(), datos));
    }

    // Quien es el dueno del token. Sirve para probar que la autenticacion llega al API
    [Authorize(Policy = "CualquierSesion")]
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

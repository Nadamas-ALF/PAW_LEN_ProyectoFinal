using LEN_API.Dtos;
using LEN_API.Infraestructura;
using LEN_API.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LEN_API.Controllers;

[ApiController]
[Authorize(Roles = "Administrador")]
[Route("api/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioServicio _servicio;

    public UsuariosController(IUsuarioServicio servicio)
    {
        _servicio = servicio;
    }

    // HU-USR-005. 200 con la vigencia y si el correo salio, o 400 si el usuario no existe o esta inactivo.
    // Si el cuerpo no trae contrasena, la genera el API y la devuelve una sola vez
    [HttpPost("{id:int}/contrasena-temporal")]
    public async Task<IActionResult> ContrasenaTemporal(int id, [FromBody] ContrasenaTemporalDto datos)
    {
        var respuesta = await _servicio.AsignarContrasenaTemporal(User.IdUsuario(), id, datos);
        return Ok(respuesta);
    }
}

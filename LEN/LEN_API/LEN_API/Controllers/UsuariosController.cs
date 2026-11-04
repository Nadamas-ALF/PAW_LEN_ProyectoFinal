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

    // HU-USR-001. Consultar todos los usuarios.
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var usuarios = await _servicio.Listar();

        return Ok(usuarios);
    }

    [HttpPost]
    public async Task<IActionResult> RegistrarDesdeAdministracion(
    [FromBody] CrearUsuarioAdminDto datos)
    {
        int idAdministrador = User.IdUsuario();

        int idUsuario = await _servicio.RegistrarDesdeAdministracion(
            idAdministrador,
            datos);

        return StatusCode(201, new
        {
            mensaje = "Usuario registrado correctamente.",
            idUsuario
        });
    }



    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(
    int id,
    [FromBody] ActualizarUsuarioAdminDto datos)
    {
        int idAdministrador = User.IdUsuario();

        await _servicio.ActualizarDesdeAdministracion(
            idAdministrador,
            id,
            datos);

        return Ok(new
        {
            mensaje = "Usuario actualizado correctamente."
        });
    }


    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarActivo(int id)
    {
        int idAdministrador = User.IdUsuario();

        await _servicio.CambiarActivo(idAdministrador, id);

        return Ok(new
        {
            mensaje = "Estado del usuario actualizado correctamente."
        });
    }
}

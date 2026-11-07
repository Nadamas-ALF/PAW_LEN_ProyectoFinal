using LEN_API.Dtos;
using LEN_API.Infraestructura;
using LEN_API.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LEN_API.Controllers;

// BT-02: el rol se pide en cada accion y no en la clase, porque "yo" es del
// Cliente. Todas las demas siguen siendo solo del Administrador
[ApiController]
[Authorize]
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
    [Authorize(Roles = "Administrador")]
    [HttpPost("{id:int}/contrasena-temporal")]
    public async Task<IActionResult> ContrasenaTemporal(int id, [FromBody] ContrasenaTemporalDto datos)
    {
        var respuesta = await _servicio.AsignarContrasenaTemporal(User.IdUsuario(), id, datos);
        return Ok(respuesta);
    }

    // HU-USR-001. Consultar todos los usuarios.
    [Authorize(Roles = "Administrador")]
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var usuarios = await _servicio.Listar();

        return Ok(usuarios);
    }

    [Authorize(Roles = "Administrador")]
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



    [Authorize(Roles = "Administrador")]
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


    [Authorize(Roles = "Administrador")]
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

    // BT-02. El cliente consulta sus propios datos. El IdUsuario sale del
    // token, asi que no hay forma de pedir los de otro
    [Authorize(Roles = "Cliente")]
    [HttpGet("yo")]
    public async Task<IActionResult> ObtenerCuenta()
    {
        var cuenta = await _servicio.ObtenerCuenta(User.IdUsuario());

        if (cuenta is null)
            return NotFound(new RespuestaMensajeDto { Mensaje = "No se encontró la cuenta." });

        return Ok(cuenta);
    }

    // BT-02. SP_Usuario_Actualizar. 400 si el correo o la cedula ya son de otro
    [Authorize(Roles = "Cliente")]
    [HttpPut("yo")]
    public async Task<IActionResult> ActualizarCuenta([FromBody] ActualizarCuentaDto datos)
    {
        await _servicio.ActualizarCuenta(User.IdUsuario(), datos);

        return Ok(new
        {
            mensaje = "Sus datos fueron actualizados."
        });
    }
}

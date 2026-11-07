
using LEN_API.Dtos;
using LEN_API.Infraestructura;
using LEN_API.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LEN_API.Controllers;

// BT-02: el rol se pide en cada accion y no en la clase.
//  - usuarios/...  solo Administrador (BT-04), igual que antes
//  - provincias, cantones, distritos: cualquier sesion, para que la misma
//    cascada sirva al cliente y al administrador (CA-5)
//  - mias/...  solo Cliente. El IdUsuario sale siempre del token, asi un
//    cliente no puede tocar direcciones de otro cambiando un numero
[ApiController]
[Authorize]
[Route("api/direcciones")]
public class DireccionesController : ControllerBase
{
    private readonly IDireccionServicio _servicio;

    public DireccionesController(IDireccionServicio servicio)
    {
        _servicio = servicio;
    }

    // BT-04. Consultar direcciones de un usuario.
    [Authorize(Roles = "Administrador")]
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
    [Authorize(Roles = "Administrador")]
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
    [Authorize(Roles = "Administrador")]
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
    [Authorize(Roles = "Administrador")]
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
    [Authorize(Roles = "Administrador")]
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

    // ------------------------------------------------------------
    // BT-02. El cliente sobre sus propias direcciones
    // ------------------------------------------------------------

    // MiCuenta pide solo activas; EditarCuenta todas
    [Authorize(Roles = "Cliente")]
    [HttpGet("mias")]
    public async Task<IActionResult> ListarMias([FromQuery] bool incluirInactivas = false)
    {
        return Ok(await _servicio.ListarPorUsuario(User.IdUsuario(), incluirInactivas));
    }

    // 404 si no existe o es de otro usuario: no se distingue
    [Authorize(Roles = "Cliente")]
    [HttpGet("mias/{idDireccion:int}")]
    public async Task<IActionResult> ObtenerMia(int idDireccion)
    {
        var direccion = await _servicio.ObtenerPorId(idDireccion, User.IdUsuario());

        if (direccion == null)
        {
            return NotFound(new
            {
                mensaje = "No se encontró la dirección solicitada."
            });
        }

        return Ok(direccion);
    }

    // La primera direccion queda principal aunque no se pida (lo hace el SP)
    [Authorize(Roles = "Cliente")]
    [HttpPost("mias")]
    public async Task<IActionResult> RegistrarMia([FromBody] CrearDireccionDto datos)
    {
        int idUsuario = User.IdUsuario();

        // lo que venga en el cuerpo se ignora: el dueno es quien tiene la sesion
        datos.IdUsuario = idUsuario;

        int idDireccion = await _servicio.Registrar(idUsuario, datos);

        return StatusCode(201, new
        {
            mensaje = "Dirección registrada correctamente.",
            idDireccion
        });
    }

    [Authorize(Roles = "Cliente")]
    [HttpPut("mias/{idDireccion:int}")]
    public async Task<IActionResult> ActualizarMia(
        int idDireccion,
        [FromBody] ActualizarDireccionDto datos)
    {
        int idUsuario = User.IdUsuario();

        await _servicio.Actualizar(idUsuario, idDireccion, idUsuario, datos);

        return Ok(new
        {
            mensaje = "Dirección actualizada correctamente."
        });
    }

    // Baja logica. Si era la principal el SP promueve otra
    [Authorize(Roles = "Cliente")]
    [HttpPatch("mias/{idDireccion:int}/estado")]
    public async Task<IActionResult> DesactivarMia(int idDireccion)
    {
        int idUsuario = User.IdUsuario();

        await _servicio.Desactivar(idUsuario, idDireccion, idUsuario);

        return Ok(new
        {
            mensaje = "Dirección desactivada correctamente."
        });
    }

    // SP_Direccion_MarcarPrincipal
    [Authorize(Roles = "Cliente")]
    [HttpPatch("mias/{idDireccion:int}/principal")]
    public async Task<IActionResult> MarcarPrincipalMia(int idDireccion)
    {
        int idUsuario = User.IdUsuario();

        await _servicio.MarcarPrincipal(idUsuario, idDireccion, idUsuario);

        return Ok(new
        {
            mensaje = "La dirección quedó como principal."
        });
    }
}

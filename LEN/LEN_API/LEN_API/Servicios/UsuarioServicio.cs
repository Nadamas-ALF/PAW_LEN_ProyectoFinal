using LEN_API.Dtos;
using LEN_API.Infraestructura;
using LEN_API.Repositorios;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace LEN_API.Servicios;

public class UsuarioServicio : IUsuarioServicio
{
    private readonly IUsuarioRepositorio _repositorio;
    private readonly IPasswordHasher<string> _hasher;
    private readonly IServicioCorreo _correo;
    private readonly IRegistroErrores _registroErrores;
    private readonly OpcionesCorreo _opcionesCorreo;
    private readonly OpcionesContrasenaTemporal _opcionesTemporal;

    public UsuarioServicio(
        IUsuarioRepositorio repositorio,
        IPasswordHasher<string> hasher,
        IServicioCorreo correo,
        IRegistroErrores registroErrores,
        IOptions<OpcionesCorreo> opcionesCorreo,
        IOptions<OpcionesContrasenaTemporal> opcionesTemporal)
    {
        _repositorio = repositorio;
        _hasher = hasher;
        _correo = correo;
        _registroErrores = registroErrores;
        _opcionesCorreo = opcionesCorreo.Value;
        _opcionesTemporal = opcionesTemporal.Value;
    }

    public async Task<ContrasenaTemporalRespuestaDto> AsignarContrasenaTemporal(
        int idAdministrador, int idUsuario, ContrasenaTemporalDto datos)
    {
        var usuario = await _repositorio.ObtenerPorId(idUsuario);

        // el procedimiento lo vuelve a comprobar; aca se necesita el correo y el nombre
        if (usuario is not { Activo: true })
            throw new ExcepcionNegocio("El usuario indicado no existe o está inactivo.");

        var generada = string.IsNullOrEmpty(datos.Contrasena);
        var temporal = generada ? GeneradorContrasena.Temporal() : datos.Contrasena!;
        var vigencia = DateTime.Now.AddHours(_opcionesTemporal.Horas);

        // Aca se asigna primero y se envia despues, al reves que en la recuperacion: el
        // administrador conoce la temporal y puede dictarla si el correo no sale (DEC-L-03)
        await _repositorio.AsignarContrasenaTemporal(
            idAdministrador, idUsuario, _hasher.HashPassword(string.Empty, temporal), vigencia);

        var enviado = true;
        try
        {
            await _correo.Enviar(usuario.Correo, "Su contraseña temporal de LEN Accessories",
                ArmarCuerpo(usuario.Nombre, temporal, vigencia));
        }
        catch (Exception error)
        {
            enviado = false;
            await _registroErrores.Registrar(error, $"api/usuarios/{idUsuario}/contrasena-temporal", null);
        }

        return new ContrasenaTemporalRespuestaDto
        {
            Vigencia = vigencia,
            CorreoEnviado = enviado,
            ContrasenaGenerada = generada ? temporal : null
        };
    }

    private string ArmarCuerpo(string nombre, string temporal, DateTime vigencia) =>
        $"Hola, {nombre}.\n\n" +
        "Un administrador de LEN Accessories restableció el acceso a su cuenta.\n\n" +
        $"Su contraseña temporal es: {temporal}\n" +
        $"Sirve hasta el {vigencia:dd/MM/yyyy HH:mm}. Al entrar le vamos a pedir que escriba una definitiva.\n\n" +
        (string.IsNullOrWhiteSpace(_opcionesCorreo.UrlInicioSesion) ? string.Empty : $"Inicie sesión aquí: {_opcionesCorreo.UrlInicioSesion}\n\n") +
        "Si no esperaba este mensaje, comuníquese con la tienda.\n";
}

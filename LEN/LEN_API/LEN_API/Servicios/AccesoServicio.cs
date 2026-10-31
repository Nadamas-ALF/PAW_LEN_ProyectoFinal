using LEN_API.Datos.Entidades;
using LEN_API.Dtos;
using LEN_API.Infraestructura;
using LEN_API.Repositorios;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace LEN_API.Servicios;

public class AccesoServicio : IAccesoServicio
{
    private readonly IUsuarioRepositorio _repositorio;
    private readonly IPasswordHasher<string> _hasher;
    private readonly IEmisorToken _emisor;
    private readonly IServicioCorreo _correo;
    private readonly IRegistroErrores _registroErrores;
    private readonly OpcionesCorreo _opcionesCorreo;
    private readonly OpcionesContrasenaTemporal _opcionesTemporal;

    // Hash de relleno: cuando el correo no existe se verifica contra este para que
    // responder tarde lo mismo y el tiempo no delate si la cuenta existe
    private readonly string _hashRelleno;

    public AccesoServicio(
        IUsuarioRepositorio repositorio,
        IPasswordHasher<string> hasher,
        IEmisorToken emisor,
        IServicioCorreo correo,
        IRegistroErrores registroErrores,
        IOptions<OpcionesCorreo> opcionesCorreo,
        IOptions<OpcionesContrasenaTemporal> opcionesTemporal)
    {
        _repositorio = repositorio;
        _hasher = hasher;
        _emisor = emisor;
        _correo = correo;
        _registroErrores = registroErrores;
        _opcionesCorreo = opcionesCorreo.Value;
        _opcionesTemporal = opcionesTemporal.Value;
        _hashRelleno = hasher.HashPassword(string.Empty, Guid.NewGuid().ToString());
    }

    public async Task<int> Registrar(RegistroDto datos)
    {
        var hash = _hasher.HashPassword(string.Empty, datos.Contrasena);
        return await _repositorio.Registrar(datos, hash);
    }

    public async Task<SesionDto?> IniciarSesion(IniciarSesionDto datos)
    {
        var usuario = await _repositorio.ObtenerPorCorreo(datos.Correo.Trim().ToLowerInvariant());

        var hashAComparar = usuario is { Activo: true } ? usuario.ContrasenaHash : _hashRelleno;
        var resultado = _hasher.VerifyHashedPassword(string.Empty, hashAComparar, datos.Contrasena);

        if (usuario is not { Activo: true } || resultado == PasswordVerificationResult.Failed)
            return null;

        // HU-ACC-005. Vencida significa vencida: no entra, y hay que pedir otra
        if (TemporalVencida(usuario))
            return null;

        return ArmarSesion(usuario);
    }

    public async Task Recuperar(string correo)
    {
        var usuario = await _repositorio.ObtenerPorCorreo(correo.Trim().ToLowerInvariant());
        if (usuario is not { Activo: true }) return;

        var temporal = GeneradorContrasena.Temporal();
        var vigencia = DateTime.Now.AddHours(_opcionesTemporal.Horas);

        try
        {
            // El envio va primero. Si falla, la contrasena vieja sigue sirviendo: nadie esta
            // presente para rescatar a quien quedara fuera de su cuenta (DEC-L-03)
            await _correo.Enviar(usuario.Correo, "Su contraseña temporal de LEN Accessories",
                ArmarCuerpo(usuario.Nombre, temporal, vigencia));

            await _repositorio.AsignarContrasenaTemporal(
                usuario.IdUsuario, usuario.IdUsuario, _hasher.HashPassword(string.Empty, temporal), vigencia);
        }
        catch (Exception error)
        {
            // el que llama responde igual: decir que fallo confirmaria que el correo esta registrado
            await _registroErrores.Registrar(error, "api/acceso/recuperacion", null);
        }
    }

    public async Task<SesionDto> CambiarContrasena(int idUsuario, CambiarContrasenaDto datos)
    {
        var usuario = await _repositorio.ObtenerPorId(idUsuario);

        if (usuario is not { Activo: true })
            throw new ExcepcionNegocio("El usuario indicado no existe o está inactivo.");

        if (usuario.TieneContrasenaTemporal)
        {
            if (TemporalVencida(usuario))
                throw new ExcepcionNegocio("La contraseña temporal venció. Solicite una nueva.");
        }
        else if (string.IsNullOrEmpty(datos.ContrasenaActual)
                 || _hasher.VerifyHashedPassword(string.Empty, usuario.ContrasenaHash, datos.ContrasenaActual) == PasswordVerificationResult.Failed)
        {
            throw new ExcepcionNegocio("La contraseña actual no es correcta.");
        }

        // si la nueva fuera igual a la temporal, el cambio obligatorio no cambiaria nada
        if (_hasher.VerifyHashedPassword(string.Empty, usuario.ContrasenaHash, datos.Contrasena) != PasswordVerificationResult.Failed)
            throw new ExcepcionNegocio("La nueva contraseña debe ser distinta de la actual.");

        await _repositorio.RestablecerContrasena(idUsuario, idUsuario, _hasher.HashPassword(string.Empty, datos.Contrasena));

        // se vuelve a leer: la sesion nueva tiene que salir de lo que quedo en la base
        var actualizado = await _repositorio.ObtenerPorId(idUsuario);
        return ArmarSesion(actualizado!);
    }

    private static bool TemporalVencida(TB_USUARIO usuario) =>
        usuario.TieneContrasenaTemporal
        && usuario.VigenciaContrasenaTemporal is { } vigencia
        && vigencia < DateTime.Now;

    private SesionDto ArmarSesion(TB_USUARIO usuario)
    {
        var (token, expira) = _emisor.Emitir(usuario);

        return new SesionDto
        {
            Token = token,
            Expira = expira,
            IdUsuario = usuario.IdUsuario,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Correo = usuario.Correo,
            IdRol = usuario.IdRol,
            NombreRol = usuario.IdRolNavigation.Nombre,
            TieneContrasenaTemporal = usuario.TieneContrasenaTemporal
        };
    }

    private string ArmarCuerpo(string nombre, string temporal, DateTime vigencia) =>
        $"Hola, {nombre}.\n\n" +
        "Recibimos una solicitud para recuperar el acceso a su cuenta de LEN Accessories.\n\n" +
        $"Su contraseña temporal es: {temporal}\n" +
        $"Sirve hasta el {vigencia:dd/MM/yyyy HH:mm}. Al entrar le vamos a pedir que escriba una definitiva.\n\n" +
        (string.IsNullOrWhiteSpace(_opcionesCorreo.UrlInicioSesion) ? string.Empty : $"Inicie sesión aquí: {_opcionesCorreo.UrlInicioSesion}\n\n") +
        "Si usted no lo solicitó, puede ignorar este mensaje.\n";
}

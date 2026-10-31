using LEN_API.Dtos;
using LEN_API.Infraestructura;
using LEN_API.Repositorios;
using Microsoft.AspNetCore.Identity;

namespace LEN_API.Servicios;

public class AccesoServicio : IAccesoServicio
{
    private readonly IUsuarioRepositorio _repositorio;
    private readonly IPasswordHasher<string> _hasher;
    private readonly IEmisorToken _emisor;

    // Hash de relleno: cuando el correo no existe se verifica contra este para que
    // responder tarde lo mismo y el tiempo no delate si la cuenta existe
    private readonly string _hashRelleno;

    public AccesoServicio(IUsuarioRepositorio repositorio, IPasswordHasher<string> hasher, IEmisorToken emisor)
    {
        _repositorio = repositorio;
        _hasher = hasher;
        _emisor = emisor;
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
}

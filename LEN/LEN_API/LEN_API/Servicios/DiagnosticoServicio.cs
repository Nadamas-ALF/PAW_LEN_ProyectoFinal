using LEN_API.Dtos;
using LEN_API.Infraestructura;
using LEN_API.Repositorios;
using Microsoft.Extensions.Options;

namespace LEN_API.Servicios;

public class DiagnosticoServicio : IDiagnosticoServicio
{
    private readonly IDiagnosticoRepositorio _repositorio;
    private readonly IServicioCorreo _correo;
    private readonly OpcionesCorreo _opciones;

    public DiagnosticoServicio(
        IDiagnosticoRepositorio repositorio,
        IServicioCorreo correo,
        IOptions<OpcionesCorreo> opciones)
    {
        _repositorio = repositorio;
        _correo = correo;
        _opciones = opciones.Value;
    }

    public Task<EstadoBaseDto> ObtenerEstado() => _repositorio.ObtenerEstado();
    public Task ProvocarErrorDeNegocio() => _repositorio.ProvocarErrorDeNegocio();
    public Task ProvocarErrorInterno() => _repositorio.ProvocarErrorInterno();

    // Aisla el envio del flujo de recuperacion, que por diseno nunca dice si fallo. Si el
    // SMTP esta mal el error sube y el middleware lo deja en TB_ERROR con el mensaje real
    public async Task<string> EnviarCorreoDePrueba(string destino)
    {
        await _correo.Enviar(
            destino,
            "Prueba de configuracion de LEN Accessories",
            "Si este mensaje llego, el envio de contrasenas temporales esta configurado.");

        return _opciones.EstaConfigurado
            ? $"Enviado a {destino} por {_opciones.Servidor}:{_opciones.Puerto}."
            : "La seccion Correo no esta configurada: el cuerpo quedo en el log del API.";
    }
}

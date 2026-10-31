using LEN_API.Datos;
using LEN_API.Datos.Entidades;

namespace LEN_API.Infraestructura;

public class RegistroErrores : IRegistroErrores
{
    private readonly IDbContextFactory<ContextoLen> _fabrica;
    private readonly ILogger<RegistroErrores> _log;

    public RegistroErrores(IDbContextFactory<ContextoLen> fabrica, ILogger<RegistroErrores> log)
    {
        _fabrica = fabrica;
        _log = log;
    }

    public async Task<int?> Registrar(Exception excepcion, string url, string? usuario)
    {
        try
        {
            // contexto propio: no comparte conexion ni transaccion con la peticion,
            // asi un rollback no se lleva el registro
            await using var contexto = await _fabrica.CreateDbContextAsync();

            var error = new TB_ERROR
            {
                Origen = Recortar(excepcion.TargetSite?.DeclaringType?.FullName ?? excepcion.Source, 150),
                Metodo = Recortar(excepcion.TargetSite?.Name, 150),
                MensajeError = excepcion.Message,
                DetalleError = excepcion.InnerException?.Message,
                UsuarioSistema = Recortar(usuario, 150),
                Url = Recortar(url, 500),
                RastroPila = excepcion.StackTrace
            };

            contexto.TB_ERROR.Add(error);
            await contexto.SaveChangesAsync();
            return error.IdError;
        }
        catch (Exception fallo)
        {
            // si la base esta caida no hay donde escribir; queda en el log del proceso
            _log.LogError(fallo, "No se pudo escribir TB_ERROR. Error original: {Mensaje}", excepcion.Message);
            return null;
        }
    }

    private static string? Recortar(string? texto, int maximo) =>
        texto is { Length: > 0 } && texto.Length > maximo ? texto[..maximo] : texto;
}

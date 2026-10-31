using LEN_API.Dtos;

namespace LEN_API.Infraestructura;

public class MiddlewareErrores
{
    private readonly RequestDelegate _siguiente;

    public MiddlewareErrores(RequestDelegate siguiente)
    {
        _siguiente = siguiente;
    }

    public async Task Invoke(HttpContext contexto, IRegistroErrores registro)
    {
        try
        {
            await _siguiente(contexto);
        }
        catch (Exception excepcion) when (!contexto.RequestAborted.IsCancellationRequested
                                          && !contexto.Response.HasStarted)
        {
            contexto.Response.Clear();

            if (TraductorSqlException.EsErrorDeNegocio(excepcion, out var mensaje))
            {
                contexto.Response.StatusCode = StatusCodes.Status400BadRequest;
                await contexto.Response.WriteAsJsonAsync(new RespuestaMensajeDto { Mensaje = mensaje });
                return;
            }

            // solo la ruta: el query string puede traer datos que no van al registro
            var id = await registro.Registrar(excepcion, contexto.Request.Path, contexto.User.Identity?.Name);

            contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await contexto.Response.WriteAsJsonAsync(new RespuestaErrorDto
            {
                Mensaje = "Ocurrió un error inesperado. Intenta de nuevo más tarde.",
                CodigoReferencia = id
            });
        }
    }
}

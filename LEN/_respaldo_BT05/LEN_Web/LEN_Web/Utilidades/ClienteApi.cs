using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace LEN_Web.Utilidades
{
    public class ClienteApi : IClienteApi
    {
        private static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _contexto;
        private readonly ILogger<ClienteApi> _log;

        public ClienteApi(HttpClient http, IHttpContextAccessor contexto, ILogger<ClienteApi> log)
        {
            _http = http;
            _contexto = contexto;
            _log = log;
        }

        public async Task<ResultadoApi<T>> Obtener<T>(string ruta, CancellationToken cancelacion = default)
        {
            return await Enviar<T>(HttpMethod.Get, ruta, null, cancelacion);
        }

        public async Task<ResultadoApi> Enviar(HttpMethod metodo, string ruta, object? cuerpo = null, CancellationToken cancelacion = default)
        {
            var (respuesta, fallo) = await Llamar(metodo, ruta, cuerpo, cancelacion);
            if (respuesta is null) return fallo!;

            using (respuesta)
            {
                var basico = await Traducir(respuesta, cancelacion);
                return new ResultadoApi
                {
                    Tipo = basico.Tipo, Estado = basico.Estado,
                    Mensaje = basico.Mensaje, CodigoReferencia = basico.CodigoReferencia
                };
            }
        }

        public async Task<ResultadoApi<T>> Enviar<T>(HttpMethod metodo, string ruta, object? cuerpo = null, CancellationToken cancelacion = default)
        {
            var (respuesta, fallo) = await Llamar(metodo, ruta, cuerpo, cancelacion);
            if (respuesta is null)
            {
                return new ResultadoApi<T>
                {
                    Tipo = fallo!.Tipo, Estado = fallo.Estado,
                    Mensaje = fallo.Mensaje, CodigoReferencia = fallo.CodigoReferencia
                };
            }

            using (respuesta)
            {
                var basico = await Traducir(respuesta, cancelacion);
                T? datos = default;

                if (basico.Tipo == TipoResultadoApi.Exito && respuesta.Content.Headers.ContentLength != 0)
                {
                    try
                    {
                        datos = await respuesta.Content.ReadFromJsonAsync<T>(OpcionesJson, cancelacion);
                    }
                    catch (JsonException error)
                    {
                        // el API contesto 2xx con algo que no es lo esperado: es un fallo, no un exito vacio
                        _log.LogError(error, "Respuesta del API no legible en {Ruta}", ruta);
                        return new ResultadoApi<T> { Tipo = TipoResultadoApi.Fallo, Estado = (int)respuesta.StatusCode };
                    }
                }

                return new ResultadoApi<T>
                {
                    Tipo = basico.Tipo, Estado = basico.Estado,
                    Mensaje = basico.Mensaje, CodigoReferencia = basico.CodigoReferencia,
                    Datos = datos
                };
            }
        }

        private async Task<(HttpResponseMessage? Respuesta, ResultadoApi? Fallo)> Llamar(
            HttpMethod metodo, string ruta, object? cuerpo, CancellationToken cancelacion)
        {
            try
            {
                using var peticion = new HttpRequestMessage(metodo, ruta);
                if (cuerpo is not null) peticion.Content = JsonContent.Create(cuerpo, options: OpcionesJson);

                var token = await ObtenerToken();
                if (token is not null) peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var respuesta = await _http.SendAsync(peticion, cancelacion);
                return (respuesta, null);
            }
            catch (Exception error) when (error is HttpRequestException or TaskCanceledException && !cancelacion.IsCancellationRequested)
            {
                // API caido, sin red o sin respuesta a tiempo
                _log.LogError(error, "No se pudo llamar al API en {Ruta}", ruta);
                return (null, new ResultadoApi { Tipo = TipoResultadoApi.Fallo, Estado = 0 });
            }
        }

        // El JWT vive en la cookie de autenticacion (HttpOnly), nunca en JavaScript.
        // Sin sesion abierta no hay token y la llamada va anonima
        private async Task<string?> ObtenerToken()
        {
            var contexto = _contexto.HttpContext;
            if (contexto?.User.Identity?.IsAuthenticated != true) return null;
            return await contexto.GetTokenAsync("access_token");
        }

        private static async Task<ResultadoApi> Traducir(HttpResponseMessage respuesta, CancellationToken cancelacion)
        {
            var estado = (int)respuesta.StatusCode;

            if (respuesta.IsSuccessStatusCode)
                return new ResultadoApi { Tipo = TipoResultadoApi.Exito, Estado = estado };

            var (mensaje, codigo) = await LeerError(respuesta, cancelacion);

            return respuesta.StatusCode switch
            {
                HttpStatusCode.BadRequest => new ResultadoApi { Tipo = TipoResultadoApi.Invalido, Estado = estado, Mensaje = mensaje },
                HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
                    new ResultadoApi { Tipo = TipoResultadoApi.SinPermiso, Estado = estado },
                HttpStatusCode.NotFound => new ResultadoApi { Tipo = TipoResultadoApi.NoEncontrado, Estado = estado },
                // el mensaje de un 500 es generico y no se muestra: lo que sale en pantalla es el codigo
                _ => new ResultadoApi { Tipo = TipoResultadoApi.Fallo, Estado = estado, CodigoReferencia = codigo }
            };
        }

        private static async Task<(string? Mensaje, int? Codigo)> LeerError(HttpResponseMessage respuesta, CancellationToken cancelacion)
        {
            try
            {
                using var documento = await JsonDocument.ParseAsync(await respuesta.Content.ReadAsStreamAsync(cancelacion), cancellationToken: cancelacion);
                var raiz = documento.RootElement;
                if (raiz.ValueKind != JsonValueKind.Object) return (null, null);

                string? mensaje = raiz.TryGetProperty("mensaje", out var m) && m.ValueKind == JsonValueKind.String ? m.GetString() : null;
                int? codigo = raiz.TryGetProperty("codigoReferencia", out var c) && c.ValueKind == JsonValueKind.Number ? c.GetInt32() : null;
                return (mensaje, codigo);
            }
            catch (JsonException)
            {
                // cuerpo vacio o que no es JSON, como el de un 401
                return (null, null);
            }
        }
    }
}

namespace LEN_Web.Utilidades
{
    // Unico lugar del Web que habla con el API. Las rutas son relativas a Valores:UrlAPI
    // y van sin barra inicial: "acceso/registro". Nunca lanza por un fallo del API;
    // lo devuelve como ResultadoApi de tipo Fallo
    public interface IClienteApi
    {
        Task<ResultadoApi<T>> Obtener<T>(string ruta, CancellationToken cancelacion = default);

        Task<ResultadoApi> Enviar(HttpMethod metodo, string ruta, object? cuerpo = null, CancellationToken cancelacion = default);

        Task<ResultadoApi<T>> Enviar<T>(HttpMethod metodo, string ruta, object? cuerpo = null, CancellationToken cancelacion = default);
    }
}

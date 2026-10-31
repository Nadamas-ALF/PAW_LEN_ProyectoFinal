using System.Net;

namespace LEN_Web.Utilidades
{
    // Los casos que le importan a un controlador. El codigo HTTP crudo no sale del ClienteApi
    public enum TipoResultadoApi
    {
        Exito,
        // 400: regla de negocio o dato invalido. Mensaje se puede mostrar al usuario
        Invalido,
        // 401 o 403: Estado dice cual de los dos
        SinPermiso,
        // 404: no existe, o existe y no es del usuario que pregunta
        NoEncontrado,
        // 500, API caido o sin respuesta. CodigoReferencia es el IdError, si lo hubo
        Fallo
    }

    public class ResultadoApi
    {
        public TipoResultadoApi Tipo { get; init; }

        public int Estado { get; init; }

        // Texto plano tal como lo manda el API. Para pintarlo en un aviso usar MensajeHtml
        public string? Mensaje { get; init; }

        public int? CodigoReferencia { get; init; }

        public bool EsExito => Tipo == TipoResultadoApi.Exito;

        // Los avisos se pintan con Html.Raw, asi que lo que viene del API se escapa antes
        public string? MensajeHtml => Mensaje is null ? null : WebUtility.HtmlEncode(Mensaje);
    }

    public class ResultadoApi<T> : ResultadoApi
    {
        public T? Datos { get; init; }
    }
}

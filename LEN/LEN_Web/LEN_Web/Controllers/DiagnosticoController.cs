using LEN_Web.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Controllers
{
    // Solo para comprobar en desarrollo el recorrido completo: ClienteApi, API y avisos.
    // Fuera de Development responde 404
    [AllowAnonymous]
    public class DiagnosticoController : Controller
    {
        private readonly IClienteApi _api;
        private readonly IWebHostEnvironment _entorno;

        public DiagnosticoController(IClienteApi api, IWebHostEnvironment entorno)
        {
            _api = api;
            _entorno = entorno;
        }

        // caso: estado, negocio, interno, pendiente o aviso. retorno: ruta local donde mostrar el aviso
        [HttpGet]
        public async Task<IActionResult> Probar(string caso, string? retorno)
        {
            if (!_entorno.IsDevelopment()) return NotFound();

            switch (caso)
            {
                case "estado":
                    var estado = await _api.Obtener<EstadoApi>("diagnostico/estado");
                    Mostrar(estado, $"Base conectada: {estado.Datos?.BaseDeDatos}, {estado.Datos?.Roles} roles.");
                    break;
                case "negocio":
                    Mostrar(await _api.Enviar(HttpMethod.Get, "diagnostico/error-negocio"), "Sin error.");
                    break;
                case "interno":
                    Mostrar(await _api.Enviar(HttpMethod.Get, "diagnostico/error-interno"), "Sin error.");
                    break;
                case "pendiente":
                    TempData["MensajePendiente"] = "Esta acci&oacute;n todav&iacute;a no est&aacute; conectada al servidor.";
                    break;
                case "aviso":
                    TempData["MensajeAviso"] = "Este es un aviso informativo.";
                    break;
                default:
                    return NotFound();
            }

            return LocalRedirect(Url.IsLocalUrl(retorno) ? retorno! : "/Publico/Catalogo");
        }

        private void Mostrar(ResultadoApi resultado, string textoExito)
        {
            switch (resultado.Tipo)
            {
                case TipoResultadoApi.Exito:
                    TempData["MensajeExito"] = textoExito;
                    break;
                case TipoResultadoApi.Invalido:
                    TempData["MensajeError"] = resultado.MensajeHtml;
                    break;
                case TipoResultadoApi.SinPermiso:
                    TempData["MensajeAviso"] = "No tiene permiso para esta acci&oacute;n.";
                    break;
                case TipoResultadoApi.NoEncontrado:
                    TempData["MensajeError"] = "No se encontr&oacute; lo solicitado.";
                    break;
                default:
                    TempData["MensajeError"] = "Ocurri&oacute; un error inesperado." +
                        (resultado.CodigoReferencia is int codigo ? $" C&oacute;digo de referencia: {codigo}." : string.Empty);
                    break;
            }
        }

        private class EstadoApi
        {
            public string BaseDeDatos { get; set; } = string.Empty;
            public int Roles { get; set; }
            public int Distritos { get; set; }
        }
    }
}

namespace LEN_Web.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        // Sera el IdError de TB_ERROR, para que el usuario lo pueda dictar al
        // reportar. Mientras no exista el registro de errores queda nulo y la
        // pantalla no muestra la linea
        public string? CodigoReferencia { get; set; }
    }
}

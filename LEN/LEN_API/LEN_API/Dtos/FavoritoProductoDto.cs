namespace LEN_API.Dtos
{

    // Columnas diseñadas para devolver el SP_Favorito_ListarPorUsuario.
    // El IdUsuario se obtiene siempre del token y no se envia al navegador.
    public class FavoritoProductoDto
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string NombreCategoria { get; set; } = string.Empty;
        public string NombreEstado { get; set; } = string.Empty;
        public string? RutaImagen { get; set; }
        public DateTime FechaAgregado { get; set; }
    }
}// Fin de la clase

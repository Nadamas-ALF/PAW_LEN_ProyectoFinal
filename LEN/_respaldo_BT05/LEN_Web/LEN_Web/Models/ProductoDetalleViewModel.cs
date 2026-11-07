namespace LEN_Web.Models
{
    public class ProductoDetalleViewModel
    {
        public int IdProducto { get; set; }

        public string NombreProducto { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public decimal Precio { get; set; }

        public int Stock { get; set; }

        public bool EsPiezaUnica { get; set; }

        public int IdCategoria { get; set; }

        public string NombreCategoria { get; set; } = string.Empty;

        public bool EsFavorito { get; set; }

        // La principal va primera. Sin imagenes la galeria cae al marcador
        public List<string> Imagenes { get; set; } = new();

        public List<VarianteProducto> Variantes { get; set; } = new();

        // Disponible o Agotado se calcula del stock, no es una columna de la base
        public bool Disponible => Stock > 0;
    }

    public class VarianteProducto
    {
        public int IdVariante { get; set; }

        public string? Color { get; set; }

        public string? Talla { get; set; }

        public string? Modelo { get; set; }

        public int Stock { get; set; }

        // Color talla y modelo pegados con barra, sin los que vengan vacios, y la
        // disponibilidad entre parentesis al final
        public string Etiqueta
        {
            get
            {
                var partes = new[] { Color, Talla, Modelo }
                    .Where(parte => !string.IsNullOrWhiteSpace(parte));

                var nombre = string.Join(" / ", partes);
                var disponibilidad = Stock == 0 ? "agotado" : $"{Stock} disponibles";

                return $"{nombre} ({disponibilidad})";
            }
        }
    }
}

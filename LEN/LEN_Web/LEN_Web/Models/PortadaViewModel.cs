namespace LEN_Web.Models
{
    public class PortadaViewModel
    {
        public List<ProductoDestacado> Destacados { get; set; } = new();
        public List<ProductoResumen> Productos { get; set; } = new();
    }

    public class ProductoDestacado
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string NombreCategoria { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string? RutaImagen { get; set; }

        // BT-05. HU-PRD-010: si viene vacio la vista usa el nombre del producto
        public string? TextoAlternativo { get; set; }
    }

    // BT-05: el JSON de api/productos/catalogo se lee directo a esta clase
    public class ProductoResumen
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string NombreCategoria { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public bool Disponible { get; set; }
        public string? RutaImagen { get; set; }

        // BT-05. Lo que agrega el API. Disponible lo calcula el controlador de Stock
        public int Stock { get; set; }
        public string? TextoAlternativo { get; set; }
        public bool EsFavorito { get; set; }

        // El mismo en todas las filas: el total sin paginar, para TotalPaginas
        public int TotalFilas { get; set; }
    }
}

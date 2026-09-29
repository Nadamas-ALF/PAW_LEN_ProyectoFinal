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
    }

    public class ProductoResumen
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string NombreCategoria { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public bool Disponible { get; set; }
        public string? RutaImagen { get; set; }
    }
}

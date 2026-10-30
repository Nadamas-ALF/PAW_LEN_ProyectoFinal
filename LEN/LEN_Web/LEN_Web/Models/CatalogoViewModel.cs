namespace LEN_Web.Models
{
    public class CatalogoViewModel
    {
        // Los tres filtros vuelven rellenados a la pantalla. Van por GET para que la
        // busqueda se pueda compartir por enlace
        public string? Busqueda { get; set; }

        public int? IdCategoria { get; set; }

        public string? Color { get; set; }

        public int PaginaActual { get; set; } = 1;

        public int TotalPaginas { get; set; } = 1;

        public List<ProductoResumen> Productos { get; set; } = new();

        public (int IdCategoria, string Nombre)[] Categorias { get; set; } = [];

        // Decide cual de los dos estados vacios se muestra. No se encontraron
        // resultados no es lo mismo que todavia no hay productos
        public bool HayFiltros =>
            !string.IsNullOrWhiteSpace(Busqueda)
            || IdCategoria.HasValue
            || !string.IsNullOrWhiteSpace(Color);
    }
}

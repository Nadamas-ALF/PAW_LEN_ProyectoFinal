namespace LEN_Web.Models
{
    // Modelos de las parciales compartidas. Son de presentacion: no salen del API
    // ni de la base, los arma la vista que usa la parcial

    public class ResumenImportesViewModel
    {
        public decimal Subtotal { get; set; }

        public decimal CostoEnvio { get; set; }

        public decimal Total => Subtotal + CostoEnvio;

        // En el checkout es Total y en la confirmacion es Total pagado
        public string EtiquetaTotal { get; set; } = "Total";
    }

    public class InsigniaEstadoViewModel
    {
        public string Etiqueta { get; set; } = string.Empty;

        // Sale de TB_ESTADO_PEDIDO.ClaseCss. La vista no elige el color, asi que
        // agregar un estado en la base no obliga a tocar ninguna pantalla
        public string ClaseCss { get; set; } = string.Empty;

        public string? Clases { get; set; }
    }

    public class ConfirmarModalViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string Titulo { get; set; } = string.Empty;

        public string Cuerpo { get; set; } = string.Empty;

        public string TextoConfirmar { get; set; } = "Confirmar";

        // Para que el JavaScript enganche el boton de confirmar
        public string IdBotonConfirmar { get; set; } = string.Empty;
    }

    public enum VarianteAviso
    {
        Exito,
        Aviso,
        Error,
        Info,
    }

    public class AvisoViewModel
    {
        // Titulo y Cuerpo se pintan con Html.Raw porque el texto de la interfaz
        // lleva las tildes como entidades, igual que en las vistas. Un mensaje que
        // venga del API hay que escaparlo antes de pasarlo aqui
        public string Titulo { get; set; } = string.Empty;

        public string? Cuerpo { get; set; }

        public VarianteAviso Variante { get; set; } = VarianteAviso.Info;

        // Nace oculto y lo muestra el JavaScript con la respuesta del API
        public bool Oculto { get; set; }

        // Agrega la X de Bootstrap para descartarlo
        public bool Cerrable { get; set; }

        public string? Clases { get; set; }
    }

    public class VacioViewModel
    {
        public string Titulo { get; set; } = string.Empty;

        public string Texto { get; set; } = string.Empty;

        // El boton es opcional: hacen falta los dos para que se pinte
        public string? TextoBoton { get; set; }

        public string? UrlBoton { get; set; }

        public string? Clases { get; set; }
    }

    public class PaginacionViewModel
    {
        public int PaginaActual { get; set; } = 1;

        public int TotalPaginas { get; set; } = 1;

        // La ruta sin query, por ejemplo /Publico/Catalogo
        public string Ruta { get; set; } = string.Empty;

        // Los filtros de la pantalla. Cada enlace de pagina los arrastra: sin esto
        // se pierde la busqueda al cambiar de pagina
        public Dictionary<string, string?> Filtros { get; set; } = new();

        public string UrlDe(int pagina)
        {
            var partes = Filtros
                .Where(filtro => !string.IsNullOrWhiteSpace(filtro.Value))
                .Select(filtro => $"{Uri.EscapeDataString(filtro.Key)}={Uri.EscapeDataString(filtro.Value!)}")
                .ToList();

            partes.Add($"pagina={pagina}");

            return $"{Ruta}?{string.Join("&", partes)}";
        }

        // Con muchas paginas no se pintan todas: se recorta una ventana alrededor
        // de la actual. El original pintaba un enlace por pagina y con 100 paginas
        // eso es inusable
        public IEnumerable<int> PaginasVisibles(int maximo = 7)
        {
            if (TotalPaginas <= maximo)
            {
                return Enumerable.Range(1, TotalPaginas);
            }

            var inicio = Math.Max(1, PaginaActual - maximo / 2);
            inicio = Math.Min(inicio, TotalPaginas - maximo + 1);

            return Enumerable.Range(inicio, maximo);
        }
    }

    public enum VarianteTarjeta
    {
        // Agregar al carrito y Ver. El titulo va en h3 porque la seccion ya trae h2
        Portada,

        // Corazon de favorito arriba y sin bloque de acciones
        Catalogo,

        // Quitar de favoritos
        Favoritos,
    }

    public class TarjetaProductoViewModel
    {
        public int IdProducto { get; set; }

        public string NombreProducto { get; set; } = string.Empty;

        public string NombreCategoria { get; set; } = string.Empty;

        public decimal Precio { get; set; }

        public string? RutaImagen { get; set; }

        public bool Disponible { get; set; }

        public bool EsFavorito { get; set; }

        public VarianteTarjeta Variante { get; set; }

        // La direccion a la que vuelve el formulario de favoritos, para no perder
        // los filtros de busqueda del catalogo
        public string? UrlRetorno { get; set; }

        public int NivelTitulo { get; set; } = 3;

        public static TarjetaProductoViewModel Desde(
            ProductoResumen producto,
            VarianteTarjeta variante,
            int nivelTitulo = 3,
            bool esFavorito = false,
            string? urlRetorno = null) => new()
            {
                IdProducto = producto.IdProducto,
                NombreProducto = producto.NombreProducto,
                NombreCategoria = producto.NombreCategoria,
                Precio = producto.Precio,
                RutaImagen = producto.RutaImagen,
                Disponible = producto.Disponible,
                Variante = variante,
                NivelTitulo = nivelTitulo,
                EsFavorito = esFavorito,
                UrlRetorno = urlRetorno,
            };
    }
}

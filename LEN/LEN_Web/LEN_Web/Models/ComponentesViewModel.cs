namespace LEN_Web.Models
{
    // Modelos de las parciales compartidas. Son de presentacion: no salen del API
    // ni de la base, los arma la vista que usa la parcial

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

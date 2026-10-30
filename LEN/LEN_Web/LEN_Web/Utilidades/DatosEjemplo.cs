using LEN_Web.Models;

namespace LEN_Web.Utilidades
{
    // ====================================================================
    // DATOS DE EJEMPLO. Este archivo se borra cuando el API responda.
    //
    // Todo lo que las pantallas muestran mientras no hay endpoints vive aqui
    // y solo aqui. La idea es que al conectar el API se cambien las llamadas
    // y se borre este archivo, sin andar cazando datos sueltos en cada
    // controlador.
    //
    // Los productos salen de una sola lista: portada catalogo y detalle leen
    // de ahi. Si estuvieran escritos aparte se desincronizarian y el mismo
    // producto tendria dos precios segun la pantalla.
    //
    // Lo mismo con los pedidos cuando lleguen: el pedido FAC-000012 de Ana
    // Mora Jimenez vale 25 000 en el historial, el detalle, la factura, los
    // pedidos del panel y los pagos.
    // ====================================================================
    public static class DatosEjemplo
    {
        public const int ProductosPorPagina = 8;

        public static (int IdCategoria, string Nombre)[] CategoriasMenu =>
        [
            (1, "Collares"),
            (2, "Pulseras"),
            (3, "Llaveros"),
            (4, "Aretes"),
        ];

        // ----------------------------------------------------------------
        // Portada
        // ----------------------------------------------------------------

        public static PortadaViewModel Portada(IWebHostEnvironment entorno) => new()
        {
            Destacados = Catalogo
                .Where(producto => producto.ArchivoDestacado is not null)
                .Select(producto => new ProductoDestacado
                {
                    IdProducto = producto.Id,
                    NombreProducto = producto.Nombre,
                    NombreCategoria = producto.Categoria,
                    Precio = producto.Precio,
                    RutaImagen = Imagen(entorno, producto.ArchivoDestacado!),
                })
                .ToList(),

            Productos = Catalogo
                .Take(4)
                .Select(producto => Resumen(entorno, producto))
                .ToList(),
        };

        // ----------------------------------------------------------------
        // Catalogo
        // ----------------------------------------------------------------

        public static CatalogoViewModel CatalogoFiltrado(
            IWebHostEnvironment entorno,
            string? busqueda,
            int? idCategoria,
            string? color,
            int pagina)
        {
            var encontrados = Catalogo.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                encontrados = encontrados.Where(producto =>
                    producto.Nombre.Contains(busqueda.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (idCategoria.HasValue)
            {
                encontrados = encontrados.Where(producto => producto.IdCategoria == idCategoria.Value);
            }

            // El color no esta en el producto sino en sus variantes
            if (!string.IsNullOrWhiteSpace(color))
            {
                encontrados = encontrados.Where(producto => producto.Variantes.Any(variante =>
                    variante.Color is not null
                    && variante.Color.Contains(color.Trim(), StringComparison.OrdinalIgnoreCase)));
            }

            var lista = encontrados.ToList();
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(lista.Count / (double)ProductosPorPagina));
            var paginaSegura = Math.Clamp(pagina, 1, totalPaginas);

            return new CatalogoViewModel
            {
                Busqueda = busqueda,
                IdCategoria = idCategoria,
                Color = color,
                PaginaActual = paginaSegura,
                TotalPaginas = totalPaginas,
                Categorias = CategoriasMenu,
                Productos = lista
                    .Skip((paginaSegura - 1) * ProductosPorPagina)
                    .Take(ProductosPorPagina)
                    .Select(producto => Resumen(entorno, producto))
                    .ToList(),
            };
        }

        // ----------------------------------------------------------------
        // Detalle
        // ----------------------------------------------------------------

        // Devuelve null si el producto no existe. La pantalla responde 404, no
        // una pagina vacia
        public static ProductoDetalleViewModel? Detalle(IWebHostEnvironment entorno, int idProducto)
        {
            var producto = Catalogo.FirstOrDefault(fila => fila.Id == idProducto);

            if (producto is null)
            {
                return null;
            }

            return new ProductoDetalleViewModel
            {
                IdProducto = producto.Id,
                NombreProducto = producto.Nombre,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                Stock = producto.Stock,
                EsPiezaUnica = producto.EsPiezaUnica,
                IdCategoria = producto.IdCategoria,
                NombreCategoria = producto.Categoria,
                Variantes = producto.Variantes.ToList(),
                Imagenes = producto.Archivos
                    .Select(archivo => Imagen(entorno, archivo))
                    .Where(ruta => ruta is not null)
                    .Select(ruta => ruta!)
                    .ToList(),
            };
        }

        // ----------------------------------------------------------------
        // Apoyo
        // ----------------------------------------------------------------

        private static ProductoResumen Resumen(IWebHostEnvironment entorno, ProductoEjemplo producto) => new()
        {
            IdProducto = producto.Id,
            NombreProducto = producto.Nombre,
            NombreCategoria = producto.Categoria,
            Precio = producto.Precio,
            Disponible = producto.Stock > 0,
            RutaImagen = producto.Archivos.Length == 0 ? null : Imagen(entorno, producto.Archivos[0]),
        };

        // Devuelve el nombre del archivo solo si de verdad esta en wwwroot/imagenes.
        // Si falta, la vista cae al LenMarcador en vez de dejar una imagen rota
        public static string? Imagen(IWebHostEnvironment entorno, string archivo)
        {
            var ruta = Path.Combine(entorno.WebRootPath, "imagenes", archivo);
            return File.Exists(ruta) ? archivo : null;
        }

        private record ProductoEjemplo(
            int Id,
            string Nombre,
            int IdCategoria,
            string Categoria,
            decimal Precio,
            int Stock,
            bool EsPiezaUnica,
            string? Descripcion,
            string[] Archivos,
            string? ArchivoDestacado,
            VarianteProducto[] Variantes);

        // Diez productos a proposito: con ocho por pagina el catalogo queda en dos
        // paginas y se puede ver la paginacion funcionando
        private static readonly ProductoEjemplo[] Catalogo =
        [
            new(1, "Collar Bello", 1, "Collares", 8500m, 7, false,
                "Collar artesanal con dije, tejido a mano con hilo encerado y mostacilla checa. Largo ajustable de 40 a 45 cm.",
                ["producto-collar-bello.jpeg", "destacado-collar-bello.jpeg"],
                "destacado-collar-bello.jpeg",
                [
                    new VarianteProducto { IdVariante = 1, Color = "Rosado", Stock = 4 },
                    new VarianteProducto { IdVariante = 2, Color = "Azul", Stock = 3 },
                    new VarianteProducto { IdVariante = 3, Color = "Negro", Talla = "Grande", Stock = 0 },
                ]),

            new(2, "Collar Pato", 1, "Collares", 7000m, 5, false,
                "Collar corto con dije de pato en arcilla polimerica, hecho y pintado a mano.",
                ["producto-collar-pato.jpeg"], null,
                [
                    new VarianteProducto { IdVariante = 4, Color = "Amarillo", Stock = 5 },
                ]),

            new(3, "Pulsera de Perlas", 2, "Pulseras", 5500m, 12, false,
                "Pulsera de perlas de rio con broche de acero quirurgico.",
                ["producto-pulsera-perlas.jpeg", "destacado-pulsera-perlas.jpeg"],
                "destacado-pulsera-perlas.jpeg",
                [
                    new VarianteProducto { IdVariante = 5, Talla = "Pequena", Stock = 6 },
                    new VarianteProducto { IdVariante = 6, Talla = "Mediana", Stock = 6 },
                ]),

            new(4, "Llavero Flor", 3, "Llaveros", 2500m, 0, false,
                "Llavero con flor tejida en crochet y argolla reforzada.",
                ["producto-llavero-flor.jpeg"], null, []),

            new(5, "Aretes Luna", 4, "Aretes", 3800m, 4, false,
                "Aretes de luna en acero dorado, livianos y antialergicos.",
                [], "destacado-aretes-luna.jpeg",
                [
                    new VarianteProducto { IdVariante = 7, Color = "Dorado", Stock = 4 },
                ]),

            new(6, "Collar Trenzado", 1, "Collares", 9200m, 3, true,
                "Pieza unica: collar trenzado en hilo encerado con cuentas de vidrio sopladas a mano.",
                [], null, []),

            new(7, "Pulsera Charms", 2, "Pulseras", 6400m, 9, false,
                "Pulsera base para charms, con cadena de acero y mosqueton.",
                [], null,
                [
                    new VarianteProducto { IdVariante = 8, Color = "Plateado", Stock = 5 },
                    new VarianteProducto { IdVariante = 9, Color = "Dorado", Stock = 4 },
                ]),

            new(8, "Llavero Corazon", 3, "Llaveros", 2800m, 6, false,
                "Llavero de corazon en mostacilla, disponible en varios colores.",
                [], null,
                [
                    new VarianteProducto { IdVariante = 10, Color = "Rojo", Stock = 3 },
                    new VarianteProducto { IdVariante = 11, Color = "Rosado", Stock = 3 },
                ]),

            new(9, "Aretes Gota", 4, "Aretes", 4200m, 0, false,
                "Aretes en forma de gota con bano de oro.",
                [], null, []),

            new(10, "Collar Minimal", 1, "Collares", 6800m, 2, false,
                "Collar fino de cadena con dije pequeno.",
                [], null, []),
        ];
    }
}

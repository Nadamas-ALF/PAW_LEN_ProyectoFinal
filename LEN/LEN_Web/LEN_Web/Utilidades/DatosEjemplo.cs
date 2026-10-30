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
        // Compra: carrito checkout y confirmacion
        // ----------------------------------------------------------------

        // El carrito trae a proposito las cuatro situaciones que la pantalla tiene
        // que saber pintar: linea normal, pieza unica con maximo 1, y linea pedida
        // por encima del stock que bloquea el pago
        public static CarritoViewModel Carrito(IWebHostEnvironment entorno) => new()
        {
            Lineas =
            [
                LineaCarritoDe(entorno, 1, 2),
                LineaCarritoDe(entorno, 3, 1),
                LineaCarritoDe(entorno, 6, 1),
                LineaCarritoDe(entorno, 10, 3),
            ],
        };

        // El checkout muestra solo lo que de verdad se puede comprar, y suma
        // 22 500 mas 2 500 de envio igual a 25 000, que es el total del pedido
        // FAC-000012 que repiten la confirmacion la factura y los pagos
        public static CheckoutViewModel Checkout(IWebHostEnvironment entorno) => new()
        {
            Lineas =
            [
                LineaCarritoDe(entorno, 1, 2),
                LineaCarritoDe(entorno, 3, 1),
            ],
            CostoEnvio = 2500m,
            Direcciones = Direcciones(),
            MetodosPago = MetodosPago(),
        };

        public static ConfirmacionPedidoViewModel Confirmacion(IWebHostEnvironment entorno) => new()
        {
            IdPedido = 12,
            NumeroFactura = "FAC-000012",
            FechaPedido = new DateTime(2026, 9, 20, 15, 42, 0),
            Subtotal = 22500m,
            CostoEnvio = 2500m,
            Total = 25000m,
            EstadoVisible = "Comprada",
            ClaseCssEstado = "estado-pedido-pagado",
            NombreMetodoPago = "Tarjeta",
            ComprobantePago = "4242",
            Direccion = Direcciones()[0],
            Lineas =
            [
                LineaPedidoDe(entorno, 1, 2),
                LineaPedidoDe(entorno, 3, 1),
            ],
        };

        // Las activas, que es lo que ven el checkout y MiCuenta. Los identificadores
        // de canton y distrito son el orden en que los inserta 02_catalogos.sql,
        // porque esas dos tablas usan IDENTITY y el script no los fija
        public static List<DireccionEnvio> Direcciones() =>
        [
            new()
            {
                IdDireccion = 1,
                NombreDestinatario = "Ana Mora Jimenez",
                DireccionExacta = "200 m sur de la escuela, casa verde",
                Referencia = "porton negro, frente al parque",
                Distrito = "Carmen", Canton = "San Jose", Provincia = "San Jose",
                IdProvincia = 1, IdCanton = 1, IdDistrito = 1,
                TelefonoContacto = "8888-0001",
                EsPrincipal = true,
            },
            new()
            {
                IdDireccion = 2,
                NombreDestinatario = "Luis Castro Pineda",
                DireccionExacta = "Oficentro La Sabana, edificio 4, piso 2",
                Distrito = "Mata Redonda", Canton = "San Jose", Provincia = "San Jose",
                IdProvincia = 1, IdCanton = 1, IdDistrito = 8,
                TelefonoContacto = "8888-0002",
            },
        ];

        // ----------------------------------------------------------------
        // Cuenta del cliente
        // ----------------------------------------------------------------

        public static UsuarioCuenta UsuarioActual() => new()
        {
            IdUsuario = 1,
            Nombre = "Ana",
            Apellido = "Mora Jimenez",
            Correo = "ana.mora@example.com",
            Telefono = "8888-0001",
            Cedula = "118880001",
            FechaRegistro = new DateTime(2026, 3, 14),
        };

        public static CuentaViewModel Cuenta() => new()
        {
            Usuario = UsuarioActual(),
            Direcciones = Direcciones(),
        };

        public static EditarCuentaViewModel EdicionCuenta() => new()
        {
            Usuario = UsuarioActual(),
            Direcciones = DireccionesAdministrables(),
            Provincias = Provincias(),
        };

        // EditarCuenta lista todas, activas y no. La segunda viene de baja a
        // proposito: es el unico lugar donde se puede ver ese estado
        public static List<DireccionEnvio> DireccionesAdministrables()
        {
            var lista = Direcciones();
            lista[1].Activo = false;
            return lista;
        }

        // Las 7 de SP_Provincia_Listar
        public static List<OpcionUbicacion> Provincias() =>
        [
            new() { Id = 1, Nombre = "San Jose" },
            new() { Id = 2, Nombre = "Alajuela" },
            new() { Id = 3, Nombre = "Cartago" },
            new() { Id = 4, Nombre = "Heredia" },
            new() { Id = 5, Nombre = "Guanacaste" },
            new() { Id = 6, Nombre = "Puntarenas" },
            new() { Id = 7, Nombre = "Limon" },
        ];

        // Al editar una direccion la cascada llega resuelta. Sin endpoint de
        // catalogos solo se puede mostrar la provincia que ya estaba guardada
        public static List<OpcionUbicacion> CantonesDeEjemplo() =>
        [
            new() { Id = 1, Nombre = "San Jose" },
            new() { Id = 2, Nombre = "Escazu" },
        ];

        public static List<OpcionUbicacion> DistritosDeEjemplo() =>
        [
            new() { Id = 1, Nombre = "Carmen" },
            new() { Id = 2, Nombre = "Merced" },
            new() { Id = 8, Nombre = "Mata Redonda" },
        ];

        // ----------------------------------------------------------------
        // Favoritos
        // ----------------------------------------------------------------

        // El Llavero Flor esta en cero y sirve para ver la cinta de agotado: un
        // favorito agotado sigue en la lista, no desaparece
        public static List<ProductoResumen> Favoritos(IWebHostEnvironment entorno) =>
            new[] { 1, 3, 4 }
                .Select(id => Resumen(entorno, Catalogo.First(fila => fila.Id == id)))
                .ToList();

        // ----------------------------------------------------------------
        // Pedidos del cliente
        // ----------------------------------------------------------------

        public const int PedidosPorPagina = 3;

        public static HistorialPedidosViewModel HistorialPedidos(int pagina)
        {
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(PedidosEjemplo.Count / (double)PedidosPorPagina));
            var paginaSegura = Math.Clamp(pagina, 1, totalPaginas);

            return new HistorialPedidosViewModel
            {
                PaginaActual = paginaSegura,
                TotalPaginas = totalPaginas,
                Pedidos = PedidosEjemplo
                    .Skip((paginaSegura - 1) * PedidosPorPagina)
                    .Take(PedidosPorPagina)
                    .ToList(),
            };
        }

        // Devuelve null si el pedido no es de esta cuenta. La pantalla vuelve al
        // historial con aviso en lugar de mostrar el pedido de otra persona
        public static DetallePedidoViewModel? DetallePedido(IWebHostEnvironment entorno, int idPedido)
        {
            var pedido = PedidosEjemplo.FirstOrDefault(fila => fila.IdPedido == idPedido);

            if (pedido is null)
            {
                return null;
            }

            var cancelado = pedido.IdEstadoPedido == 4;
            var lineas = LineasDe(entorno, pedido.IdPedido);
            var subtotal = lineas.Sum(linea => linea.SubtotalLinea);

            // El envio no es fijo: lo calcula la aplicacion segun la provincia.
            // Se despeja del total para que las tres cifras siempre cuadren
            return new DetallePedidoViewModel
            {
                IdPedido = pedido.IdPedido,
                NumeroFactura = pedido.NumeroFactura,
                FechaPedido = pedido.FechaPedido,
                Subtotal = subtotal,
                CostoEnvio = pedido.Total - subtotal,
                Total = pedido.Total,
                IdEstadoPedido = pedido.IdEstadoPedido,
                EstadoVisible = pedido.EstadoVisible,
                ClaseCssEstado = pedido.ClaseCssEstado,
                MotivoCancelacion = cancelado ? "el cliente pidio la cancelacion." : null,
                Direccion = Direcciones()[0],
                NombreMetodoPago = "Tarjeta",
                ComprobantePago = "4242",
                FechaPago = pedido.FechaPedido,
                EstadoPago = cancelado ? "Anulado" : "Pagado",
                ClaseCssEstadoPago = cancelado ? "LenBadge--Negativo" : "LenBadge--Exito",
                Lineas = lineas,
            };
        }

        // Las unidades de cada uno suman su CantidadProductos del historial
        private static List<LineaPedido> LineasDe(IWebHostEnvironment entorno, int idPedido) => idPedido switch
        {
            12 => [LineaPedidoDe(entorno, 1, 2), LineaPedidoDe(entorno, 3, 1)],
            9 => [LineaPedidoDe(entorno, 2, 1), LineaPedidoDe(entorno, 5, 1)],
            7 => [LineaPedidoDe(entorno, 3, 1)],
            _ => [LineaPedidoDe(entorno, 4, 1)],
        };

        // El FAC-000012 vale 25 000 aqui, en su detalle, en la confirmacion, en
        // la factura y en los pagos del panel. Los cuatro estados de
        // TB_ESTADO_PEDIDO estan representados para ver los cuatro colores
        private static readonly List<PedidoResumen> PedidosEjemplo =
        [
            new()
            {
                IdPedido = 12, NumeroFactura = "FAC-000012",
                FechaPedido = new DateTime(2026, 9, 20, 15, 42, 0),
                Total = 25000m, CantidadProductos = 3,
                IdEstadoPedido = 2, EstadoVisible = "En camino",
                ClaseCssEstado = "estado-pedido-preparacion",
            },
            new()
            {
                IdPedido = 9, NumeroFactura = "FAC-000009",
                FechaPedido = new DateTime(2026, 9, 2, 10, 15, 0),
                Total = 12800m, CantidadProductos = 2,
                IdEstadoPedido = 3, EstadoVisible = "Recibido",
                ClaseCssEstado = "estado-pedido-entregado",
            },
            new()
            {
                IdPedido = 7, NumeroFactura = "FAC-000007",
                FechaPedido = new DateTime(2026, 8, 27, 9, 5, 0),
                Total = 7000m, CantidadProductos = 1,
                IdEstadoPedido = 1, EstadoVisible = "Comprada",
                ClaseCssEstado = "estado-pedido-pagado",
            },
            new()
            {
                IdPedido = 5, NumeroFactura = "FAC-000005",
                FechaPedido = new DateTime(2026, 8, 18, 16, 30, 0),
                Total = 4500m, CantidadProductos = 1,
                IdEstadoPedido = 4, EstadoVisible = "Cancelado",
                ClaseCssEstado = "estado-pedido-cancelado",
            },
        ];

        // Los cuatro que carga 02_catalogos.sql
        public static List<MetodoPago> MetodosPago() =>
        [
            new() { IdMetodoPago = 1, Nombre = "Tarjeta" },
            new() { IdMetodoPago = 2, Nombre = "Transferencia bancaria" },
            new() { IdMetodoPago = 3, Nombre = "SINPE Movil" },
            new() { IdMetodoPago = 4, Nombre = "Efectivo contra entrega" },
        ];

        private static LineaCarrito LineaCarritoDe(IWebHostEnvironment entorno, int idProducto, int cantidad)
        {
            var producto = Catalogo.First(fila => fila.Id == idProducto);

            return new LineaCarrito
            {
                IdProducto = producto.Id,
                NombreProducto = producto.Nombre,
                PrecioUnitario = producto.Precio,
                Cantidad = cantidad,
                StockDisponible = producto.Stock,
                EsPiezaUnica = producto.EsPiezaUnica,
                RutaImagen = producto.Archivos.Length == 0 ? null : Imagen(entorno, producto.Archivos[0]),
            };
        }

        private static LineaPedido LineaPedidoDe(IWebHostEnvironment entorno, int idProducto, int cantidad)
        {
            var producto = Catalogo.First(fila => fila.Id == idProducto);

            return new LineaPedido
            {
                IdProducto = producto.Id,
                NombreProducto = producto.Nombre,
                PrecioUnitario = producto.Precio,
                Cantidad = cantidad,
                RutaImagen = producto.Archivos.Length == 0 ? null : Imagen(entorno, producto.Archivos[0]),
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

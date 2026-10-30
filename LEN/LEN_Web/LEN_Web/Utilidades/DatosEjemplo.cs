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

        // Solo las activas: el menu publico y los filtros no listan una categoria
        // dada de baja
        public static (int IdCategoria, string Nombre)[] CategoriasMenu =>
            CategoriasEjemplo
                .Where(categoria => categoria.Activo)
                .Select(categoria => (categoria.Id, categoria.Nombre))
                .ToArray();

        private record CategoriaEjemplo(int Id, string Nombre, string? Descripcion, bool Activo = true);

        // La quinta esta de baja y sin productos a proposito: es el unico estado
        // que la pantalla de categorias no podria mostrar de otra forma, porque
        // una categoria con productos activos no se puede desactivar
        private static readonly CategoriaEjemplo[] CategoriasEjemplo =
        [
            new(1, "Collares", "Collares artesanales tejidos a mano"),
            new(2, "Pulseras", "Pulseras y charms para bolso"),
            new(3, "Llaveros", null),
            new(4, "Aretes", "Aretes de temporada"),
            new(5, "Dijes", "Dijes sueltos para armar", Activo: false),
        ];

        // ----------------------------------------------------------------
        // Portada
        // ----------------------------------------------------------------

        public static PortadaViewModel Portada(IWebHostEnvironment entorno) => new()
        {
            Destacados = Catalogo
                .Where(producto => producto.Activo
                    && producto.Destacado
                    && producto.ArchivoDestacado is not null)
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
                .Where(producto => producto.Activo)
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
            // Un producto dado de baja no sale en el catalogo publico
            var encontrados = Catalogo.Where(producto => producto.Activo);

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
            // Tampoco se puede abrir el detalle de uno dado de baja
            var producto = Catalogo.FirstOrDefault(fila => fila.Id == idProducto && fila.Activo);

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

        // La cuenta con la que se entra mientras no hay sesion
        private const int IdUsuarioSesion = 1;

        // Solo los pedidos de la cuenta. El procedimiento recibe IdUsuario por
        // eso mismo: el historial de uno no puede mostrar los de otro
        public static HistorialPedidosViewModel HistorialPedidos(int pagina)
        {
            var mios = LibroPedidos
                .Where(pedido => pedido.IdUsuario == IdUsuarioSesion)
                .ToList();

            var totalPaginas = Math.Max(1, (int)Math.Ceiling(mios.Count / (double)PedidosPorPagina));
            var paginaSegura = Math.Clamp(pagina, 1, totalPaginas);

            return new HistorialPedidosViewModel
            {
                PaginaActual = paginaSegura,
                TotalPaginas = totalPaginas,
                Pedidos = mios
                    .Skip((paginaSegura - 1) * PedidosPorPagina)
                    .Take(PedidosPorPagina)
                    .Select(pedido => new PedidoResumen
                    {
                        IdPedido = pedido.Id,
                        NumeroFactura = pedido.NumeroFactura ?? $"Pedido #{pedido.Id}",
                        FechaPedido = pedido.Fecha,
                        Total = TotalDe(pedido),
                        IdEstadoPedido = pedido.IdEstado,
                        EstadoVisible = EstadoVisibleDe(pedido.IdEstado),
                        ClaseCssEstado = EstadosPedido()
                            .First(estado => estado.IdEstadoPedido == pedido.IdEstado).ClaseCss,
                        CantidadProductos = pedido.Lineas.Sum(linea => linea.Cantidad),
                    })
                    .ToList(),
            };
        }

        // Devuelve null si el pedido no es de esta cuenta. La pantalla vuelve al
        // historial con aviso en lugar de mostrar el pedido de otra persona
        public static DetallePedidoViewModel? DetallePedido(IWebHostEnvironment entorno, int idPedido)
        {
            // El procedimiento recibe IdPedido e IdUsuario los dos: sin lo
            // segundo cualquiera veria el pedido de otro cambiando el numero
            var pedido = LibroPedidos.FirstOrDefault(fila =>
                fila.Id == idPedido && fila.IdUsuario == IdUsuarioSesion);

            if (pedido is null)
            {
                return null;
            }

            var cancelado = pedido.IdEstado == 4;

            return new DetallePedidoViewModel
            {
                IdPedido = pedido.Id,
                NumeroFactura = pedido.NumeroFactura ?? $"Pedido #{pedido.Id}",
                FechaPedido = pedido.Fecha,
                Subtotal = SubtotalDe(pedido),
                CostoEnvio = pedido.CostoEnvio,
                Total = TotalDe(pedido),
                IdEstadoPedido = pedido.IdEstado,
                EstadoVisible = EstadoVisibleDe(pedido.IdEstado),
                ClaseCssEstado = EstadosPedido()
                    .First(estado => estado.IdEstadoPedido == pedido.IdEstado).ClaseCss,
                MotivoCancelacion = cancelado ? "el cliente pidio la cancelacion." : null,
                Direccion = Direcciones()[0],
                NombreMetodoPago = MetodoDe(pedido),
                ComprobantePago = pedido.Comprobante,
                FechaPago = pedido.Fecha,
                EstadoPago = cancelado ? "Anulado" : "Pagado",
                ClaseCssEstadoPago = cancelado ? "LenBadge--Negativo" : "LenBadge--Exito",
                Lineas = LineasDe(entorno, pedido),
            };
        }

        // Las unidades de cada linea suman el CantidadProductos del historial
        private static List<LineaPedido> LineasDe(IWebHostEnvironment entorno, PedidoEjemplo pedido) =>
            pedido.Lineas
                .Select(linea => LineaPedidoDe(entorno, linea.IdProducto, linea.Cantidad))
                .ToList();

        // ----------------------------------------------------------------
        // El libro de pedidos
        // ----------------------------------------------------------------
        //
        // Una sola lista, y de ella salen seis pantallas: el historial y el
        // detalle del cliente, la bandeja y el detalle del panel, los pagos y
        // las facturas. El total de un pedido es el mismo numero en las seis
        // porque se calcula de sus lineas, no se escribe.
        //
        // El FAC-000012 vale 25 000 aqui y en la confirmacion de compra, que es
        // el invariante que pide el LEEME de la plantilla. Los cuatro estados de
        // TB_ESTADO_PEDIDO estan representados para ver los cuatro colores, y
        // hay tres clientes para que la bandeja del panel no sea de uno solo.

        private record PedidoEjemplo(
            int Id,
            string? NumeroFactura,
            DateTime Fecha,
            int IdUsuario,
            int IdEstado,
            decimal CostoEnvio,
            int IdMetodoPago,
            string? Comprobante,
            (int IdProducto, int Cantidad)[] Lineas);

        private static readonly PedidoEjemplo[] LibroPedidos =
        [
            new(12, "FAC-000012", new DateTime(2026, 9, 20, 15, 42, 0), 1, 2, 2500m, 1, "4242",
                [(1, 2), (3, 1)]),

            new(11, "FAC-000011", new DateTime(2026, 9, 19, 11, 8, 0), 2, 1, 3500m, 3, "8888-0002",
                [(10, 1)]),

            new(10, "FAC-000010", new DateTime(2026, 9, 10, 14, 20, 0), 3, 3, 2400m, 2, "000123456",
                [(5, 1), (8, 1)]),

            // Trae el Collar Pato, que esta dado de baja: una baja no borra lo
            // que ya se vendio
            new(9, "FAC-000009", new DateTime(2026, 9, 2, 10, 15, 0), 1, 3, 2000m, 4, "SIM-000009",
                [(2, 1), (5, 1)]),

            new(7, "FAC-000007", new DateTime(2026, 8, 27, 9, 5, 0), 1, 1, 1500m, 1, "1881",
                [(3, 1)]),

            new(5, "FAC-000005", new DateTime(2026, 8, 18, 16, 30, 0), 1, 4, 2000m, 2, "000123456",
                [(4, 1)]),
        ];

        // Cada cliente con su direccion de envio. Sin esto el detalle de un
        // pedido de Luis mostraria a Ana como destinataria
        private record ClienteEjemplo(
            int Id,
            string Nombre,
            string Correo,
            string? Telefono,
            string? Cedula,
            string DireccionExacta,
            string Distrito,
            string Canton,
            string? Referencia);

        private static readonly ClienteEjemplo[] ClientesEjemplo =
        [
            // Los datos de Ana son los mismos de Direcciones()[0], que es lo que
            // usan el checkout y MiCuenta
            new(1, "Ana Mora Jimenez", "ana.mora@example.com", "8888-0001", "118880001",
                "200 m sur de la escuela, casa verde", "Carmen", "San Jose",
                "porton negro, frente al parque"),

            new(2, "Luis Castro Pineda", "luis.castro@example.com", "8888-0002", null,
                "Oficentro La Sabana, edificio 4, piso 2", "Mata Redonda", "San Jose", null),

            // Sin referencia: es opcional y conviene ver el caso
            new(3, "Sofia Ramirez Leon", "sofia.ramirez@example.com", "8888-0003", "207770003",
                "Residencial Los Robles, casa 14", "Escazu", "Escazu", null),
        ];

        private static DireccionEnvio DireccionDe(ClienteEjemplo cliente) => new()
        {
            NombreDestinatario = cliente.Nombre,
            DireccionExacta = cliente.DireccionExacta,
            Referencia = cliente.Referencia,
            Distrito = cliente.Distrito,
            Canton = cliente.Canton,
            Provincia = "San Jose",
            TelefonoContacto = cliente.Telefono,
        };

        // Los cuatro de TB_ESTADO_PEDIDO con el nombre que ve el panel
        public static List<EstadoPedidoOpcion> EstadosPedido() =>
        [
            new() { IdEstadoPedido = 1, Nombre = "Pagado", ClaseCss = "estado-pedido-pagado" },
            new() { IdEstadoPedido = 2, Nombre = "En preparaci&oacute;n", ClaseCss = "estado-pedido-preparacion" },
            new() { IdEstadoPedido = 3, Nombre = "Entregado", ClaseCss = "estado-pedido-entregado" },
            new() { IdEstadoPedido = 4, Nombre = "Cancelado", ClaseCss = "estado-pedido-cancelado" },
        ];

        // Lo que el cliente lee en lugar del nombre del estado
        private static string EstadoVisibleDe(int idEstado) => idEstado switch
        {
            1 => "Comprada",
            2 => "En camino",
            3 => "Recibido",
            _ => "Cancelado",
        };

        // Las transiciones que admite SP_Pedido_CambiarEstado. Entregado y
        // Cancelado son finales y devuelven lista vacia, y con eso la pantalla
        // esconde el formulario entero
        private static List<EstadoPedidoOpcion> EstadosSiguientesDe(int idEstado)
        {
            var validos = idEstado switch
            {
                1 => new[] { 2, 4 },
                2 => new[] { 3, 4 },
                _ => [],
            };

            return EstadosPedido()
                .Where(estado => validos.Contains(estado.IdEstadoPedido))
                .ToList();
        }

        private static decimal SubtotalDe(PedidoEjemplo pedido) =>
            pedido.Lineas.Sum(linea =>
                Catalogo.First(fila => fila.Id == linea.IdProducto).Precio * linea.Cantidad);

        private static decimal TotalDe(PedidoEjemplo pedido) => SubtotalDe(pedido) + pedido.CostoEnvio;

        private static ClienteEjemplo ClienteDe(PedidoEjemplo pedido) =>
            ClientesEjemplo.First(cliente => cliente.Id == pedido.IdUsuario);

        private static string MetodoDe(PedidoEjemplo pedido) =>
            MetodosPago().First(metodo => metodo.IdMetodoPago == pedido.IdMetodoPago).Nombre;

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
        // Panel: tablero
        // ----------------------------------------------------------------

        // Agregados de toda la tienda. El libro de pedidos tiene solo los seis
        // que las tablas muestran en detalle, asi que estas cifras no salen de
        // ahi: son el resumen de una historia mas larga. Viven aca y no en cada
        // pantalla para que el tablero y los pagos no se contradigan
        public const int TotalPedidosHistoricos = 37;

        public const int PedidosCanceladosHistoricos = 4;

        public static TableroViewModel Tablero()
        {
            var ventas = VentasDelAnio();

            return new TableroViewModel
            {
                TotalVentas = ventas.Sum(venta => venta.Total),
                TotalPedidos = TotalPedidosHistoricos,
                PedidosPorPreparar = 8,
                TotalClientesActivos = 18,
                TotalProductosAgotados = Catalogo.Count(producto => producto.Activo && producto.Stock == 0),
                VentasMensuales = ventas,
                ActividadesRecientes =
                [
                    new()
                    {
                        Titulo = "Pedido #37",
                        Descripcion = "Luis Castro Pineda realizo un pedido por " + FormatoMoneda.Colones(18400m),
                        Fecha = new DateTime(2026, 9, 26, 10, 14, 0),
                        Tipo = "Entregado",
                        ClaseCssEstado = "estado-pedido-entregado",
                    },
                    new()
                    {
                        Titulo = "Pedido #36",
                        Descripcion = "Ana Mora Jimenez realizo un pedido por " + FormatoMoneda.Colones(25000m),
                        Fecha = new DateTime(2026, 9, 25, 16, 2, 0),
                        Tipo = "En preparacion",
                        ClaseCssEstado = "estado-pedido-preparacion",
                    },
                    new()
                    {
                        Titulo = "Pedido #35",
                        Descripcion = "Sofia Ramirez Leon realizo un pedido por " + FormatoMoneda.Colones(4500m),
                        Fecha = new DateTime(2026, 9, 24, 9, 31, 0),
                        Tipo = "Cancelado",
                        ClaseCssEstado = "estado-pedido-cancelado",
                    },
                ],
            };
        }

        // Los doce meses siempre, con cero donde no hubo ventas. El porcentaje se
        // calcula contra el mes mas alto y se redondea a pasos de 5, con minimo 5
        // si hubo alguna venta: asi diciembre no se ve igual que octubre
        private static List<VentaMensual> VentasDelAnio()
        {
            var meses = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
            var totales = new[] { 42000m, 58000m, 71500m, 64000m, 88300m, 121000m, 96500m, 103000m, 79000m, 0m, 0m, 2500m };
            var mayor = totales.Max();

            return meses
                .Select((mes, indice) => new VentaMensual
                {
                    Mes = mes,
                    Total = totales[indice],
                    Porcentaje = PorcentajeBarra(totales[indice], mayor),
                })
                .ToList();
        }

        private static int PorcentajeBarra(decimal total, decimal mayor)
        {
            if (total <= 0 || mayor <= 0)
            {
                return 0;
            }

            var crudo = (double)(total / mayor) * 100;
            var paso = (int)Math.Round(crudo / 5, MidpointRounding.AwayFromZero) * 5;

            return Math.Max(5, paso);
        }

        // ----------------------------------------------------------------
        // Panel: productos
        // ----------------------------------------------------------------

        public const int ProductosAdminPorPagina = 8;

        // Umbral por defecto de SP_Producto_ListarBajoStock
        public const int UmbralBajoStock = 3;

        public static ListadoProductosViewModel ProductosAdmin(
            IWebHostEnvironment entorno,
            string? busqueda,
            int? idCategoria,
            int pagina)
        {
            // El panel si ve los dados de baja: es el unico lugar donde se pueden
            // volver a activar
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

            var lista = encontrados.ToList();
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(lista.Count / (double)ProductosAdminPorPagina));
            var paginaSegura = Math.Clamp(pagina, 1, totalPaginas);

            return new ListadoProductosViewModel
            {
                Busqueda = busqueda,
                IdCategoria = idCategoria,
                PaginaActual = paginaSegura,
                TotalPaginas = totalPaginas,
                Categorias = CategoriasActivas(),
                Productos = lista
                    .Skip((paginaSegura - 1) * ProductosAdminPorPagina)
                    .Take(ProductosAdminPorPagina)
                    .Select(producto => FilaAdmin(entorno, producto))
                    .ToList(),

                // La alerta mira todo el catalogo, no solo la pagina abierta: si
                // solo contara la pagina, el aviso cambiaria al paginar
                AlertasBajoStock = Catalogo
                    .Where(producto => producto.Activo && producto.Stock <= UmbralBajoStock)
                    .Select(producto => new AlertaStock
                    {
                        NombreProducto = producto.Nombre,
                        Stock = producto.Stock,
                    })
                    .ToList(),
            };
        }

        public static List<CategoriaOpcion> CategoriasActivas() =>
            CategoriasEjemplo
                .Where(categoria => categoria.Activo)
                .Select(categoria => new CategoriaOpcion
                {
                    IdCategoria = categoria.Id,
                    Nombre = categoria.Nombre,
                })
                .ToList();

        public static ProductoFormViewModel FormularioNuevo() => new()
        {
            Categorias = CategoriasActivas(),
        };

        // Null si el producto no existe. Aca si se devuelve uno dado de baja: el
        // panel tiene que poder editarlo
        public static ProductoFormViewModel? FormularioDe(int idProducto)
        {
            var producto = Catalogo.FirstOrDefault(fila => fila.Id == idProducto);

            if (producto is null)
            {
                return null;
            }

            var fila = new ProductoAdmin
            {
                Stock = producto.Stock,
                Activo = producto.Activo,
            };

            return new ProductoFormViewModel
            {
                IdProducto = producto.Id,
                NombreProducto = producto.Nombre,
                IdCategoria = producto.IdCategoria,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                Stock = producto.Stock,
                EsPiezaUnica = producto.EsPiezaUnica,
                Destacado = producto.Destacado,
                Categorias = CategoriasActivas(),
                EsEdicion = true,
                EstadoVisible = fila.EstadoVisible,
                ClaseCssEstado = fila.ClaseCssEstado,
            };
        }

        // Las imagenes salen de los archivos del producto. La primera es la
        // principal, como el indice de la base obliga
        public static ImagenesProductoViewModel? ImagenesDe(IWebHostEnvironment entorno, int idProducto)
        {
            var producto = Catalogo.FirstOrDefault(fila => fila.Id == idProducto);

            if (producto is null)
            {
                return null;
            }

            return new ImagenesProductoViewModel
            {
                IdProducto = producto.Id,
                NombreProducto = producto.Nombre,
                Imagenes = producto.Archivos
                    .Select((archivo, indice) => new ImagenProducto
                    {
                        IdImagen = (producto.Id * 10) + indice + 1,
                        RutaImagen = Imagen(entorno, archivo),
                        EsPrincipal = indice == 0,
                    })
                    .ToList(),
            };
        }

        // ----------------------------------------------------------------
        // Panel: categorias
        // ----------------------------------------------------------------

        // Las dos cuentas se calculan del catalogo: escritas a mano se
        // despegarian de la tabla de productos en cuanto cambie un producto
        public static List<CategoriaAdmin> CategoriasAdmin() =>
            CategoriasEjemplo
                .Select(categoria => new CategoriaAdmin
                {
                    IdCategoria = categoria.Id,
                    Nombre = categoria.Nombre,
                    Descripcion = categoria.Descripcion,
                    Activo = categoria.Activo,
                    CantidadProductos = Catalogo.Count(producto => producto.IdCategoria == categoria.Id),
                    CantidadProductosActivos = Catalogo.Count(producto =>
                        producto.IdCategoria == categoria.Id && producto.Activo),
                })
                .ToList();

        private static ProductoAdmin FilaAdmin(IWebHostEnvironment entorno, ProductoEjemplo producto) => new()
        {
            IdProducto = producto.Id,
            NombreProducto = producto.Nombre,
            NombreCategoria = producto.Categoria,
            Precio = producto.Precio,
            Stock = producto.Stock,
            EsPiezaUnica = producto.EsPiezaUnica,
            Destacado = producto.Destacado,
            Activo = producto.Activo,
            RutaImagen = producto.Archivos.Length == 0 ? null : Imagen(entorno, producto.Archivos[0]),
        };

        // ----------------------------------------------------------------
        // Panel: pedidos
        // ----------------------------------------------------------------

        public const int PedidosAdminPorPagina = 5;

        public static ListadoPedidosAdminViewModel PedidosAdmin(
            string? busqueda,
            int? idEstado,
            string? fechaInicio,
            string? fechaFin,
            int pagina)
        {
            // El panel ve los de todos los clientes, no los de una cuenta
            var encontrados = LibroPedidos.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();
                encontrados = encontrados.Where(pedido =>
                    (pedido.NumeroFactura ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                    || ClienteDe(pedido).Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase)
                    || ClienteDe(pedido).Correo.Contains(texto, StringComparison.OrdinalIgnoreCase));
            }

            if (idEstado.HasValue)
            {
                encontrados = encontrados.Where(pedido => pedido.IdEstado == idEstado.Value);
            }

            encontrados = EntreFechas(encontrados, pedido => pedido.Fecha, fechaInicio, fechaFin);

            var lista = encontrados.ToList();
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(lista.Count / (double)PedidosAdminPorPagina));
            var paginaSegura = Math.Clamp(pagina, 1, totalPaginas);

            return new ListadoPedidosAdminViewModel
            {
                Busqueda = busqueda,
                IdEstado = idEstado,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                PaginaActual = paginaSegura,
                TotalPaginas = totalPaginas,
                Estados = EstadosPedido(),
                Pedidos = lista
                    .Skip((paginaSegura - 1) * PedidosAdminPorPagina)
                    .Take(PedidosAdminPorPagina)
                    .Select(pedido => new PedidoAdminResumen
                    {
                        IdPedido = pedido.Id,
                        NumeroFactura = pedido.NumeroFactura,
                        FechaPedido = pedido.Fecha,
                        Total = TotalDe(pedido),
                        IdEstadoPedido = pedido.IdEstado,
                        NombreEstado = EstadosPedido()
                            .First(estado => estado.IdEstadoPedido == pedido.IdEstado).Nombre,
                        ClaseCssEstado = EstadosPedido()
                            .First(estado => estado.IdEstadoPedido == pedido.IdEstado).ClaseCss,
                        IdUsuario = pedido.IdUsuario,
                        NombreCliente = ClienteDe(pedido).Nombre,
                        CorreoCliente = ClienteDe(pedido).Correo,
                        NombreMetodoPago = MetodoDe(pedido),
                        CantidadProductos = pedido.Lineas.Sum(linea => linea.Cantidad),
                    })
                    .ToList(),
            };
        }

        // Null si el pedido no existe. Aca no se filtra por usuario: el
        // administrador ve cualquier pedido, y por eso el procedimiento de
        // administracion no recibe IdUsuario
        public static DetallePedidoAdminViewModel? DetallePedidoAdmin(IWebHostEnvironment entorno, int idPedido)
        {
            var pedido = LibroPedidos.FirstOrDefault(fila => fila.Id == idPedido);

            if (pedido is null)
            {
                return null;
            }

            var cliente = ClienteDe(pedido);
            var estado = EstadosPedido().First(fila => fila.IdEstadoPedido == pedido.IdEstado);

            return new DetallePedidoAdminViewModel
            {
                IdPedido = pedido.Id,
                NumeroFactura = pedido.NumeroFactura,
                FechaPedido = pedido.Fecha,
                Subtotal = SubtotalDe(pedido),
                CostoEnvio = pedido.CostoEnvio,
                Total = TotalDe(pedido),
                IdEstadoPedido = pedido.IdEstado,
                NombreEstado = estado.Nombre,
                ClaseCssEstado = estado.ClaseCss,
                MotivoCancelacion = pedido.IdEstado == 4 ? "El cliente pidio la cancelacion." : null,
                NombreCliente = cliente.Nombre,
                CorreoCliente = cliente.Correo,
                TelefonoCliente = cliente.Telefono,
                Direccion = DireccionDe(cliente),
                NombreMetodoPago = MetodoDe(pedido),
                ComprobantePago = pedido.Comprobante,
                Lineas = LineasDe(entorno, pedido),
                EstadosSiguientes = EstadosSiguientesDe(pedido.IdEstado),
            };
        }

        // ----------------------------------------------------------------
        // Panel: pagos
        // ----------------------------------------------------------------

        public const int PagosPorPagina = 5;

        public static ListadoPagosViewModel Pagos(
            string? busqueda,
            int? idEstadoPago,
            string? fechaInicio,
            string? fechaFin,
            int pagina)
        {
            var todos = LibroPedidos.Select(PagoDe).ToList();
            var encontrados = todos.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();
                encontrados = encontrados.Where(pago =>
                    pago.NombreCliente.Contains(texto, StringComparison.OrdinalIgnoreCase)
                    || pago.CorreoCliente.Contains(texto, StringComparison.OrdinalIgnoreCase)
                    || (pago.Comprobante ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase)
                    || (pago.NumeroFactura ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase));
            }

            if (idEstadoPago.HasValue)
            {
                encontrados = encontrados.Where(pago => pago.IdEstadoPago == idEstadoPago.Value);
            }

            encontrados = EntreFechas(encontrados, pago => pago.FechaPago, fechaInicio, fechaFin);

            var lista = encontrados.ToList();
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(lista.Count / (double)PagosPorPagina));
            var paginaSegura = Math.Clamp(pagina, 1, totalPaginas);
            var hoy = DateTime.Today;

            return new ListadoPagosViewModel
            {
                Busqueda = busqueda,
                IdEstadoPago = idEstadoPago,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                PaginaActual = paginaSegura,
                TotalPaginas = totalPaginas,

                // Los indicadores miran toda la historia, no la pagina abierta
                // ni el filtro: si no, cambiarian al paginar. Salen del mismo
                // agregado que el tablero, para que las dos pantallas no digan
                // cifras distintas de lo mismo
                TotalPagos = TotalPedidosHistoricos + PedidosCanceladosHistoricos,
                TotalRecaudado = VentasDelAnio().Sum(venta => venta.Total),
                PagosAnulados = PedidosCanceladosHistoricos,

                // Estos dos si son una comparacion de fecha real, no un
                // agregado: con los pedidos de ejemplo no hay ninguno de hoy
                PagosHoy = todos.Count(pago => pago.FechaPago.Date == hoy),
                RecaudadoHoy = todos
                    .Where(pago => !pago.EstaAnulado && pago.FechaPago.Date == hoy)
                    .Sum(pago => pago.Monto),

                Pagos = lista
                    .Skip((paginaSegura - 1) * PagosPorPagina)
                    .Take(PagosPorPagina)
                    .ToList(),
            };
        }

        // Un pago por pedido. Cancelar el pedido anula su pago, y por eso el
        // estado del pago no se escribe: sale del estado del pedido
        private static PagoAdmin PagoDe(PedidoEjemplo pedido)
        {
            var anulado = pedido.IdEstado == 4;
            var cliente = ClienteDe(pedido);

            return new PagoAdmin
            {
                // El pago es otra entidad que el pedido: el numero no coincide
                IdPago = pedido.Id + 20,
                IdPedido = pedido.Id,
                NumeroFactura = pedido.NumeroFactura,
                NombreCliente = cliente.Nombre,
                CorreoCliente = cliente.Correo,
                NombreMetodoPago = MetodoDe(pedido),
                Monto = TotalDe(pedido),
                FechaPago = pedido.Fecha,
                Comprobante = pedido.Comprobante,
                IdEstadoPago = anulado ? 3 : 2,
                NombreEstado = anulado ? "Anulado" : "Pagado",
            };
        }

        // Los tres de TB_ESTADO_PAGO
        public static List<(int Id, string Nombre)> EstadosPago() =>
        [
            (1, "Pendiente"),
            (2, "Pagado"),
            (3, "Anulado"),
        ];

        // ----------------------------------------------------------------
        // Panel: facturas
        // ----------------------------------------------------------------

        public const int FacturasPorPagina = 5;

        public static ListadoFacturasViewModel Facturas(
            string? busqueda,
            string? fechaInicio,
            string? fechaFin,
            int pagina)
        {
            // Solo los pedidos con factura emitida. La factura es uno a uno con
            // el pedido por UQ_Factura_Pedido
            var encontrados = LibroPedidos
                .Where(pedido => pedido.NumeroFactura is not null)
                .Select(FacturaDe);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();
                encontrados = encontrados.Where(factura =>
                    factura.NumeroFactura.Contains(texto, StringComparison.OrdinalIgnoreCase)
                    || factura.NombreCliente.Contains(texto, StringComparison.OrdinalIgnoreCase)
                    || factura.CorreoCliente.Contains(texto, StringComparison.OrdinalIgnoreCase));
            }

            encontrados = EntreFechas(encontrados, factura => factura.FechaFactura, fechaInicio, fechaFin);

            var lista = encontrados.ToList();
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(lista.Count / (double)FacturasPorPagina));
            var paginaSegura = Math.Clamp(pagina, 1, totalPaginas);

            return new ListadoFacturasViewModel
            {
                Busqueda = busqueda,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                PaginaActual = paginaSegura,
                TotalPaginas = totalPaginas,
                Facturas = lista
                    .Skip((paginaSegura - 1) * FacturasPorPagina)
                    .Take(FacturasPorPagina)
                    .ToList(),
            };
        }

        public static DetalleFacturaViewModel? DetalleFactura(IWebHostEnvironment entorno, int idFactura)
        {
            var pedido = LibroPedidos.FirstOrDefault(fila =>
                fila.NumeroFactura is not null && fila.Id == idFactura);

            if (pedido is null)
            {
                return null;
            }

            var cliente = ClienteDe(pedido);
            var anulada = pedido.IdEstado == 4;

            return new DetalleFacturaViewModel
            {
                Encabezado = new EncabezadoFactura
                {
                    IdFactura = pedido.Id,
                    IdPedido = pedido.Id,
                    NumeroFactura = pedido.NumeroFactura!,
                    FechaFactura = pedido.Fecha,
                    Subtotal = SubtotalDe(pedido),
                    CostoEnvio = pedido.CostoEnvio,
                    Total = TotalDe(pedido),
                    IdEstadoFactura = anulada ? 2 : 1,
                    NombreEstado = anulada ? "Anulada" : "Emitida",
                    NombreCliente = cliente.Nombre,
                    CorreoCliente = cliente.Correo,
                    TelefonoCliente = cliente.Telefono,
                    CedulaCliente = cliente.Cedula,
                    Direccion = DireccionDe(cliente),
                    NombreMetodoPago = MetodoDe(pedido),
                    Comprobante = pedido.Comprobante,
                    FechaPago = anulada ? null : pedido.Fecha,
                },
                Lineas = LineasDe(entorno, pedido),
            };
        }

        // La factura nace con el pedido y se anula cuando el pedido se cancela
        private static FacturaAdmin FacturaDe(PedidoEjemplo pedido)
        {
            var anulada = pedido.IdEstado == 4;
            var cliente = ClienteDe(pedido);

            return new FacturaAdmin
            {
                IdFactura = pedido.Id,
                IdPedido = pedido.Id,
                NumeroFactura = pedido.NumeroFactura!,
                FechaFactura = pedido.Fecha,
                Total = TotalDe(pedido),
                IdEstadoFactura = anulada ? 2 : 1,
                NombreEstado = anulada ? "Anulada" : "Emitida",
                NombreCliente = cliente.Nombre,
                CorreoCliente = cliente.Correo,
                NombreMetodoPago = MetodoDe(pedido),
                Comprobante = pedido.Comprobante,
            };
        }

        // Los campos date mandan yyyy-MM-dd. El limite de arriba suma un dia
        // para que el ultimo dia entre completo, igual que los procedimientos
        private static IEnumerable<T> EntreFechas<T>(
            IEnumerable<T> origen,
            Func<T, DateTime> fecha,
            string? desde,
            string? hasta)
        {
            if (DateTime.TryParse(desde, out var inicio))
            {
                origen = origen.Where(fila => fecha(fila) >= inicio.Date);
            }

            if (DateTime.TryParse(hasta, out var fin))
            {
                origen = origen.Where(fila => fecha(fila) < fin.Date.AddDays(1));
            }

            return origen;
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
            VarianteProducto[] Variantes,
            // Los dos ultimos son del panel. Activo en false saca al producto del
            // catalogo publico pero lo deja visible en la tabla de productos
            bool Activo = true,
            bool Destacado = false);

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
                ], Destacado: true),

            // Dado de baja. Sale en la tabla del panel y no en el catalogo, y el
            // pedido FAC-000009 lo sigue conteniendo: una baja no borra historia
            new(2, "Collar Pato", 1, "Collares", 7000m, 5, false,
                "Collar corto con dije de pato en arcilla polimerica, hecho y pintado a mano.",
                ["producto-collar-pato.jpeg"], null,
                [
                    new VarianteProducto { IdVariante = 4, Color = "Amarillo", Stock = 5 },
                ],
                Activo: false),

            new(3, "Pulsera de Perlas", 2, "Pulseras", 5500m, 12, false,
                "Pulsera de perlas de rio con broche de acero quirurgico.",
                ["producto-pulsera-perlas.jpeg", "destacado-pulsera-perlas.jpeg"],
                "destacado-pulsera-perlas.jpeg",
                [
                    new VarianteProducto { IdVariante = 5, Talla = "Pequena", Stock = 6 },
                    new VarianteProducto { IdVariante = 6, Talla = "Mediana", Stock = 6 },
                ], Destacado: true),

            new(4, "Llavero Flor", 3, "Llaveros", 2500m, 0, false,
                "Llavero con flor tejida en crochet y argolla reforzada.",
                ["producto-llavero-flor.jpeg"], null, []),

            new(5, "Aretes Luna", 4, "Aretes", 3800m, 4, false,
                "Aretes de luna en acero dorado, livianos y antialergicos.",
                [], "destacado-aretes-luna.jpeg",
                [
                    new VarianteProducto { IdVariante = 7, Color = "Dorado", Stock = 4 },
                ], Destacado: true),

            // Stock 1 y no mas: CK_Producto_PiezaUnicaStock no admite otra cosa
            new(6, "Collar Trenzado", 1, "Collares", 9200m, 1, true,
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

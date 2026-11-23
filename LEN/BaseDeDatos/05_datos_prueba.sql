/* ============================================================
   LEN Accessories - 05 Datos de prueba
   Correr despues de 01_esquema.sql, 02_catalogos.sql y 03_procedimientos.sql

   SOLO PARA DESARROLLO. Estos dos usuarios existen para poder entrar al
   sistema en una base recien creada. No correr en una base de produccion
   y cambiar las contrasenas si alguna vez se publica.

   Usuario        Rol            Contrasena
   admin@len.test    Administrador  Admin2026Len
   cliente@len.test  Cliente        Cliente2026Len

   Los hashes los genero la aplicacion al registrar las dos cuentas desde
   la pantalla de registro (PasswordHasher, PBKDF2 con HMAC-SHA256), y el
   administrador es la primera cuenta subida de rol. La base no calcula
   hashes: por eso no se puede reescribir este archivo a mano con otra
   contrasena. Para cambiarla, registrar una cuenta nueva y copiar su hash.

   Se puede correr varias veces: si el correo ya existe no se toca nada,
   asi no se pisa una contrasena cambiada desde la aplicacion.

   BT-05. Tambien deja un catalogo de prueba: categorias, productos,
   imagenes y variantes. Igual que los usuarios, solo inserta lo que falta
   (por nombre), asi no pisa lo que se haya cambiado desde el panel.
   Las imagenes apuntan a wwwroot/imagenes/productos del proyecto Web, donde
   estan copiadas las fotos de ejemplo. Estos datos entran directo a las
   tablas y no dejan bitacora: no son acciones de ningun usuario.

   Que permite probar:
     - 4 categorias activas en el menu y 1 inactiva que no debe salir
     - 14 productos activos: con 12 por pagina, el catalogo tiene 2 paginas
     - 1 producto inactivo que no debe aparecer en ningun lado
     - 2 agotados (stock 0). Uno esta marcado destacado y aun asi no sale
       en la portada, porque los destacados piden stock
     - 1 pieza unica con stock 1
     - 2 productos con dos imagenes (galeria) y varios sin imagen (marcador)
     - variantes con color para el filtro: Dorado, Plateado, Azul, Rojo, Blanco

   BT-10. Tambien deja 12 pedidos de cliente@len.test, para probar el
   historial y el detalle mientras no exista el checkout (BT-09). Siguen las
   mismas reglas que SP_Pedido_Confirmar: factura FAC-000000 con el IdPedido,
   precio congelado en el detalle, y al cancelar pago Anulado y factura
   Anulada. Solo se crean si ese cliente todavia no tiene ningun pedido.
     - 12 pedidos: con 10 por pagina, el historial tiene 2 paginas
     - estados Comprada, En camino, Recibido y Cancelado (con motivo)
     - pagos Pagado, Pendiente (efectivo contra entrega) y Anulado
     - pedidos de varias lineas y cantidades: Articulos suma unidades
   No descuentan stock: son historia, no compras nuevas.

   BT-11. Para la bandeja de pedidos del panel:
     - 2 clientes mas, maria.prueba@len.test y jose.prueba@len.test, con la
       misma contrasena que cliente@len.test (Cliente2026Len)
     - 11 pedidos de ellos: con los 12 de arriba son 23, 3 paginas de 10.
       Hay de hoy, varios En preparacion (para avanzar a Entregado), dos
       Cancelados y uno a las 23:30, para probar que el filtro de fechas
       incluye el ultimo dia completo
     - historial en TB_BITACORA para todos los pedidos de prueba que no lo
       tengan: Confirmar compra y cada Cambiar estado, con el mismo JSON que
       escriben SP_Pedido_Confirmar y SP_Pedido_CambiarEstado
   ============================================================ */

USE BD_LEN;
GO

SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

BEGIN TRY
BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM dbo.TB_USUARIO WHERE Correo = N'admin@len.test')
    INSERT INTO dbo.TB_USUARIO (IdRol, Nombre, Apellido, Correo, ContrasenaHash)
    VALUES (1, N'Administrador', N'Prueba', N'admin@len.test',
            N'AQAAAAIAAYagAAAAEGkzeQ1ia/53cGBsHPMpjTrGDjFRr5/w1WNiXYs+mLQOwOPw5s/tr9tjmv5hZSvmNg==');

IF NOT EXISTS (SELECT 1 FROM dbo.TB_USUARIO WHERE Correo = N'cliente@len.test')
    INSERT INTO dbo.TB_USUARIO (IdRol, Nombre, Apellido, Correo, ContrasenaHash)
    VALUES (2, N'Cliente', N'Prueba', N'cliente@len.test',
            N'AQAAAAIAAYagAAAAEHncdIy6BRgG9dihrnnteXNW0jZ/IQ6X3TKeNFUjUZ2971wcrUVEnMFJF0exCQNeCw==');

COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

/* ============================================================
   BT-05. Catalogo de prueba
   ============================================================ */

BEGIN TRY
BEGIN TRANSACTION;

/* Categorias. La ultima esta inactiva y sin productos: es la que prueba
   que el menu publico no lista categorias dadas de baja (HU-PUB-007 CA-4) */
INSERT INTO dbo.TB_CATEGORIA (Nombre, Descripcion, Activo)
SELECT v.Nombre, v.Descripcion, v.Activo
FROM (VALUES
    (N'Collares',           N'Collares y gargantillas hechos a mano.',          CAST(1 AS BIT)),
    (N'Pulseras',           N'Pulseras de perlas, mostacilla e hilo.',          CAST(1 AS BIT)),
    (N'Aretes',             N'Aretes ligeros para todos los días.',             CAST(1 AS BIT)),
    (N'Llaveros y charms',  N'Llaveros y charms para bolso.',                   CAST(1 AS BIT)),
    (N'Temporada pasada',   N'Colección retirada. No debe verse en la tienda.', CAST(0 AS BIT))
) AS v (Nombre, Descripcion, Activo)
WHERE NOT EXISTS (SELECT 1 FROM dbo.TB_CATEGORIA AS C WHERE C.Nombre = v.Nombre);

/* Productos. Precio en colones */
INSERT INTO dbo.TB_PRODUCTO (IdCategoria, Nombre, Descripcion, Precio, Stock, EsPiezaUnica, Destacado, Activo)
SELECT C.IdCategoria, v.Nombre, v.Descripcion, v.Precio, v.Stock, v.EsPiezaUnica, v.Destacado, v.Activo
FROM (VALUES
    /*  Categoria             Nombre                         Descripcion                                                          Precio     Stock  Unica Destacado Activo */
    (N'Collares',          N'Collar Bello',               N'Collar artesanal de la colección LEN. Cierre ajustable.',          12500.00,  8,     0,    1,        1),
    (N'Collares',          N'Collar Pato',                N'Collar con dije de pato. Agotado y destacado: no sale en portada.', 9800.00,   0,     0,    1,        1),
    (N'Collares',          N'Collar de mostacilla',       N'Collar de mostacilla en varios colores.',                          7500.00,   6,     0,    0,        1),
    (N'Collares',          N'Gargantilla pieza única',    N'Gargantilla irrepetible. Solo hay una.',                           18000.00,  1,     1,    0,        1),
    (N'Collares',          N'Collar descontinuado',       N'Producto inactivo: no debe aparecer en la tienda.',                8000.00,   5,     0,    0,        0),
    (N'Pulseras',          N'Pulsera de perlas',          N'Pulsera de perlas con cierre de broche.',                          8900.00,   10,    0,    1,        1),
    (N'Pulseras',          N'Pulsera de mostacilla',      N'Pulsera elástica de mostacilla.',                                  5500.00,   12,    0,    0,        1),
    (N'Pulseras',          N'Pulsera tejida',             N'Pulsera tejida a mano con hilo encerado.',                         6000.00,   3,     0,    0,        1),
    (N'Pulseras',          N'Pulsera de hilo',            N'Pulsera sencilla de hilo. Agotada.',                               3500.00,   0,     0,    0,        1),
    (N'Aretes',            N'Aretes Luna',                N'Aretes con dije de luna.',                                         7800.00,   5,     0,    1,        1),
    (N'Aretes',            N'Aretes de perla',            N'Aretes pequeños de perla.',                                        6500.00,   7,     0,    0,        1),
    (N'Aretes',            N'Aretes de flor',             N'Aretes con forma de flor.',                                        5900.00,   4,     0,    0,        1),
    (N'Llaveros y charms', N'Llavero Flor',               N'Llavero con charm de flor.',                                       4500.00,   9,     0,    1,        1),
    (N'Llaveros y charms', N'Charm corazón para bolso',   N'Charm de corazón para colgar en el bolso.',                        4000.00,   15,    0,    0,        1),
    (N'Llaveros y charms', N'Charm estrella',             N'Charm de estrella para bolso o llavero.',                          3800.00,   6,     0,    0,        1)
) AS v (Categoria, Nombre, Descripcion, Precio, Stock, EsPiezaUnica, Destacado, Activo)
INNER JOIN dbo.TB_CATEGORIA AS C
    ON C.Nombre = v.Categoria
WHERE NOT EXISTS (SELECT 1 FROM dbo.TB_PRODUCTO AS P WHERE P.Nombre = v.Nombre);

/* Imagenes. Rutas relativas a wwwroot/imagenes del Web. Collar Bello y
   Pulsera de perlas llevan dos, para probar la galeria del detalle.
   La principal solo se marca si el producto todavia no tiene una activa:
   el indice UX_ImagenProducto_PrincipalProducto no admite dos */
INSERT INTO dbo.TB_IMAGEN_PRODUCTO (IdProducto, RutaImagen, TextoAlternativo, EsPrincipal)
SELECT
    P.IdProducto,
    v.RutaImagen,
    v.TextoAlternativo,
    CASE WHEN v.EsPrincipal = 1
              AND NOT EXISTS (SELECT 1 FROM dbo.TB_IMAGEN_PRODUCTO AS I
                              WHERE I.IdProducto = P.IdProducto AND I.EsPrincipal = 1 AND I.Activo = 1)
         THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END
FROM (VALUES
    (N'Collar Bello',      N'productos/producto-collar-bello.jpeg',    N'Collar Bello, vista principal',  1),
    (N'Collar Bello',      N'productos/destacado-collar-bello.jpeg',   N'Collar Bello, otra vista',       0),
    (N'Collar Pato',       N'productos/producto-collar-pato.jpeg',     N'Collar con dije de pato',        1),
    (N'Pulsera de perlas', N'productos/producto-pulsera-perlas.jpeg',  N'Pulsera de perlas, vista principal', 1),
    (N'Pulsera de perlas', N'productos/destacado-pulsera-perlas.jpeg', NULL,                              0),
    (N'Aretes Luna',       N'productos/destacado-aretes-luna.jpeg',    N'Aretes con dije de luna',        1),
    (N'Llavero Flor',      N'productos/producto-llavero-flor.jpeg',    N'Llavero con charm de flor',      1)
) AS v (Producto, RutaImagen, TextoAlternativo, EsPrincipal)
INNER JOIN dbo.TB_PRODUCTO AS P
    ON P.Nombre = v.Producto
WHERE NOT EXISTS (SELECT 1 FROM dbo.TB_IMAGEN_PRODUCTO AS I
                  WHERE I.IdProducto = P.IdProducto AND I.RutaImagen = v.RutaImagen);

/* Variantes con color, para el filtro del catalogo (HU-PUB-003 CA-3).
   Ejemplo: filtrar por Azul devuelve Collar de mostacilla y Pulsera de mostacilla */
INSERT INTO dbo.TB_VARIANTE_PRODUCTO (IdProducto, Color, Stock)
SELECT P.IdProducto, v.Color, v.Stock
FROM (VALUES
    (N'Collar Bello',          N'Dorado',   4),
    (N'Collar Bello',          N'Plateado', 4),
    (N'Collar de mostacilla',  N'Azul',     3),
    (N'Collar de mostacilla',  N'Rojo',     3),
    (N'Pulsera de mostacilla', N'Azul',     6),
    (N'Pulsera de mostacilla', N'Rojo',     6),
    (N'Pulsera de perlas',     N'Blanco',   10),
    (N'Aretes Luna',           N'Dorado',   5)
) AS v (Producto, Color, Stock)
INNER JOIN dbo.TB_PRODUCTO AS P
    ON P.Nombre = v.Producto
WHERE NOT EXISTS (SELECT 1 FROM dbo.TB_VARIANTE_PRODUCTO AS V
                  WHERE V.IdProducto = P.IdProducto
                    AND V.Color = v.Color AND V.Talla IS NULL AND V.Modelo IS NULL);

COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

/* ============================================================
   BT-10. Pedidos de prueba de cliente@len.test
   ============================================================ */

BEGIN TRY
BEGIN TRANSACTION;

DECLARE @IdCliente INT = (SELECT IdUsuario FROM dbo.TB_USUARIO WHERE Correo = N'cliente@len.test');

IF @IdCliente IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.TB_PEDIDO WHERE IdUsuario = @IdCliente)
BEGIN
    /* Direccion de envio: la principal del cliente si ya tiene una. Si no,
       se crea una en Carmen, San Jose, y queda principal */
    DECLARE @IdDireccion INT =
    (
        SELECT TOP (1) IdDireccion
        FROM dbo.TB_DIRECCION_ENVIO
        WHERE IdUsuario = @IdCliente AND Activo = 1
        ORDER BY EsPrincipal DESC, IdDireccion
    );

    IF @IdDireccion IS NULL
    BEGIN
        INSERT INTO dbo.TB_DIRECCION_ENVIO
            (IdUsuario, IdDistrito, NombreDestinatario, DireccionExacta, Referencia, TelefonoContacto, EsPrincipal)
        SELECT TOP (1) @IdCliente, D.IdDistrito, N'Cliente Prueba', N'Avenida 2, calle 5, casa 12',
               N'Frente al parque', N'8888-0000', 1
        FROM dbo.TB_DISTRITO AS D
        INNER JOIN dbo.TB_CANTON    AS C ON C.IdCanton    = D.IdCanton
        INNER JOIN dbo.TB_PROVINCIA AS P ON P.IdProvincia = C.IdProvincia
        WHERE P.Nombre = N'San José' AND C.Nombre = N'San José' AND D.Nombre = N'Carmen';

        SET @IdDireccion = CAST(SCOPE_IDENTITY() AS INT);
    END;

    /* Estados: 1 Pagado (Comprada), 2 En preparacion (En camino),
                3 Entregado (Recibido), 4 Cancelado
       Pago:    1 Pendiente, 2 Pagado, 3 Anulado
       Metodo:  1 Tarjeta, 2 Transferencia, 3 SINPE Movil, 4 Efectivo */
    DECLARE @Pedidos TABLE
    (
        Orden          INT PRIMARY KEY,
        DiasAtras      INT,
        IdEstadoPedido INT,
        IdMetodoPago   INT,
        IdEstadoPago   INT,
        Comprobante    NVARCHAR(255) NULL,
        Motivo         NVARCHAR(255) NULL
    );

    INSERT INTO @Pedidos VALUES
        (1,   2, 1, 1, 2, N'4242',       NULL),
        (2,   5, 2, 3, 2, N'8888-1234',  NULL),
        (3,   9, 3, 2, 2, N'REF-102938', NULL),
        (4,  12, 4, 1, 3, N'1111',       N'Cancelado a solicitud del cliente antes del envío.'),
        (5,  15, 1, 4, 1, NULL,          NULL),
        (6,  20, 3, 1, 2, N'5555',       NULL),
        (7,  26, 3, 3, 2, N'8888-1234',  NULL),
        (8,  33, 3, 1, 2, N'4242',       NULL),
        (9,  41, 3, 2, 2, N'REF-554433', NULL),
        (10, 50, 3, 3, 2, N'8888-1234',  NULL),
        (11, 62, 3, 1, 2, N'4242',       NULL),
        (12, 75, 3, 4, 2, NULL,          NULL);

    DECLARE @Lineas TABLE (Orden INT, Producto NVARCHAR(150), Cantidad INT);

    INSERT INTO @Lineas VALUES
        (1,  N'Collar Bello', 1), (1, N'Aretes Luna', 2),
        (2,  N'Pulsera de perlas', 3),
        (3,  N'Llavero Flor', 2), (3, N'Charm estrella', 1), (3, N'Charm corazón para bolso', 1),
        (4,  N'Collar de mostacilla', 1),
        (5,  N'Aretes de perla', 1),
        (6,  N'Pulsera de mostacilla', 2),
        (7,  N'Aretes de flor', 1), (7, N'Pulsera tejida', 1),
        (8,  N'Collar Bello', 2),
        (9,  N'Charm corazón para bolso', 3),
        (10, N'Gargantilla pieza única', 1),
        (11, N'Aretes Luna', 1), (11, N'Llavero Flor', 1),
        (12, N'Pulsera de hilo', 2);

    DECLARE @CostoEnvio DECIMAL(10,2) = 2500.00;
    DECLARE @Orden INT = 1;
    DECLARE @Maximo INT = (SELECT MAX(Orden) FROM @Pedidos);
    DECLARE @Subtotal DECIMAL(10,2), @IdPedido INT, @Fecha DATETIME2(0);

    WHILE @Orden <= @Maximo
    BEGIN
        /* El precio que se congela es el actual del producto */
        SET @Subtotal =
        (
            SELECT SUM(PR.Precio * L.Cantidad)
            FROM @Lineas AS L
            INNER JOIN dbo.TB_PRODUCTO AS PR ON PR.Nombre = L.Producto
            WHERE L.Orden = @Orden
        );

        /* Sin el catalogo de prueba no hay productos: el pedido se salta */
        IF @Subtotal IS NOT NULL
        BEGIN
            SET @Fecha = DATEADD(DAY, -(SELECT DiasAtras FROM @Pedidos WHERE Orden = @Orden), SYSDATETIME());

            INSERT INTO dbo.TB_PEDIDO
                (IdUsuario, IdDireccion, IdEstadoPedido, FechaPedido, Subtotal, CostoEnvio, Total, MotivoCancelacion)
            SELECT @IdCliente, @IdDireccion, X.IdEstadoPedido, @Fecha,
                   @Subtotal, @CostoEnvio, @Subtotal + @CostoEnvio, X.Motivo
            FROM @Pedidos AS X
            WHERE X.Orden = @Orden;

            SET @IdPedido = CAST(SCOPE_IDENTITY() AS INT);

            INSERT INTO dbo.TB_DETALLE_PEDIDO (IdPedido, IdProducto, Cantidad, PrecioUnitario, SubtotalLinea)
            SELECT @IdPedido, PR.IdProducto, L.Cantidad, PR.Precio, PR.Precio * L.Cantidad
            FROM @Lineas AS L
            INNER JOIN dbo.TB_PRODUCTO AS PR ON PR.Nombre = L.Producto
            WHERE L.Orden = @Orden;

            /* 1 Emitida, 2 Anulada: la de un pedido cancelado queda anulada */
            INSERT INTO dbo.TB_FACTURA
                (IdPedido, IdEstadoFactura, NumeroFactura, FechaFactura, Subtotal, CostoEnvio, Total)
            SELECT @IdPedido,
                   CASE WHEN X.IdEstadoPedido = 4 THEN 2 ELSE 1 END,
                   N'FAC-' + RIGHT(N'000000' + CAST(@IdPedido AS NVARCHAR(10)), 6),
                   @Fecha, @Subtotal, @CostoEnvio, @Subtotal + @CostoEnvio
            FROM @Pedidos AS X
            WHERE X.Orden = @Orden;

            INSERT INTO dbo.TB_PAGO (IdPedido, IdMetodoPago, IdEstadoPago, Monto, FechaPago, Comprobante)
            SELECT @IdPedido, X.IdMetodoPago, X.IdEstadoPago, @Subtotal + @CostoEnvio, @Fecha, X.Comprobante
            FROM @Pedidos AS X
            WHERE X.Orden = @Orden;
        END;

        SET @Orden += 1;
    END;
END;

COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

/* ============================================================
   BT-11. Mas clientes, pedidos e historial para el panel
   ============================================================ */

BEGIN TRY
BEGIN TRANSACTION;

/* Dos clientes mas. El hash es el de cliente@len.test: PasswordHasher guarda
   la sal dentro del hash, asi que sirve igual en otra cuenta y la contrasena
   es la misma, Cliente2026Len */
INSERT INTO dbo.TB_USUARIO (IdRol, Nombre, Apellido, Telefono, Correo, ContrasenaHash)
SELECT 2, v.Nombre, v.Apellido, v.Telefono, v.Correo,
       N'AQAAAAIAAYagAAAAEHncdIy6BRgG9dihrnnteXNW0jZ/IQ6X3TKeNFUjUZ2971wcrUVEnMFJF0exCQNeCw=='
FROM (VALUES
    (N'María', N'Rodríguez', N'8701-2233', N'maria.prueba@len.test'),
    (N'José',  N'Mora',      N'8602-4455', N'jose.prueba@len.test')
) AS v (Nombre, Apellido, Telefono, Correo)
WHERE NOT EXISTS (SELECT 1 FROM dbo.TB_USUARIO AS U WHERE U.Correo = v.Correo);

/* Una direccion principal para cada uno, si no tiene ninguna activa */
INSERT INTO dbo.TB_DIRECCION_ENVIO
    (IdUsuario, IdDistrito, NombreDestinatario, DireccionExacta, Referencia, TelefonoContacto, EsPrincipal)
SELECT U.IdUsuario, D.IdDistrito, U.Nombre + N' ' + U.Apellido, v.DireccionExacta, v.Referencia,
       ISNULL(U.Telefono, N'8888-0000'), 1
FROM (VALUES
    (N'maria.prueba@len.test', N'Heredia',  N'Heredia',  N'Mercedes', N'Del templo 200 m norte, casa azul', N'Portón negro'),
    (N'jose.prueba@len.test',  N'Alajuela', N'Alajuela', N'Alajuela', N'Barrio El Carmen, casa 8',          NULL)
) AS v (Correo, Provincia, Canton, Distrito, DireccionExacta, Referencia)
INNER JOIN dbo.TB_USUARIO   AS U ON U.Correo = v.Correo
INNER JOIN dbo.TB_PROVINCIA AS P ON P.Nombre = v.Provincia
INNER JOIN dbo.TB_CANTON    AS C ON C.IdProvincia = P.IdProvincia AND C.Nombre = v.Canton
INNER JOIN dbo.TB_DISTRITO  AS D ON D.IdCanton = C.IdCanton AND D.Nombre = v.Distrito
WHERE NOT EXISTS (SELECT 1 FROM dbo.TB_DIRECCION_ENVIO AS DE
                  WHERE DE.IdUsuario = U.IdUsuario AND DE.Activo = 1);

/* Pedidos: solo para los clientes que todavia no tienen ninguno.
   Estados: 1 Pagado, 2 En preparacion, 3 Entregado, 4 Cancelado
   Pago:    1 Pendiente, 2 Pagado, 3 Anulado
   Metodo:  1 Tarjeta, 2 Transferencia, 3 SINPE Movil, 4 Efectivo */
DECLARE @Pedidos TABLE
(
    Orden          INT PRIMARY KEY,
    Correo         NVARCHAR(150),
    DiasAtras      INT,
    Hora           TIME(0),
    IdEstadoPedido INT,
    IdMetodoPago   INT,
    IdEstadoPago   INT,
    Comprobante    NVARCHAR(255) NULL,
    Motivo         NVARCHAR(255) NULL
);

INSERT INTO @Pedidos VALUES
    (1,  N'maria.prueba@len.test',  0, '09:15', 1, 1, 2, N'4242',       NULL),
    (2,  N'maria.prueba@len.test',  1, '14:40', 2, 3, 2, N'8701-2233',  NULL),
    (3,  N'maria.prueba@len.test',  3, '23:30', 1, 2, 2, N'REF-771100', NULL),
    (4,  N'maria.prueba@len.test',  8, '10:05', 4, 1, 3, N'9090',       N'El cliente no respondió para coordinar la entrega.'),
    (5,  N'maria.prueba@len.test', 18, '16:20', 3, 3, 2, N'8701-2233',  NULL),
    (6,  N'maria.prueba@len.test', 35, '11:00', 3, 4, 2, NULL,          NULL),
    (7,  N'jose.prueba@len.test',   0, '07:45', 1, 4, 1, NULL,          NULL),
    (8,  N'jose.prueba@len.test',   2, '19:10', 2, 1, 2, N'3131',       NULL),
    (9,  N'jose.prueba@len.test',   6, '12:30', 3, 2, 2, N'REF-440022', NULL),
    (10, N'jose.prueba@len.test',  14, '08:50', 4, 3, 3, N'8602-4455',  N'Un producto se agotó al preparar el pedido.'),
    (11, N'jose.prueba@len.test',  28, '15:15', 3, 1, 2, N'3131',       NULL);

DECLARE @Lineas TABLE (Orden INT, Producto NVARCHAR(150), Cantidad INT);

INSERT INTO @Lineas VALUES
    (1,  N'Pulsera de perlas', 1), (1, N'Aretes de perla', 1),
    (2,  N'Collar Bello', 1),
    (3,  N'Charm estrella', 2), (3, N'Llavero Flor', 1),
    (4,  N'Aretes Luna', 1),
    (5,  N'Pulsera de mostacilla', 3),
    (6,  N'Collar de mostacilla', 1), (6, N'Aretes de flor', 1),
    (7,  N'Charm corazón para bolso', 2),
    (8,  N'Pulsera tejida', 1), (8, N'Aretes Luna', 1),
    (9,  N'Collar Bello', 1),
    (10, N'Pulsera de perlas', 2),
    (11, N'Llavero Flor', 3);

DECLARE @Elegibles TABLE (Correo NVARCHAR(150) PRIMARY KEY, IdUsuario INT, IdDireccion INT);

INSERT INTO @Elegibles (Correo, IdUsuario, IdDireccion)
SELECT U.Correo, U.IdUsuario,
       (SELECT TOP (1) DE.IdDireccion FROM dbo.TB_DIRECCION_ENVIO AS DE
        WHERE DE.IdUsuario = U.IdUsuario AND DE.Activo = 1
        ORDER BY DE.EsPrincipal DESC, DE.IdDireccion)
FROM dbo.TB_USUARIO AS U
WHERE U.Correo IN (N'maria.prueba@len.test', N'jose.prueba@len.test')
  AND NOT EXISTS (SELECT 1 FROM dbo.TB_PEDIDO AS PE WHERE PE.IdUsuario = U.IdUsuario);

DECLARE @CostoEnvio DECIMAL(10,2) = 2500.00;
DECLARE @Orden INT = 1;
DECLARE @Maximo INT = (SELECT MAX(Orden) FROM @Pedidos);
DECLARE @Subtotal DECIMAL(10,2), @IdPedido INT, @Fecha DATETIME2(0), @IdUsuario INT, @IdDireccion INT;

WHILE @Orden <= @Maximo
BEGIN
    SELECT @IdUsuario = E.IdUsuario, @IdDireccion = E.IdDireccion
    FROM @Pedidos AS X
    INNER JOIN @Elegibles AS E ON E.Correo = X.Correo
    WHERE X.Orden = @Orden;

    SET @Subtotal =
    (
        SELECT SUM(PR.Precio * L.Cantidad)
        FROM @Lineas AS L
        INNER JOIN dbo.TB_PRODUCTO AS PR ON PR.Nombre = L.Producto
        WHERE L.Orden = @Orden
    );

    /* Sin cliente elegible, sin direccion o sin catalogo, el pedido se salta */
    IF @IdUsuario IS NOT NULL AND @IdDireccion IS NOT NULL AND @Subtotal IS NOT NULL
    BEGIN
        /* El dia de hoy menos DiasAtras, a la hora indicada */
        SELECT @Fecha = DATEADD(SECOND, DATEDIFF(SECOND, CAST('00:00' AS TIME(0)), X.Hora),
                                CAST(DATEADD(DAY, -X.DiasAtras, CAST(SYSDATETIME() AS DATE)) AS DATETIME2(0)))
        FROM @Pedidos AS X
        WHERE X.Orden = @Orden;

        INSERT INTO dbo.TB_PEDIDO
            (IdUsuario, IdDireccion, IdEstadoPedido, FechaPedido, Subtotal, CostoEnvio, Total, MotivoCancelacion)
        SELECT @IdUsuario, @IdDireccion, X.IdEstadoPedido, @Fecha,
               @Subtotal, @CostoEnvio, @Subtotal + @CostoEnvio, X.Motivo
        FROM @Pedidos AS X
        WHERE X.Orden = @Orden;

        SET @IdPedido = CAST(SCOPE_IDENTITY() AS INT);

        INSERT INTO dbo.TB_DETALLE_PEDIDO (IdPedido, IdProducto, Cantidad, PrecioUnitario, SubtotalLinea)
        SELECT @IdPedido, PR.IdProducto, L.Cantidad, PR.Precio, PR.Precio * L.Cantidad
        FROM @Lineas AS L
        INNER JOIN dbo.TB_PRODUCTO AS PR ON PR.Nombre = L.Producto
        WHERE L.Orden = @Orden;

        INSERT INTO dbo.TB_FACTURA
            (IdPedido, IdEstadoFactura, NumeroFactura, FechaFactura, Subtotal, CostoEnvio, Total)
        SELECT @IdPedido,
               CASE WHEN X.IdEstadoPedido = 4 THEN 2 ELSE 1 END,
               N'FAC-' + RIGHT(N'000000' + CAST(@IdPedido AS NVARCHAR(10)), 6),
               @Fecha, @Subtotal, @CostoEnvio, @Subtotal + @CostoEnvio
        FROM @Pedidos AS X
        WHERE X.Orden = @Orden;

        INSERT INTO dbo.TB_PAGO (IdPedido, IdMetodoPago, IdEstadoPago, Monto, FechaPago, Comprobante)
        SELECT @IdPedido, X.IdMetodoPago, X.IdEstadoPago, @Subtotal + @CostoEnvio, @Fecha, X.Comprobante
        FROM @Pedidos AS X
        WHERE X.Orden = @Orden;
    END;

    SELECT @IdUsuario = NULL, @IdDireccion = NULL;
    SET @Orden += 1;
END;

/* Historial (HU-ADM-PED-005) de los pedidos de prueba que no tienen ninguna
   fila en TB_BITACORA. Confirmar compra la hace el cliente; los cambios de
   estado, el administrador de prueba. El JSON es el que escriben los
   procedimientos, y el nombre del estado sale de TB_ESTADO_PEDIDO */
DECLARE @IdAdmin INT = (SELECT IdUsuario FROM dbo.TB_USUARIO WHERE Correo = N'admin@len.test');

DECLARE @SinHistorial TABLE
(
    IdPedido       INT PRIMARY KEY,
    IdUsuario      INT,
    IdEstadoPedido INT,
    FechaPedido    DATETIME2(0),
    Subtotal       DECIMAL(10,2),
    CostoEnvio     DECIMAL(10,2),
    Total          DECIMAL(10,2),
    Motivo         NVARCHAR(255) NULL,
    NumeroFactura  NVARCHAR(50) NULL,
    IdMetodoPago   INT NULL
);

INSERT INTO @SinHistorial
SELECT PE.IdPedido, PE.IdUsuario, PE.IdEstadoPedido, PE.FechaPedido, PE.Subtotal, PE.CostoEnvio,
       PE.Total, PE.MotivoCancelacion, F.NumeroFactura, PG.IdMetodoPago
FROM dbo.TB_PEDIDO AS PE
INNER JOIN dbo.TB_USUARIO AS U  ON U.IdUsuario = PE.IdUsuario
LEFT  JOIN dbo.TB_FACTURA AS F  ON F.IdPedido  = PE.IdPedido
LEFT  JOIN dbo.TB_PAGO    AS PG ON PG.IdPedido = PE.IdPedido
WHERE U.Correo LIKE N'%@len.test'
  AND NOT EXISTS (SELECT 1 FROM dbo.TB_BITACORA AS B
                  WHERE B.Entidad = N'TB_PEDIDO' AND B.IdRegistro = PE.IdPedido);

/* Confirmar compra, a la hora del pedido */
INSERT INTO dbo.TB_BITACORA (IdUsuario, Entidad, IdRegistro, Accion, ValorNuevo, FechaHora)
SELECT S.IdUsuario, N'TB_PEDIDO', S.IdPedido, N'Confirmar compra',
       (SELECT S.Subtotal AS Subtotal, S.CostoEnvio AS CostoEnvio, S.Total AS Total,
               S.IdMetodoPago AS IdMetodoPago, S.NumeroFactura AS NumeroFactura
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
       S.FechaPedido
FROM @SinHistorial AS S;

/* Pagado -> En preparacion: los que hoy estan En preparacion o Entregado */
INSERT INTO dbo.TB_BITACORA (IdUsuario, Entidad, IdRegistro, Accion, ValorAnterior, ValorNuevo, FechaHora)
SELECT @IdAdmin, N'TB_PEDIDO', S.IdPedido, N'Cambiar estado',
       (SELECT E.IdEstadoPedido, E.Nombre AS NombreEstado FROM dbo.TB_ESTADO_PEDIDO AS E
        WHERE E.IdEstadoPedido = 1 FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
       (SELECT E.IdEstadoPedido, E.Nombre AS NombreEstado FROM dbo.TB_ESTADO_PEDIDO AS E
        WHERE E.IdEstadoPedido = 2 FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
       DATEADD(HOUR, 4, S.FechaPedido)
FROM @SinHistorial AS S
WHERE S.IdEstadoPedido IN (2, 3);

/* En preparacion -> Entregado: los Entregados, dos dias despues */
INSERT INTO dbo.TB_BITACORA (IdUsuario, Entidad, IdRegistro, Accion, ValorAnterior, ValorNuevo, FechaHora)
SELECT @IdAdmin, N'TB_PEDIDO', S.IdPedido, N'Cambiar estado',
       (SELECT E.IdEstadoPedido, E.Nombre AS NombreEstado FROM dbo.TB_ESTADO_PEDIDO AS E
        WHERE E.IdEstadoPedido = 2 FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
       (SELECT E.IdEstadoPedido, E.Nombre AS NombreEstado FROM dbo.TB_ESTADO_PEDIDO AS E
        WHERE E.IdEstadoPedido = 3 FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
       DATEADD(DAY, 2, S.FechaPedido)
FROM @SinHistorial AS S
WHERE S.IdEstadoPedido = 3;

/* Pagado -> Cancelado, con el motivo guardado en el pedido */
INSERT INTO dbo.TB_BITACORA (IdUsuario, Entidad, IdRegistro, Accion, ValorAnterior, ValorNuevo, FechaHora)
SELECT @IdAdmin, N'TB_PEDIDO', S.IdPedido, N'Cambiar estado',
       (SELECT E.IdEstadoPedido, E.Nombre AS NombreEstado FROM dbo.TB_ESTADO_PEDIDO AS E
        WHERE E.IdEstadoPedido = 1 FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
       (SELECT E.IdEstadoPedido, E.Nombre AS NombreEstado, S.Motivo AS MotivoCancelacion
        FROM dbo.TB_ESTADO_PEDIDO AS E
        WHERE E.IdEstadoPedido = 4 FOR JSON PATH, WITHOUT_ARRAY_WRAPPER),
       DATEADD(HOUR, 3, S.FechaPedido)
FROM @SinHistorial AS S
WHERE S.IdEstadoPedido = 4;

COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

DECLARE @Resumen NVARCHAR(600) =
    N'05_datos_prueba.sql: listo. Administradores de prueba=' + CAST((SELECT COUNT(*) FROM dbo.TB_USUARIO WHERE Correo = N'admin@len.test' AND IdRol = 1) AS NVARCHAR(10))
  + N' Clientes de prueba=' + CAST((SELECT COUNT(*) FROM dbo.TB_USUARIO WHERE Correo = N'cliente@len.test' AND IdRol = 2) AS NVARCHAR(10))
  + N' Categorias activas=' + CAST((SELECT COUNT(*) FROM dbo.TB_CATEGORIA WHERE Activo = 1) AS NVARCHAR(10))
  + N' Productos activos=' + CAST((SELECT COUNT(*) FROM dbo.TB_PRODUCTO WHERE Activo = 1) AS NVARCHAR(10))
  + N' Imagenes activas=' + CAST((SELECT COUNT(*) FROM dbo.TB_IMAGEN_PRODUCTO WHERE Activo = 1) AS NVARCHAR(10))
  + N' Pedidos del cliente=' + CAST((SELECT COUNT(*) FROM dbo.TB_PEDIDO AS PE
                                     INNER JOIN dbo.TB_USUARIO AS U ON U.IdUsuario = PE.IdUsuario
                                     WHERE U.Correo = N'cliente@len.test') AS NVARCHAR(10))
  + N' Clientes extra=' + CAST((SELECT COUNT(*) FROM dbo.TB_USUARIO
                                WHERE Correo IN (N'maria.prueba@len.test', N'jose.prueba@len.test')) AS NVARCHAR(10))
  + N' Pedidos de prueba=' + CAST((SELECT COUNT(*) FROM dbo.TB_PEDIDO AS PE
                                   INNER JOIN dbo.TB_USUARIO AS U ON U.IdUsuario = PE.IdUsuario
                                   WHERE U.Correo LIKE N'%@len.test') AS NVARCHAR(10))
  + N' Bitacora de pedidos=' + CAST((SELECT COUNT(*) FROM dbo.TB_BITACORA WHERE Entidad = N'TB_PEDIDO') AS NVARCHAR(10));

PRINT @Resumen;
GO

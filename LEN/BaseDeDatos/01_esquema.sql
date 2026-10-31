/* ============================================================
   LEN Accessories - 01 Esquema
   Microsoft SQL Server 2019 o superior

   Crea la base BD_LEN y todas sus tablas. Es el primer script que
   se corre y no depende de ningun otro.

   Se puede correr las veces que haga falta: solo crea lo que
   falta. Todo el bloque de tablas va dentro de una transaccion,
   asi que si algo falla no queda la base a medias.

   Si la base ya existe con el esquema viejo del primer avance
   (tablas dbo.TB_usuario con columna id_usuario), el script se
   detiene con un mensaje y no toca nada. Ver LEEME.txt.
   ============================================================ */

SET NOCOUNT ON;
GO

IF DB_ID(N'BD_LEN') IS NULL
BEGIN
    /* La intercalacion se fija aqui y no se cambia despues: cambiarla
       obliga a recrear la base. CI_AI es insensible a mayusculas y a
       acentos, asi que buscar "jose" encuentra "José". */
    EXEC (N'CREATE DATABASE BD_LEN COLLATE Latin1_General_100_CI_AI_SC_UTF8;');
END;
GO

USE BD_LEN;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

/* ------------------------------------------------------------
   Guarda contra el esquema viejo.

   SQL Server no distingue mayusculas en los nombres de objeto, asi
   que dbo.TB_usuario y dbo.TB_USUARIO son la misma tabla. Si se
   corriera este script sobre la base del primer avance, las guardas
   IF OBJECT_ID darian por creadas tablas que en realidad tienen el
   esquema viejo, y quedaria una base mezclada e inservible.

   Va en su propio lote a proposito. El lote de tablas se compila
   completo antes de ejecutarse, asi que sobre la base vieja reventaria
   con un "Invalid column name" incomprensible antes de llegar a este
   THROW. Separado, el mensaje claro sale primero.
   ------------------------------------------------------------ */
IF COL_LENGTH('dbo.TB_USUARIO', 'id_usuario') IS NOT NULL
BEGIN
    THROW 50001, N'La base BD_LEN ya existe con el esquema viejo (snake_case). Renombrela o eliminela a mano antes de correr este script. Ver LEEME.txt.', 1;
END;
GO

BEGIN TRY
BEGIN TRANSACTION;

/* ============================================================
   1. Catalogos
   ============================================================ */

IF OBJECT_ID('dbo.TB_ROL', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_ROL
    (
        IdRol       INT           IDENTITY(1,1) NOT NULL,
        Nombre      NVARCHAR(50)  NOT NULL,
        Descripcion NVARCHAR(255) NULL,
        Activo      BIT           NOT NULL CONSTRAINT DF_Rol_Activo DEFAULT 1,

        CONSTRAINT PK_Rol PRIMARY KEY (IdRol),
        CONSTRAINT UQ_Rol_Nombre UNIQUE (Nombre),
        CONSTRAINT CK_Rol_NombreNoVacio CHECK (LTRIM(RTRIM(Nombre)) <> N'')
    );
END;

/* Estados del pedido. Ids fijos, los procedimientos los usan como
   constantes: 1 Pagado, 2 En preparacion, 3 Entregado, 4 Cancelado.
   Orden es la posicion en el flujo, no el Id. */
IF OBJECT_ID('dbo.TB_ESTADO_PEDIDO', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_ESTADO_PEDIDO
    (
        IdEstadoPedido INT          NOT NULL,
        Nombre         NVARCHAR(50) NOT NULL,
        Orden          INT          NOT NULL,
        ClaseCss       NVARCHAR(50) NULL,

        CONSTRAINT PK_EstadoPedido PRIMARY KEY (IdEstadoPedido),
        CONSTRAINT UQ_EstadoPedido_Nombre UNIQUE (Nombre),
        CONSTRAINT CK_EstadoPedido_Orden CHECK (Orden > 0)
    );
END;

/* 1 Pendiente, 2 Pagado, 3 Anulado. Hoy el checkout solo crea pagos
   en Pagado, porque el pago es simulado y se confirma en el mismo
   acto de compra. Los otros dos quedan para cuando exista pasarela. */
IF OBJECT_ID('dbo.TB_ESTADO_PAGO', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_ESTADO_PAGO
    (
        IdEstadoPago INT          NOT NULL,
        Nombre       NVARCHAR(50) NOT NULL,

        CONSTRAINT PK_EstadoPago PRIMARY KEY (IdEstadoPago),
        CONSTRAINT UQ_EstadoPago_Nombre UNIQUE (Nombre)
    );
END;

/* 1 Emitida, 2 Anulada */
IF OBJECT_ID('dbo.TB_ESTADO_FACTURA', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_ESTADO_FACTURA
    (
        IdEstadoFactura INT          NOT NULL,
        Nombre          NVARCHAR(50) NOT NULL,

        CONSTRAINT PK_EstadoFactura PRIMARY KEY (IdEstadoFactura),
        CONSTRAINT UQ_EstadoFactura_Nombre UNIQUE (Nombre)
    );
END;

IF OBJECT_ID('dbo.TB_METODO_PAGO', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_METODO_PAGO
    (
        IdMetodoPago INT           IDENTITY(1,1) NOT NULL,
        Nombre       NVARCHAR(100) NOT NULL,
        Descripcion  NVARCHAR(255) NULL,
        /* Que se le pide al cliente en el checkout como comprobante
           simulado: los ultimos digitos de la tarjeta, la referencia
           de la transferencia, el telefono del SINPE. NULL cuando no
           se le pide nada, como en efectivo contra entrega. */
        EtiquetaDatoPago NVARCHAR(100) NULL,
        Activo           BIT          NOT NULL CONSTRAINT DF_MetodoPago_Activo DEFAULT 1,

        CONSTRAINT PK_MetodoPago PRIMARY KEY (IdMetodoPago),
        CONSTRAINT UQ_MetodoPago_Nombre UNIQUE (Nombre)
    );
END;

/* ============================================================
   2. Usuarios
   ============================================================ */

IF OBJECT_ID('dbo.TB_USUARIO', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_USUARIO
    (
        IdUsuario      INT           IDENTITY(1,1) NOT NULL,
        IdRol          INT           NOT NULL,
        Nombre         NVARCHAR(100) NOT NULL,
        Apellido       NVARCHAR(100) NOT NULL,
        Cedula         NVARCHAR(20)  NULL,
        Telefono       NVARCHAR(20)  NULL,
        Correo         NVARCHAR(150) NOT NULL,
        /* Hash calculado siempre en la aplicacion. Aqui solo se guarda. */
        ContrasenaHash NVARCHAR(255) NOT NULL,
        TieneContrasenaTemporal   BIT           NOT NULL CONSTRAINT DF_Usuario_TieneContrasenaTemporal DEFAULT 0,
        VigenciaContrasenaTemporal DATETIME2(0) NULL,
        Activo         BIT           NOT NULL CONSTRAINT DF_Usuario_Activo DEFAULT 1,
        FechaRegistro  DATETIME2(0)  NOT NULL CONSTRAINT DF_Usuario_FechaRegistro DEFAULT SYSDATETIME(),

        CONSTRAINT PK_Usuario PRIMARY KEY (IdUsuario),
        CONSTRAINT UQ_Usuario_Correo UNIQUE (Correo),

        CONSTRAINT FK_Usuario_Rol
            FOREIGN KEY (IdRol) REFERENCES dbo.TB_ROL (IdRol),

        CONSTRAINT CK_Usuario_NombreNoVacio CHECK (LTRIM(RTRIM(Nombre)) <> N''),
        CONSTRAINT CK_Usuario_ApellidoNoVacio CHECK (LTRIM(RTRIM(Apellido)) <> N''),
        CONSTRAINT CK_Usuario_Correo CHECK (Correo LIKE N'%_@_%._%'),
        /* Si la contrasena es temporal tiene que traer vigencia, y si no
           es temporal la vigencia no tiene sentido. */
        CONSTRAINT CK_Usuario_VigenciaTemporal CHECK
        (
            (TieneContrasenaTemporal = 1 AND VigenciaContrasenaTemporal IS NOT NULL)
            OR
            (TieneContrasenaTemporal = 0 AND VigenciaContrasenaTemporal IS NULL)
        )
    );
END;

/* Varios usuarios pueden no tener cedula, pero dos no pueden tener la
   misma. El indice filtrado es la unica forma de lograrlo en SQL Server:
   un UNIQUE normal solo aceptaria un NULL. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Usuario_Cedula' AND object_id = OBJECT_ID('dbo.TB_USUARIO'))
BEGIN
    CREATE UNIQUE INDEX UX_Usuario_Cedula
        ON dbo.TB_USUARIO (Cedula)
        WHERE Cedula IS NOT NULL;
END;

/* ============================================================
   3. Ubicacion de Costa Rica
   ============================================================ */

IF OBJECT_ID('dbo.TB_PROVINCIA', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_PROVINCIA
    (
        IdProvincia INT           NOT NULL,
        Nombre      NVARCHAR(100) NOT NULL,

        CONSTRAINT PK_Provincia PRIMARY KEY (IdProvincia),
        CONSTRAINT UQ_Provincia_Nombre UNIQUE (Nombre)
    );
END;

IF OBJECT_ID('dbo.TB_CANTON', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_CANTON
    (
        IdCanton    INT           IDENTITY(1,1) NOT NULL,
        IdProvincia INT           NOT NULL,
        Nombre      NVARCHAR(100) NOT NULL,

        CONSTRAINT PK_Canton PRIMARY KEY (IdCanton),
        CONSTRAINT UQ_Canton_Provincia_Nombre UNIQUE (IdProvincia, Nombre),

        CONSTRAINT FK_Canton_Provincia
            FOREIGN KEY (IdProvincia) REFERENCES dbo.TB_PROVINCIA (IdProvincia)
    );
END;

IF OBJECT_ID('dbo.TB_DISTRITO', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_DISTRITO
    (
        IdDistrito INT           IDENTITY(1,1) NOT NULL,
        IdCanton   INT           NOT NULL,
        Nombre     NVARCHAR(100) NOT NULL,

        CONSTRAINT PK_Distrito PRIMARY KEY (IdDistrito),
        CONSTRAINT UQ_Distrito_Canton_Nombre UNIQUE (IdCanton, Nombre),

        CONSTRAINT FK_Distrito_Canton
            FOREIGN KEY (IdCanton) REFERENCES dbo.TB_CANTON (IdCanton)
    );
END;

IF OBJECT_ID('dbo.TB_DIRECCION_ENVIO', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_DIRECCION_ENVIO
    (
        IdDireccion        INT           IDENTITY(1,1) NOT NULL,
        IdUsuario          INT           NOT NULL,
        IdDistrito         INT           NOT NULL,
        NombreDestinatario NVARCHAR(150) NOT NULL,
        DireccionExacta    NVARCHAR(300) NOT NULL,
        Referencia         NVARCHAR(300) NULL,
        TelefonoContacto   NVARCHAR(20)  NOT NULL,
        EsPrincipal        BIT           NOT NULL CONSTRAINT DF_DireccionEnvio_EsPrincipal DEFAULT 0,
        Activo             BIT           NOT NULL CONSTRAINT DF_DireccionEnvio_Activo DEFAULT 1,

        CONSTRAINT PK_DireccionEnvio PRIMARY KEY (IdDireccion),

        CONSTRAINT FK_DireccionEnvio_Usuario
            FOREIGN KEY (IdUsuario) REFERENCES dbo.TB_USUARIO (IdUsuario),

        CONSTRAINT FK_DireccionEnvio_Distrito
            FOREIGN KEY (IdDistrito) REFERENCES dbo.TB_DISTRITO (IdDistrito),

        CONSTRAINT CK_DireccionEnvio_DestinatarioNoVacio CHECK (LTRIM(RTRIM(NombreDestinatario)) <> N''),
        CONSTRAINT CK_DireccionEnvio_DireccionNoVacia CHECK (LTRIM(RTRIM(DireccionExacta)) <> N'')
    );
END;

/* Una sola direccion principal por usuario. Se cuenta solo entre las
   activas: una direccion dada de baja que era la principal no debe
   bloquear que se marque otra. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_DireccionEnvio_PrincipalUsuario' AND object_id = OBJECT_ID('dbo.TB_DIRECCION_ENVIO'))
BEGIN
    CREATE UNIQUE INDEX UX_DireccionEnvio_PrincipalUsuario
        ON dbo.TB_DIRECCION_ENVIO (IdUsuario)
        WHERE EsPrincipal = 1 AND Activo = 1;
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DireccionEnvio_Usuario' AND object_id = OBJECT_ID('dbo.TB_DIRECCION_ENVIO'))
BEGIN
    CREATE INDEX IX_DireccionEnvio_Usuario
        ON dbo.TB_DIRECCION_ENVIO (IdUsuario, Activo);
END;

/* ============================================================
   4. Productos
   ============================================================ */

IF OBJECT_ID('dbo.TB_CATEGORIA', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_CATEGORIA
    (
        IdCategoria INT           IDENTITY(1,1) NOT NULL,
        Nombre      NVARCHAR(100) NOT NULL,
        Descripcion NVARCHAR(255) NULL,
        Activo      BIT           NOT NULL CONSTRAINT DF_Categoria_Activo DEFAULT 1,

        CONSTRAINT PK_Categoria PRIMARY KEY (IdCategoria),
        CONSTRAINT UQ_Categoria_Nombre UNIQUE (Nombre),
        CONSTRAINT CK_Categoria_NombreNoVacio CHECK (LTRIM(RTRIM(Nombre)) <> N'')
    );
END;

/* El producto no tiene catalogo de estado. Activo es la baja logica del
   administrador, y "Disponible / Agotado" se calcula de Stock. En el
   esquema viejo eran dos datos que habia que mantener sincronizados a
   mano y cada procedimiento que tocaba stock tenia que acordarse. */
IF OBJECT_ID('dbo.TB_PRODUCTO', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_PRODUCTO
    (
        IdProducto    INT            IDENTITY(1,1) NOT NULL,
        IdCategoria   INT            NOT NULL,
        Nombre        NVARCHAR(150)  NOT NULL,
        Descripcion   NVARCHAR(MAX)  NULL,
        Precio        DECIMAL(10,2)  NOT NULL,
        Stock         INT            NOT NULL CONSTRAINT DF_Producto_Stock DEFAULT 0,
        EsPiezaUnica  BIT            NOT NULL CONSTRAINT DF_Producto_EsPiezaUnica DEFAULT 0,
        Destacado     BIT            NOT NULL CONSTRAINT DF_Producto_Destacado DEFAULT 0,
        Activo        BIT            NOT NULL CONSTRAINT DF_Producto_Activo DEFAULT 1,
        FechaCreacion DATETIME2(0)   NOT NULL CONSTRAINT DF_Producto_FechaCreacion DEFAULT SYSDATETIME(),

        CONSTRAINT PK_Producto PRIMARY KEY (IdProducto),

        CONSTRAINT FK_Producto_Categoria
            FOREIGN KEY (IdCategoria) REFERENCES dbo.TB_CATEGORIA (IdCategoria),

        CONSTRAINT CK_Producto_NombreNoVacio CHECK (LTRIM(RTRIM(Nombre)) <> N''),
        CONSTRAINT CK_Producto_Precio CHECK (Precio >= 0),
        CONSTRAINT CK_Producto_Stock CHECK (Stock >= 0),
        /* Una pieza unica no puede tener mas de una unidad */
        CONSTRAINT CK_Producto_PiezaUnicaStock CHECK (EsPiezaUnica = 0 OR Stock <= 1)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Producto_Categoria' AND object_id = OBJECT_ID('dbo.TB_PRODUCTO'))
BEGIN
    CREATE INDEX IX_Producto_Categoria
        ON dbo.TB_PRODUCTO (IdCategoria, Activo)
        INCLUDE (Nombre, Precio, Stock);
END;

/* Para la portada, que pide los destacados activos */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Producto_Destacado' AND object_id = OBJECT_ID('dbo.TB_PRODUCTO'))
BEGIN
    CREATE INDEX IX_Producto_Destacado
        ON dbo.TB_PRODUCTO (Destacado, Activo)
        WHERE Destacado = 1;
END;

IF OBJECT_ID('dbo.TB_IMAGEN_PRODUCTO', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_IMAGEN_PRODUCTO
    (
        IdImagen    INT           IDENTITY(1,1) NOT NULL,
        IdProducto  INT           NOT NULL,
        RutaImagen  NVARCHAR(255) NOT NULL,
        TextoAlternativo NVARCHAR(150) NULL,
        EsPrincipal BIT           NOT NULL CONSTRAINT DF_ImagenProducto_EsPrincipal DEFAULT 0,
        Activo      BIT           NOT NULL CONSTRAINT DF_ImagenProducto_Activo DEFAULT 1,

        CONSTRAINT PK_ImagenProducto PRIMARY KEY (IdImagen),

        CONSTRAINT FK_ImagenProducto_Producto
            FOREIGN KEY (IdProducto) REFERENCES dbo.TB_PRODUCTO (IdProducto),

        CONSTRAINT CK_ImagenProducto_RutaNoVacia CHECK (LTRIM(RTRIM(RutaImagen)) <> N'')
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_ImagenProducto_PrincipalProducto' AND object_id = OBJECT_ID('dbo.TB_IMAGEN_PRODUCTO'))
BEGIN
    CREATE UNIQUE INDEX UX_ImagenProducto_PrincipalProducto
        ON dbo.TB_IMAGEN_PRODUCTO (IdProducto)
        WHERE EsPrincipal = 1 AND Activo = 1;
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ImagenProducto_Producto' AND object_id = OBJECT_ID('dbo.TB_IMAGEN_PRODUCTO'))
BEGIN
    CREATE INDEX IX_ImagenProducto_Producto
        ON dbo.TB_IMAGEN_PRODUCTO (IdProducto, Activo)
        INCLUDE (RutaImagen, EsPrincipal);
END;

/* Variante: combinacion de color, talla o modelo con su propio stock.
   El precio es del producto, no de la variante. */
IF OBJECT_ID('dbo.TB_VARIANTE_PRODUCTO', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_VARIANTE_PRODUCTO
    (
        IdVariante INT          IDENTITY(1,1) NOT NULL,
        IdProducto INT          NOT NULL,
        Color      NVARCHAR(50) NULL,
        Talla      NVARCHAR(50) NULL,
        Modelo     NVARCHAR(50) NULL,
        Stock      INT          NOT NULL CONSTRAINT DF_VarianteProducto_Stock DEFAULT 0,
        Activo     BIT          NOT NULL CONSTRAINT DF_VarianteProducto_Activo DEFAULT 1,

        CONSTRAINT PK_VarianteProducto PRIMARY KEY (IdVariante),

        CONSTRAINT FK_VarianteProducto_Producto
            FOREIGN KEY (IdProducto) REFERENCES dbo.TB_PRODUCTO (IdProducto),

        /* En SQL Server el UNIQUE trata dos NULL como iguales, asi que
           esto tambien impide repetir la variante "solo color rojo" */
        CONSTRAINT UQ_VarianteProducto_Combinacion UNIQUE (IdProducto, Color, Talla, Modelo),

        CONSTRAINT CK_VarianteProducto_Stock CHECK (Stock >= 0),
        /* Una variante sin color, ni talla, ni modelo no distingue nada */
        CONSTRAINT CK_VarianteProducto_AlgunAtributo CHECK
        (
            Color IS NOT NULL OR Talla IS NOT NULL OR Modelo IS NOT NULL
        )
    );
END;

IF OBJECT_ID('dbo.TB_FAVORITO', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_FAVORITO
    (
        IdFavorito    INT          IDENTITY(1,1) NOT NULL,
        IdUsuario     INT          NOT NULL,
        IdProducto    INT          NOT NULL,
        FechaAgregado DATETIME2(0) NOT NULL CONSTRAINT DF_Favorito_FechaAgregado DEFAULT SYSDATETIME(),

        CONSTRAINT PK_Favorito PRIMARY KEY (IdFavorito),
        CONSTRAINT UQ_Favorito_Usuario_Producto UNIQUE (IdUsuario, IdProducto),

        CONSTRAINT FK_Favorito_Usuario
            FOREIGN KEY (IdUsuario) REFERENCES dbo.TB_USUARIO (IdUsuario),

        CONSTRAINT FK_Favorito_Producto
            FOREIGN KEY (IdProducto) REFERENCES dbo.TB_PRODUCTO (IdProducto)
    );
END;

/* ============================================================
   5. Carrito
   ============================================================ */

IF OBJECT_ID('dbo.TB_CARRITO', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_CARRITO
    (
        IdCarrito     INT          IDENTITY(1,1) NOT NULL,
        IdUsuario     INT          NOT NULL,
        FechaCreacion DATETIME2(0) NOT NULL CONSTRAINT DF_Carrito_FechaCreacion DEFAULT SYSDATETIME(),
        Activo        BIT          NOT NULL CONSTRAINT DF_Carrito_Activo DEFAULT 1,

        CONSTRAINT PK_Carrito PRIMARY KEY (IdCarrito),

        CONSTRAINT FK_Carrito_Usuario
            FOREIGN KEY (IdUsuario) REFERENCES dbo.TB_USUARIO (IdUsuario)
    );
END;

/* Un solo carrito activo por usuario. En el esquema viejo nada lo
   impedia y los procedimientos sacaban el primero con TOP 1: si se
   colaba un segundo carrito, el cliente veia productos que no estaban
   en el que se le cobraba. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Carrito_UsuarioActivo' AND object_id = OBJECT_ID('dbo.TB_CARRITO'))
BEGIN
    CREATE UNIQUE INDEX UX_Carrito_UsuarioActivo
        ON dbo.TB_CARRITO (IdUsuario)
        WHERE Activo = 1;
END;

IF OBJECT_ID('dbo.TB_DETALLE_CARRITO', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_DETALLE_CARRITO
    (
        IdDetalleCarrito INT NOT NULL IDENTITY(1,1),
        IdCarrito        INT NOT NULL,
        IdProducto       INT NOT NULL,
        Cantidad         INT NOT NULL,

        CONSTRAINT PK_DetalleCarrito PRIMARY KEY (IdDetalleCarrito),
        CONSTRAINT UQ_DetalleCarrito_Carrito_Producto UNIQUE (IdCarrito, IdProducto),

        /* Si algun dia se borra un carrito, sus lineas se van con el:
           una linea de carrito no tiene valor historico (RG-97) */
        CONSTRAINT FK_DetalleCarrito_Carrito
            FOREIGN KEY (IdCarrito) REFERENCES dbo.TB_CARRITO (IdCarrito)
            ON DELETE CASCADE,

        CONSTRAINT FK_DetalleCarrito_Producto
            FOREIGN KEY (IdProducto) REFERENCES dbo.TB_PRODUCTO (IdProducto),

        CONSTRAINT CK_DetalleCarrito_Cantidad CHECK (Cantidad > 0)
    );
END;

/* ============================================================
   6. Pedidos
   ============================================================ */

IF OBJECT_ID('dbo.TB_PEDIDO', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_PEDIDO
    (
        IdPedido           INT           IDENTITY(1,1) NOT NULL,
        IdUsuario          INT           NOT NULL,
        IdDireccion        INT           NOT NULL,
        IdEstadoPedido     INT           NOT NULL,
        FechaPedido        DATETIME2(0)  NOT NULL CONSTRAINT DF_Pedido_FechaPedido DEFAULT SYSDATETIME(),
        Subtotal           DECIMAL(10,2) NOT NULL,
        CostoEnvio         DECIMAL(10,2) NOT NULL,
        Total              DECIMAL(10,2) NOT NULL,
        MotivoCancelacion  NVARCHAR(255) NULL,

        CONSTRAINT PK_Pedido PRIMARY KEY (IdPedido),

        CONSTRAINT FK_Pedido_Usuario
            FOREIGN KEY (IdUsuario) REFERENCES dbo.TB_USUARIO (IdUsuario),

        CONSTRAINT FK_Pedido_DireccionEnvio
            FOREIGN KEY (IdDireccion) REFERENCES dbo.TB_DIRECCION_ENVIO (IdDireccion),

        CONSTRAINT FK_Pedido_EstadoPedido
            FOREIGN KEY (IdEstadoPedido) REFERENCES dbo.TB_ESTADO_PEDIDO (IdEstadoPedido),

        CONSTRAINT CK_Pedido_Subtotal CHECK (Subtotal >= 0),
        CONSTRAINT CK_Pedido_CostoEnvio CHECK (CostoEnvio >= 0),
        CONSTRAINT CK_Pedido_Total CHECK (Total = Subtotal + CostoEnvio),
        /* El motivo solo tiene sentido en un pedido cancelado.
           4 es Cancelado en TB_ESTADO_PEDIDO */
        CONSTRAINT CK_Pedido_MotivoSoloCancelado CHECK
        (
            MotivoCancelacion IS NULL OR IdEstadoPedido = 4
        )
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Pedido_Usuario_Fecha' AND object_id = OBJECT_ID('dbo.TB_PEDIDO'))
BEGIN
    CREATE INDEX IX_Pedido_Usuario_Fecha
        ON dbo.TB_PEDIDO (IdUsuario, FechaPedido DESC)
        INCLUDE (IdEstadoPedido, Total);
END;

/* Los reportes filtran por rango de fechas */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Pedido_Fecha' AND object_id = OBJECT_ID('dbo.TB_PEDIDO'))
BEGIN
    CREATE INDEX IX_Pedido_Fecha
        ON dbo.TB_PEDIDO (FechaPedido)
        INCLUDE (IdEstadoPedido, Subtotal, CostoEnvio, Total);
END;

/* El detalle congela precio y cantidad: un cambio de precio posterior
   no debe alterar lo ya vendido. */
IF OBJECT_ID('dbo.TB_DETALLE_PEDIDO', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_DETALLE_PEDIDO
    (
        IdDetallePedido INT           IDENTITY(1,1) NOT NULL,
        IdPedido        INT           NOT NULL,
        IdProducto      INT           NOT NULL,
        Cantidad        INT           NOT NULL,
        PrecioUnitario  DECIMAL(10,2) NOT NULL,
        SubtotalLinea   DECIMAL(10,2) NOT NULL,

        CONSTRAINT PK_DetallePedido PRIMARY KEY (IdDetallePedido),
        CONSTRAINT UQ_DetallePedido_Pedido_Producto UNIQUE (IdPedido, IdProducto),

        CONSTRAINT FK_DetallePedido_Pedido
            FOREIGN KEY (IdPedido) REFERENCES dbo.TB_PEDIDO (IdPedido)
            ON DELETE CASCADE,

        CONSTRAINT FK_DetallePedido_Producto
            FOREIGN KEY (IdProducto) REFERENCES dbo.TB_PRODUCTO (IdProducto),

        CONSTRAINT CK_DetallePedido_Cantidad CHECK (Cantidad > 0),
        CONSTRAINT CK_DetallePedido_PrecioUnitario CHECK (PrecioUnitario >= 0),
        CONSTRAINT CK_DetallePedido_SubtotalLinea CHECK (SubtotalLinea = PrecioUnitario * Cantidad)
    );
END;

/* ============================================================
   7. Facturacion y pagos
   ============================================================ */

IF OBJECT_ID('dbo.TB_FACTURA', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_FACTURA
    (
        IdFactura       INT           IDENTITY(1,1) NOT NULL,
        IdPedido        INT           NOT NULL,
        IdEstadoFactura INT           NOT NULL,
        NumeroFactura   NVARCHAR(50)  NOT NULL,
        FechaFactura    DATETIME2(0)  NOT NULL CONSTRAINT DF_Factura_FechaFactura DEFAULT SYSDATETIME(),
        Subtotal        DECIMAL(10,2) NOT NULL,
        CostoEnvio      DECIMAL(10,2) NOT NULL,
        Total           DECIMAL(10,2) NOT NULL,

        CONSTRAINT PK_Factura PRIMARY KEY (IdFactura),

        /* Una factura por pedido, y un numero de factura irrepetible */
        CONSTRAINT UQ_Factura_Pedido UNIQUE (IdPedido),
        CONSTRAINT UQ_Factura_Numero UNIQUE (NumeroFactura),

        CONSTRAINT FK_Factura_Pedido
            FOREIGN KEY (IdPedido) REFERENCES dbo.TB_PEDIDO (IdPedido),

        CONSTRAINT FK_Factura_EstadoFactura
            FOREIGN KEY (IdEstadoFactura) REFERENCES dbo.TB_ESTADO_FACTURA (IdEstadoFactura),

        CONSTRAINT CK_Factura_Subtotal CHECK (Subtotal >= 0),
        CONSTRAINT CK_Factura_CostoEnvio CHECK (CostoEnvio >= 0),
        CONSTRAINT CK_Factura_Total CHECK (Total = Subtotal + CostoEnvio)
    );
END;

IF OBJECT_ID('dbo.TB_PAGO', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_PAGO
    (
        IdPago       INT           IDENTITY(1,1) NOT NULL,
        IdPedido     INT           NOT NULL,
        IdMetodoPago INT           NOT NULL,
        IdEstadoPago INT           NOT NULL,
        Monto        DECIMAL(10,2) NOT NULL,
        FechaPago    DATETIME2(0)  NOT NULL CONSTRAINT DF_Pago_FechaPago DEFAULT SYSDATETIME(),
        /* Pago simulado: ultimos digitos de la tarjeta, referencia de la
           transferencia o telefono del SINPE que escribio el cliente */
        Comprobante  NVARCHAR(255) NULL,

        CONSTRAINT PK_Pago PRIMARY KEY (IdPago),

        CONSTRAINT FK_Pago_Pedido
            FOREIGN KEY (IdPedido) REFERENCES dbo.TB_PEDIDO (IdPedido),

        CONSTRAINT FK_Pago_MetodoPago
            FOREIGN KEY (IdMetodoPago) REFERENCES dbo.TB_METODO_PAGO (IdMetodoPago),

        CONSTRAINT FK_Pago_EstadoPago
            FOREIGN KEY (IdEstadoPago) REFERENCES dbo.TB_ESTADO_PAGO (IdEstadoPago),

        CONSTRAINT CK_Pago_Monto CHECK (Monto >= 0)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Pago_Pedido' AND object_id = OBJECT_ID('dbo.TB_PAGO'))
BEGIN
    CREATE INDEX IX_Pago_Pedido
        ON dbo.TB_PAGO (IdPedido)
        INCLUDE (IdMetodoPago, IdEstadoPago, Monto, FechaPago);
END;

/* ============================================================
   8. Auditoria y errores

   Dos cosas distintas que se confunden facil:

   TB_BITACORA es el rastro de negocio: quien cambio que y cuando. La
   escribe el procedimiento almacenado dentro de la misma transaccion
   que el cambio, asi que es imposible que un modulo se la salte y no
   puede quedar una escritura sin su rastro (RG-94).

   TB_ERROR es el log tecnico de excepciones. La escribe la aplicacion
   con su propia conexion, nunca un procedimiento: su razon de existir
   es registrar el fallo aunque la transaccion se haya revertido, y un
   rollback se llevaria el registro.
   ============================================================ */

IF OBJECT_ID('dbo.TB_BITACORA', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_BITACORA
    (
        IdBitacora    BIGINT        IDENTITY(1,1) NOT NULL,
        /* NULL cuando la accion no la origina una persona */
        IdUsuario     INT           NULL,
        Entidad       NVARCHAR(100) NOT NULL,
        IdRegistro    INT           NULL,
        Accion        NVARCHAR(50)  NOT NULL,
        ValorAnterior NVARCHAR(MAX) NULL,
        ValorNuevo    NVARCHAR(MAX) NULL,
        FechaHora     DATETIME2(0)  NOT NULL CONSTRAINT DF_Bitacora_FechaHora DEFAULT SYSDATETIME(),

        CONSTRAINT PK_Bitacora PRIMARY KEY (IdBitacora),

        CONSTRAINT FK_Bitacora_Usuario
            FOREIGN KEY (IdUsuario) REFERENCES dbo.TB_USUARIO (IdUsuario),

        CONSTRAINT CK_Bitacora_EntidadNoVacia CHECK (LTRIM(RTRIM(Entidad)) <> N''),
        CONSTRAINT CK_Bitacora_AccionNoVacia CHECK (LTRIM(RTRIM(Accion)) <> N''),
        /* Los valores se guardan como JSON con FOR JSON PATH */
        CONSTRAINT CK_Bitacora_ValorAnteriorJson CHECK (ValorAnterior IS NULL OR ISJSON(ValorAnterior) = 1),
        CONSTRAINT CK_Bitacora_ValorNuevoJson CHECK (ValorNuevo IS NULL OR ISJSON(ValorNuevo) = 1)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Bitacora_Entidad_Registro' AND object_id = OBJECT_ID('dbo.TB_BITACORA'))
BEGIN
    CREATE INDEX IX_Bitacora_Entidad_Registro
        ON dbo.TB_BITACORA (Entidad, IdRegistro, FechaHora DESC);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Bitacora_FechaHora' AND object_id = OBJECT_ID('dbo.TB_BITACORA'))
BEGIN
    CREATE INDEX IX_Bitacora_FechaHora
        ON dbo.TB_BITACORA (FechaHora DESC)
        INCLUDE (IdUsuario, Entidad, Accion);
END;

/* Sin llave foranea a proposito: tiene que poder registrar un fallo
   incluso si lo que fallo fue resolver el usuario. */
IF OBJECT_ID('dbo.TB_ERROR', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_ERROR
    (
        IdError        INT           IDENTITY(1,1) NOT NULL,
        FechaError     DATETIME2(0)  NOT NULL CONSTRAINT DF_Error_FechaError DEFAULT SYSDATETIME(),
        Origen         NVARCHAR(150) NULL,
        Metodo         NVARCHAR(150) NULL,
        MensajeError   NVARCHAR(MAX) NOT NULL,
        DetalleError   NVARCHAR(MAX) NULL,
        LineaError     INT           NULL,
        UsuarioSistema NVARCHAR(150) NULL,
        Url            NVARCHAR(500) NULL,
        RastroPila     NVARCHAR(MAX) NULL,

        CONSTRAINT PK_Error PRIMARY KEY (IdError)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Error_FechaError' AND object_id = OBJECT_ID('dbo.TB_ERROR'))
BEGIN
    CREATE INDEX IX_Error_FechaError
        ON dbo.TB_ERROR (FechaError DESC);
END;

COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

PRINT N'01_esquema.sql: listo.';
GO

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

DECLARE @Resumen NVARCHAR(400) =
    N'05_datos_prueba.sql: listo. Administradores de prueba=' + CAST((SELECT COUNT(*) FROM dbo.TB_USUARIO WHERE Correo = N'admin@len.test' AND IdRol = 1) AS NVARCHAR(10))
  + N' Clientes de prueba=' + CAST((SELECT COUNT(*) FROM dbo.TB_USUARIO WHERE Correo = N'cliente@len.test' AND IdRol = 2) AS NVARCHAR(10))
  + N' Categorias activas=' + CAST((SELECT COUNT(*) FROM dbo.TB_CATEGORIA WHERE Activo = 1) AS NVARCHAR(10))
  + N' Productos activos=' + CAST((SELECT COUNT(*) FROM dbo.TB_PRODUCTO WHERE Activo = 1) AS NVARCHAR(10))
  + N' Imagenes activas=' + CAST((SELECT COUNT(*) FROM dbo.TB_IMAGEN_PRODUCTO WHERE Activo = 1) AS NVARCHAR(10));

PRINT @Resumen;
GO

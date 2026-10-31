/* ============================================================
   LEN Accessories - 04 Pruebas de integridad
   Correr despues de 01, 02 y 03. Es OPCIONAL: no crea ni cambia nada
   permanente.

   Cada prueba intenta a proposito una operacion que la base debe
   rechazar, y dice si la rechazo o no. Si una prueba empieza a decir
   MAL, algo se rompio en el esquema.

   Todo pasa dentro de una transaccion que se revierte al final, asi que
   al terminar la base queda exactamente como estaba, incluso si se
   corre sobre una base con datos reales. Lo unico que avanza y no se
   revierte son los contadores IDENTITY, que es inevitable y no afecta
   nada.

   No se prueban los procedimientos aqui: varios hacen ROLLBACK de su
   propia transaccion, y eso se llevaria la transaccion de esta prueba
   completa. Lo que se prueba es el esquema: llaves foraneas,
   restricciones CHECK, indices unicos y cascadas.

   Correr con SSMS, o con:
     sqlcmd -S localhost\LAINSTANCIA -E -C -f 65001 -I -i 04_pruebas_integridad.sql
   El -I es necesario: sqlcmd arranca con QUOTED_IDENTIFIER OFF y los
   indices filtrados no lo admiten.
   ============================================================ */

USE BD_LEN;
GO

SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

/* A proposito OFF: con XACT_ABORT ON el primer error esperado abortaria
   la transaccion y no se podrian correr las demas pruebas. */
SET XACT_ABORT OFF;
GO

DECLARE @Fallos INT = 0;
DECLARE @IdUsuario INT, @IdCategoria INT, @IdProducto INT, @IdProducto2 INT, @IdDistrito INT;
DECLARE @IdDireccion INT, @IdCarrito INT, @IdPedido INT, @IdImagen INT;

BEGIN TRANSACTION;

PRINT N'============================================================';
PRINT N'Preparando datos temporales (se revierten al final)';
PRINT N'============================================================';

SELECT TOP (1) @IdDistrito = IdDistrito FROM dbo.TB_DISTRITO ORDER BY IdDistrito;

IF @IdDistrito IS NULL
BEGIN
    PRINT N'No hay distritos cargados. Corra 02_catalogos.sql antes de esta prueba.';
    ROLLBACK TRANSACTION;
    RETURN;
END;

INSERT INTO dbo.TB_USUARIO (IdRol, Nombre, Apellido, Correo, ContrasenaHash, Cedula)
VALUES (2, N'Prueba', N'Integridad', N'prueba.integridad@example.com', N'hash', NULL);
SET @IdUsuario = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO dbo.TB_CATEGORIA (Nombre) VALUES (N'Categoria de prueba integridad');
SET @IdCategoria = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO dbo.TB_PRODUCTO (IdCategoria, Nombre, Precio, Stock)
VALUES (@IdCategoria, N'Producto de prueba integridad', 1000.00, 5);
SET @IdProducto = CAST(SCOPE_IDENTITY() AS INT);

/* Un segundo producto para las pruebas que necesitan una linea de
   detalle nueva sin chocar antes con el indice unico por pedido */
INSERT INTO dbo.TB_PRODUCTO (IdCategoria, Nombre, Precio, Stock)
VALUES (@IdCategoria, N'Segundo producto de prueba integridad', 1000.00, 5);
SET @IdProducto2 = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO dbo.TB_DIRECCION_ENVIO
    (IdUsuario, IdDistrito, NombreDestinatario, DireccionExacta, TelefonoContacto, EsPrincipal)
VALUES
    (@IdUsuario, @IdDistrito, N'Prueba', N'Direccion de prueba', N'88880000', 1);
SET @IdDireccion = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO dbo.TB_CARRITO (IdUsuario) VALUES (@IdUsuario);
SET @IdCarrito = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO dbo.TB_DETALLE_CARRITO (IdCarrito, IdProducto, Cantidad)
VALUES (@IdCarrito, @IdProducto, 2);

INSERT INTO dbo.TB_PEDIDO (IdUsuario, IdDireccion, IdEstadoPedido, Subtotal, CostoEnvio, Total)
VALUES (@IdUsuario, @IdDireccion, 1, 2000.00, 500.00, 2500.00);
SET @IdPedido = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO dbo.TB_DETALLE_PEDIDO (IdPedido, IdProducto, Cantidad, PrecioUnitario, SubtotalLinea)
VALUES (@IdPedido, @IdProducto, 2, 1000.00, 2000.00);

INSERT INTO dbo.TB_IMAGEN_PRODUCTO (IdProducto, RutaImagen, EsPrincipal)
VALUES (@IdProducto, N'img/prueba.jpg', 1);
SET @IdImagen = CAST(SCOPE_IDENTITY() AS INT);

PRINT N'Datos temporales listos.';
PRINT N'';
PRINT N'============================================================';
PRINT N'Llaves foraneas';
PRINT N'============================================================';

BEGIN TRY
    INSERT INTO dbo.TB_PEDIDO (IdUsuario, IdDireccion, IdEstadoPedido, Subtotal, CostoEnvio, Total)
    VALUES (999999, @IdDireccion, 1, 100, 0, 100);
    PRINT N'MAL  01 Acepto un pedido de un usuario que no existe';
    SET @Fallos += 1;
END TRY
BEGIN CATCH
    PRINT CONCAT(N'BIEN 01 Pedido de usuario inexistente rechazado (', ERROR_NUMBER(), N')');
END CATCH;

BEGIN TRY
    DELETE FROM dbo.TB_ESTADO_PEDIDO WHERE IdEstadoPedido = 1;
    PRINT N'MAL  02 Dejo borrar un estado de pedido que esta en uso';
    SET @Fallos += 1;
END TRY
BEGIN CATCH
    PRINT CONCAT(N'BIEN 02 Borrado de estado de pedido en uso rechazado (', ERROR_NUMBER(), N')');
END CATCH;

BEGIN TRY
    INSERT INTO dbo.TB_PAGO (IdPedido, IdMetodoPago, IdEstadoPago, Monto)
    VALUES (@IdPedido, 999, 2, 100);
    PRINT N'MAL  03 Acepto un pago con un metodo que no existe';
    SET @Fallos += 1;
END TRY
BEGIN CATCH
    PRINT CONCAT(N'BIEN 03 Pago con metodo inexistente rechazado (', ERROR_NUMBER(), N')');
END CATCH;

PRINT N'';
PRINT N'============================================================';
PRINT N'Restricciones CHECK';
PRINT N'============================================================';

BEGIN TRY
    INSERT INTO dbo.TB_PRODUCTO (IdCategoria, Nombre, Precio, Stock)
    VALUES (@IdCategoria, N'Precio negativo', -1, 1);
    PRINT N'MAL  04 Acepto un producto con precio negativo';
    SET @Fallos += 1;
END TRY
BEGIN CATCH
    PRINT CONCAT(N'BIEN 04 Precio negativo rechazado (', ERROR_NUMBER(), N')');
END CATCH;

BEGIN TRY
    INSERT INTO dbo.TB_PRODUCTO (IdCategoria, Nombre, Precio, Stock, EsPiezaUnica)
    VALUES (@IdCategoria, N'Pieza unica con dos', 1000, 2, 1);
    PRINT N'MAL  05 Acepto una pieza unica con stock 2';
    SET @Fallos += 1;
END TRY
BEGIN CATCH
    PRINT CONCAT(N'BIEN 05 Pieza unica con stock mayor a 1 rechazada (', ERROR_NUMBER(), N')');
END CATCH;

BEGIN TRY
    INSERT INTO dbo.TB_PEDIDO (IdUsuario, IdDireccion, IdEstadoPedido, Subtotal, CostoEnvio, Total)
    VALUES (@IdUsuario, @IdDireccion, 1, 1000, 500, 9999);
    PRINT N'MAL  06 Acepto un pedido cuyo total no es subtotal mas envio';
    SET @Fallos += 1;
END TRY
BEGIN CATCH
    PRINT CONCAT(N'BIEN 06 Total descuadrado rechazado (', ERROR_NUMBER(), N')');
END CATCH;

BEGIN TRY
    UPDATE dbo.TB_PEDIDO
    SET MotivoCancelacion = N'Motivo sin cancelar'
    WHERE IdPedido = @IdPedido;
    PRINT N'MAL  07 Dejo poner motivo de cancelacion en un pedido que no esta cancelado';
    SET @Fallos += 1;
END TRY
BEGIN CATCH
    PRINT CONCAT(N'BIEN 07 Motivo en pedido no cancelado rechazado (', ERROR_NUMBER(), N')');
END CATCH;

BEGIN TRY
    INSERT INTO dbo.TB_DETALLE_PEDIDO (IdPedido, IdProducto, Cantidad, PrecioUnitario, SubtotalLinea)
    VALUES (@IdPedido, @IdProducto2, 3, 1000.00, 1.00);
    PRINT N'MAL  08 Acepto una linea cuyo subtotal no es precio por cantidad';
    SET @Fallos += 1;
END TRY
BEGIN CATCH
    PRINT CONCAT(N'BIEN 08 Subtotal de linea descuadrado rechazado (', ERROR_NUMBER(), N')');
END CATCH;

BEGIN TRY
    INSERT INTO dbo.TB_DETALLE_CARRITO (IdCarrito, IdProducto, Cantidad)
    VALUES (@IdCarrito, @IdProducto, 0);
    PRINT N'MAL  09 Acepto una linea de carrito con cantidad cero';
    SET @Fallos += 1;
END TRY
BEGIN CATCH
    PRINT CONCAT(N'BIEN 09 Cantidad cero en el carrito rechazada (', ERROR_NUMBER(), N')');
END CATCH;

BEGIN TRY
    INSERT INTO dbo.TB_USUARIO (IdRol, Nombre, Apellido, Correo, ContrasenaHash)
    VALUES (2, N'Sin', N'Arroba', N'esto-no-es-un-correo', N'hash');
    PRINT N'MAL  10 Acepto un correo sin arroba';
    SET @Fallos += 1;
END TRY
BEGIN CATCH
    PRINT CONCAT(N'BIEN 10 Correo mal formado rechazado (', ERROR_NUMBER(), N')');
END CATCH;

BEGIN TRY
    INSERT INTO dbo.TB_VARIANTE_PRODUCTO (IdProducto, Stock)
    VALUES (@IdProducto, 1);
    PRINT N'MAL  11 Acepto una variante sin color, ni talla, ni modelo';
    SET @Fallos += 1;
END TRY
BEGIN CATCH
    PRINT CONCAT(N'BIEN 11 Variante sin ningun atributo rechazada (', ERROR_NUMBER(), N')');
END CATCH;

BEGIN TRY
    INSERT INTO dbo.TB_BITACORA (IdUsuario, Entidad, IdRegistro, Accion, ValorNuevo)
    VALUES (@IdUsuario, N'TB_PRUEBA', 1, N'Prueba', N'esto no es json');
    PRINT N'MAL  12 Acepto en la bitacora un valor que no es JSON';
    SET @Fallos += 1;
END TRY
BEGIN CATCH
    PRINT CONCAT(N'BIEN 12 Valor no JSON en la bitacora rechazado (', ERROR_NUMBER(), N')');
END CATCH;

BEGIN TRY
    UPDATE dbo.TB_USUARIO
    SET TieneContrasenaTemporal = 1, VigenciaContrasenaTemporal = NULL
    WHERE IdUsuario = @IdUsuario;
    PRINT N'MAL  13 Acepto contrasena temporal sin fecha de vigencia';
    SET @Fallos += 1;
END TRY
BEGIN CATCH
    PRINT CONCAT(N'BIEN 13 Contrasena temporal sin vigencia rechazada (', ERROR_NUMBER(), N')');
END CATCH;

PRINT N'';
PRINT N'============================================================';
PRINT N'Indices unicos, incluidos los filtrados';
PRINT N'============================================================';

BEGIN TRY
    INSERT INTO dbo.TB_USUARIO (IdRol, Nombre, Apellido, Correo, ContrasenaHash)
    VALUES (2, N'Correo', N'Repetido', N'prueba.integridad@example.com', N'hash');
    PRINT N'MAL  14 Acepto un correo repetido';
    SET @Fallos += 1;
END TRY
BEGIN CATCH
    PRINT CONCAT(N'BIEN 14 Correo repetido rechazado (', ERROR_NUMBER(), N')');
END CATCH;

/* Esta si tiene que funcionar: el indice de cedula es filtrado, asi que
   varios usuarios pueden no tener cedula. Con un UNIQUE normal esto
   fallaria y nadie podria registrarse sin cedula. */
BEGIN TRY
    INSERT INTO dbo.TB_USUARIO (IdRol, Nombre, Apellido, Correo, ContrasenaHash, Cedula)
    VALUES (2, N'Otro', N'SinCedula', N'otro.sincedula@example.com', N'hash', NULL);
    PRINT N'BIEN 15 Acepto un segundo usuario sin cedula, que es lo correcto';
END TRY
BEGIN CATCH
    PRINT CONCAT(N'MAL  15 Rechazo un segundo usuario sin cedula (', ERROR_NUMBER(), N'): ', ERROR_MESSAGE());
    SET @Fallos += 1;
END CATCH;

UPDATE dbo.TB_USUARIO SET Cedula = N'999999901' WHERE IdUsuario = @IdUsuario;

BEGIN TRY
    INSERT INTO dbo.TB_USUARIO (IdRol, Nombre, Apellido, Correo, ContrasenaHash, Cedula)
    VALUES (2, N'Cedula', N'Repetida', N'cedula.repetida@example.com', N'hash', N'999999901');
    PRINT N'MAL  16 Acepto una cedula repetida';
    SET @Fallos += 1;
END TRY
BEGIN CATCH
    PRINT CONCAT(N'BIEN 16 Cedula repetida rechazada (', ERROR_NUMBER(), N')');
END CATCH;

BEGIN TRY
    INSERT INTO dbo.TB_DIRECCION_ENVIO
        (IdUsuario, IdDistrito, NombreDestinatario, DireccionExacta, TelefonoContacto, EsPrincipal)
    VALUES
        (@IdUsuario, @IdDistrito, N'Segunda principal', N'Otra direccion', N'88880001', 1);
    PRINT N'MAL  17 Acepto dos direcciones principales activas del mismo usuario';
    SET @Fallos += 1;
END TRY
BEGIN CATCH
    PRINT CONCAT(N'BIEN 17 Segunda direccion principal rechazada (', ERROR_NUMBER(), N')');
END CATCH;

/* Esta tiene que funcionar: dar de baja la principal y marcar otra es
   justamente el flujo de SP_Direccion_Desactivar */
BEGIN TRY
    UPDATE dbo.TB_DIRECCION_ENVIO SET EsPrincipal = 0 WHERE IdDireccion = @IdDireccion;
    INSERT INTO dbo.TB_DIRECCION_ENVIO
        (IdUsuario, IdDistrito, NombreDestinatario, DireccionExacta, TelefonoContacto, EsPrincipal)
    VALUES
        (@IdUsuario, @IdDistrito, N'Nueva principal', N'Tercera direccion', N'88880002', 1);
    PRINT N'BIEN 18 Cambiar de direccion principal quitando primero la anterior si funciona';
END TRY
BEGIN CATCH
    PRINT CONCAT(N'MAL  18 No dejo cambiar de direccion principal (', ERROR_NUMBER(), N'): ', ERROR_MESSAGE());
    SET @Fallos += 1;
END CATCH;

BEGIN TRY
    INSERT INTO dbo.TB_CARRITO (IdUsuario) VALUES (@IdUsuario);
    PRINT N'MAL  19 Acepto un segundo carrito activo del mismo usuario';
    SET @Fallos += 1;
END TRY
BEGIN CATCH
    PRINT CONCAT(N'BIEN 19 Segundo carrito activo rechazado (', ERROR_NUMBER(), N')');
END CATCH;

BEGIN TRY
    INSERT INTO dbo.TB_DETALLE_CARRITO (IdCarrito, IdProducto, Cantidad)
    VALUES (@IdCarrito, @IdProducto, 1);
    PRINT N'MAL  20 Acepto dos lineas del mismo producto en un carrito';
    SET @Fallos += 1;
END TRY
BEGIN CATCH
    PRINT CONCAT(N'BIEN 20 Linea repetida en el carrito rechazada (', ERROR_NUMBER(), N')');
END CATCH;

BEGIN TRY
    INSERT INTO dbo.TB_IMAGEN_PRODUCTO (IdProducto, RutaImagen, EsPrincipal)
    VALUES (@IdProducto, N'img/segunda-principal.jpg', 1);
    PRINT N'MAL  21 Acepto dos imagenes principales activas del mismo producto';
    SET @Fallos += 1;
END TRY
BEGIN CATCH
    PRINT CONCAT(N'BIEN 21 Segunda imagen principal rechazada (', ERROR_NUMBER(), N')');
END CATCH;

BEGIN TRY
    INSERT INTO dbo.TB_FACTURA (IdPedido, IdEstadoFactura, NumeroFactura, Subtotal, CostoEnvio, Total)
    VALUES (@IdPedido, 1, N'FAC-PRUEBA-1', 2000, 500, 2500);
    INSERT INTO dbo.TB_FACTURA (IdPedido, IdEstadoFactura, NumeroFactura, Subtotal, CostoEnvio, Total)
    VALUES (@IdPedido, 1, N'FAC-PRUEBA-2', 2000, 500, 2500);
    PRINT N'MAL  22 Acepto dos facturas para el mismo pedido';
    SET @Fallos += 1;
END TRY
BEGIN CATCH
    PRINT CONCAT(N'BIEN 22 Segunda factura del mismo pedido rechazada (', ERROR_NUMBER(), N')');
END CATCH;

PRINT N'';
PRINT N'============================================================';
PRINT N'Cascadas';
PRINT N'============================================================';

DECLARE @LineasCarrito INT, @LineasPedido INT;

DELETE FROM dbo.TB_DETALLE_CARRITO WHERE IdCarrito = @IdCarrito AND IdProducto = @IdProducto;
INSERT INTO dbo.TB_DETALLE_CARRITO (IdCarrito, IdProducto, Cantidad) VALUES (@IdCarrito, @IdProducto, 1);

DELETE FROM dbo.TB_CARRITO WHERE IdCarrito = @IdCarrito;
SELECT @LineasCarrito = COUNT(*) FROM dbo.TB_DETALLE_CARRITO WHERE IdCarrito = @IdCarrito;

IF @LineasCarrito = 0
    PRINT N'BIEN 23 Al borrar un carrito se van sus lineas';
ELSE
BEGIN
    PRINT N'MAL  23 Quedaron lineas de un carrito borrado';
    SET @Fallos += 1;
END;

DELETE FROM dbo.TB_FACTURA WHERE IdPedido = @IdPedido;
DELETE FROM dbo.TB_PEDIDO WHERE IdPedido = @IdPedido;
SELECT @LineasPedido = COUNT(*) FROM dbo.TB_DETALLE_PEDIDO WHERE IdPedido = @IdPedido;

IF @LineasPedido = 0
    PRINT N'BIEN 24 Al borrar un pedido se va su detalle';
ELSE
BEGIN
    PRINT N'MAL  24 Quedaron lineas de un pedido borrado';
    SET @Fallos += 1;
END;

PRINT N'';
PRINT N'============================================================';
IF @Fallos = 0
    PRINT N'04_pruebas_integridad.sql: las 24 pruebas dieron el resultado esperado.';
ELSE
    PRINT CONCAT(N'04_pruebas_integridad.sql: ATENCION, ', @Fallos, N' prueba(s) NO dieron el resultado esperado.');
PRINT N'============================================================';

ROLLBACK TRANSACTION;
PRINT N'Todo revertido: la base quedo como estaba.';
GO

/* ============================================================
   LEN Accessories - 02 Catalogos
   Correr despues de 01_esquema.sql

   No son datos de prueba: sin esto el sistema no arranca. Los Ids de
   rol, estado y metodo de pago son fijos porque los procedimientos y
   el API los usan como constantes. No cambiarlos.

   Se puede correr varias veces. Si la fila ya existe no se toca, asi
   no se pisa lo que se haya cambiado desde el panel. Por eso cada
   insercion va con NOT EXISTS y no con un UPDATE.

   Los cantones y distritos son la Division Territorial Administrativa
   2026 del Instituto Geografico Nacional, tal como venian en el primer
   avance del proyecto.
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

/* ============================================================
   1. Roles

   Lista cerrada de dos. La autorizacion es por rol y se resuelve en la
   aplicacion; no hay tabla de permisos granulares.
   ============================================================ */

SET IDENTITY_INSERT dbo.TB_ROL ON;

INSERT INTO dbo.TB_ROL (IdRol, Nombre, Descripcion)
SELECT V.IdRol, V.Nombre, V.Descripcion
FROM
(
    VALUES
        (1, N'Administrador', N'Panel completo: productos, inventario, pedidos, pagos, reportes y usuarios'),
        (2, N'Cliente',       N'Tienda publica: su carrito, sus favoritos, sus pedidos y sus facturas')
) AS V (IdRol, Nombre, Descripcion)
WHERE NOT EXISTS (SELECT 1 FROM dbo.TB_ROL AS R WHERE R.IdRol = V.IdRol);

SET IDENTITY_INSERT dbo.TB_ROL OFF;

/* ============================================================
   2. Estados

   Un catalogo por dominio. Una sola tabla generica de estados, como
   tenia el primer avance, dejaba que la llave foranea no protegiera
   nada: permitia marcar un pedido como "Agotado".

   El producto no aparece aqui a proposito: su disponibilidad se
   calcula del stock y su baja logica es la columna Activo.
   ============================================================ */

/* Flujo del pedido:
     Pagado -> En preparacion -> Entregado
     Pagado o En preparacion -> Cancelado
   Un pedido nace en Pagado porque el pago es simulado y se confirma en
   el mismo acto de compra. Orden es la posicion en el flujo, no el Id. */
INSERT INTO dbo.TB_ESTADO_PEDIDO (IdEstadoPedido, Nombre, Orden, ClaseCss)
SELECT V.IdEstadoPedido, V.Nombre, V.Orden, V.ClaseCss
FROM
(
    VALUES
        (1, N'Pagado',         1, N'estado-pedido-pagado'),
        (2, N'En preparación', 2, N'estado-pedido-preparacion'),
        (3, N'Entregado',      3, N'estado-pedido-entregado'),
        (4, N'Cancelado',      4, N'estado-pedido-cancelado')
) AS V (IdEstadoPedido, Nombre, Orden, ClaseCss)
WHERE NOT EXISTS (SELECT 1 FROM dbo.TB_ESTADO_PEDIDO AS E WHERE E.IdEstadoPedido = V.IdEstadoPedido);

/* Hoy el checkout solo crea pagos en Pagado. Pendiente y Anulado
   quedan para cuando el pago deje de ser simulado. */
INSERT INTO dbo.TB_ESTADO_PAGO (IdEstadoPago, Nombre)
SELECT V.IdEstadoPago, V.Nombre
FROM
(
    VALUES
        (1, N'Pendiente'),
        (2, N'Pagado'),
        (3, N'Anulado')
) AS V (IdEstadoPago, Nombre)
WHERE NOT EXISTS (SELECT 1 FROM dbo.TB_ESTADO_PAGO AS E WHERE E.IdEstadoPago = V.IdEstadoPago);

INSERT INTO dbo.TB_ESTADO_FACTURA (IdEstadoFactura, Nombre)
SELECT V.IdEstadoFactura, V.Nombre
FROM
(
    VALUES
        (1, N'Emitida'),
        (2, N'Anulada')
) AS V (IdEstadoFactura, Nombre)
WHERE NOT EXISTS (SELECT 1 FROM dbo.TB_ESTADO_FACTURA AS E WHERE E.IdEstadoFactura = V.IdEstadoFactura);

/* ============================================================
   3. Metodos de pago

   EtiquetaDatoPago es lo que el checkout le pide al cliente como
   comprobante simulado. NULL cuando no se le pide nada.
   ============================================================ */

SET IDENTITY_INSERT dbo.TB_METODO_PAGO ON;

INSERT INTO dbo.TB_METODO_PAGO (IdMetodoPago, Nombre, Descripcion, EtiquetaDatoPago)
SELECT V.IdMetodoPago, V.Nombre, V.Descripcion, V.EtiquetaDatoPago
FROM
(
    VALUES
        (1, N'Tarjeta',                 N'Tarjeta de débito o crédito', N'Últimos 4 dígitos de la tarjeta'),
        (2, N'Transferencia bancaria',  N'Transferencia a cuenta IBAN', N'Número de referencia de la transferencia'),
        (3, N'SINPE Móvil',             N'Pago por SINPE Móvil',        N'Teléfono desde el que se hizo el SINPE'),
        (4, N'Efectivo contra entrega', N'Se paga al recibir el pedido', NULL)
) AS V (IdMetodoPago, Nombre, Descripcion, EtiquetaDatoPago)
WHERE NOT EXISTS (SELECT 1 FROM dbo.TB_METODO_PAGO AS M WHERE M.IdMetodoPago = V.IdMetodoPago);

SET IDENTITY_INSERT dbo.TB_METODO_PAGO OFF;

/* ============================================================
   4. Ubicacion de Costa Rica

   Provincia, canton y distrito. Se usan para la direccion de envio.
   La provincia lleva Id fijo; canton y distrito son IDENTITY, pero la
   insercion va ordenada, asi que en una instalacion limpia los Ids
   salen iguales en toda maquina.
   ============================================================ */

INSERT INTO dbo.TB_PROVINCIA (IdProvincia, Nombre)
SELECT V.IdProvincia, V.Nombre
FROM
(
    VALUES
        (1, N'San José'),
        (2, N'Alajuela'),
        (3, N'Cartago'),
        (4, N'Heredia'),
        (5, N'Guanacaste'),
        (6, N'Puntarenas'),
        (7, N'Limón')
) AS V (IdProvincia, Nombre)
WHERE NOT EXISTS (SELECT 1 FROM dbo.TB_PROVINCIA AS P WHERE P.IdProvincia = V.IdProvincia);

DECLARE @Cantones TABLE
(
    Provincia NVARCHAR(100) NOT NULL,
    Canton    NVARCHAR(100) NOT NULL
);

INSERT INTO @Cantones (Provincia, Canton)
VALUES
        (N'San José', N'San José'),
        (N'San José', N'Escazú'),
        (N'San José', N'Desamparados'),
        (N'San José', N'Puriscal'),
        (N'San José', N'Tarrazú'),
        (N'San José', N'Aserrí'),
        (N'San José', N'Mora'),
        (N'San José', N'Goicoechea'),
        (N'San José', N'Santa Ana'),
        (N'San José', N'Alajuelita'),
        (N'San José', N'Vázquez de Coronado'),
        (N'San José', N'Acosta'),
        (N'San José', N'Tibás'),
        (N'San José', N'Moravia'),
        (N'San José', N'Montes de Oca'),
        (N'San José', N'Turrubares'),
        (N'San José', N'Dota'),
        (N'San José', N'Curridabat'),
        (N'San José', N'Pérez Zeledón'),
        (N'San José', N'León Cortés Castro'),
        (N'Alajuela', N'Alajuela'),
        (N'Alajuela', N'San Ramón'),
        (N'Alajuela', N'Grecia'),
        (N'Alajuela', N'San Mateo'),
        (N'Alajuela', N'Atenas'),
        (N'Alajuela', N'Naranjo'),
        (N'Alajuela', N'Palmares'),
        (N'Alajuela', N'Poás'),
        (N'Alajuela', N'Orotina'),
        (N'Alajuela', N'San Carlos'),
        (N'Alajuela', N'Zarcero'),
        (N'Alajuela', N'Sarchí'),
        (N'Alajuela', N'Upala'),
        (N'Alajuela', N'Los Chiles'),
        (N'Alajuela', N'Guatuso'),
        (N'Alajuela', N'Río Cuarto'),
        (N'Cartago', N'Cartago'),
        (N'Cartago', N'Paraíso'),
        (N'Cartago', N'La Unión'),
        (N'Cartago', N'Jiménez'),
        (N'Cartago', N'Turrialba'),
        (N'Cartago', N'Alvarado'),
        (N'Cartago', N'Oreamuno'),
        (N'Cartago', N'El Guarco'),
        (N'Heredia', N'Heredia'),
        (N'Heredia', N'Barva'),
        (N'Heredia', N'Santo Domingo'),
        (N'Heredia', N'Santa Bárbara'),
        (N'Heredia', N'San Rafael'),
        (N'Heredia', N'San Isidro'),
        (N'Heredia', N'Belén'),
        (N'Heredia', N'Flores'),
        (N'Heredia', N'San Pablo'),
        (N'Heredia', N'Sarapiquí'),
        (N'Guanacaste', N'Liberia'),
        (N'Guanacaste', N'Nicoya'),
        (N'Guanacaste', N'Santa Cruz'),
        (N'Guanacaste', N'Bagaces'),
        (N'Guanacaste', N'Carrillo'),
        (N'Guanacaste', N'Cañas'),
        (N'Guanacaste', N'Abangares'),
        (N'Guanacaste', N'Tilarán'),
        (N'Guanacaste', N'Nandayure'),
        (N'Guanacaste', N'La Cruz'),
        (N'Guanacaste', N'Hojancha'),
        (N'Puntarenas', N'Puntarenas'),
        (N'Puntarenas', N'Esparza'),
        (N'Puntarenas', N'Buenos Aires'),
        (N'Puntarenas', N'Montes de Oro'),
        (N'Puntarenas', N'Osa'),
        (N'Puntarenas', N'Quepos'),
        (N'Puntarenas', N'Golfito'),
        (N'Puntarenas', N'Coto Brus'),
        (N'Puntarenas', N'Parrita'),
        (N'Puntarenas', N'Corredores'),
        (N'Puntarenas', N'Garabito'),
        (N'Puntarenas', N'Monteverde'),
        (N'Puntarenas', N'Puerto Jiménez'),
        (N'Limón', N'Limón'),
        (N'Limón', N'Pococí'),
        (N'Limón', N'Siquirres'),
        (N'Limón', N'Talamanca'),
        (N'Limón', N'Matina'),
        (N'Limón', N'Guácimo');

/* Si falta una provincia no se sigue: quedarian cantones huerfanos y el
   selector de direcciones del checkout mostraria una lista incompleta
   sin que nadie se diera cuenta. */
IF EXISTS
(
    SELECT 1
    FROM @Cantones AS Datos
    LEFT JOIN dbo.TB_PROVINCIA AS P
        ON P.Nombre = Datos.Provincia
    WHERE P.IdProvincia IS NULL
)
BEGIN
    THROW 50010, N'Falta una o mas provincias en TB_PROVINCIA. Corra primero la seccion 4 completa.', 1;
END;

INSERT INTO dbo.TB_CANTON (IdProvincia, Nombre)
SELECT P.IdProvincia, Datos.Canton
FROM @Cantones AS Datos
INNER JOIN dbo.TB_PROVINCIA AS P
    ON P.Nombre = Datos.Provincia
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.TB_CANTON AS C
    WHERE C.IdProvincia = P.IdProvincia
      AND C.Nombre = Datos.Canton
)
ORDER BY P.IdProvincia, Datos.Canton;

DECLARE @Distritos TABLE
(
    Provincia NVARCHAR(100) NOT NULL,
    Canton    NVARCHAR(100) NOT NULL,
    Distrito  NVARCHAR(100) NOT NULL
);

INSERT INTO @Distritos (Provincia, Canton, Distrito)
VALUES
        (N'San José', N'San José', N'Carmen'),
        (N'San José', N'San José', N'Merced'),
        (N'San José', N'San José', N'Hospital'),
        (N'San José', N'San José', N'Catedral'),
        (N'San José', N'San José', N'Zapote'),
        (N'San José', N'San José', N'San Francisco de Dos Ríos'),
        (N'San José', N'San José', N'Uruca'),
        (N'San José', N'San José', N'Mata Redonda'),
        (N'San José', N'San José', N'Pavas'),
        (N'San José', N'San José', N'Hatillo'),
        (N'San José', N'San José', N'San Sebastián'),
        (N'San José', N'Escazú', N'Escazú'),
        (N'San José', N'Escazú', N'San Antonio'),
        (N'San José', N'Escazú', N'San Rafael'),
        (N'San José', N'Desamparados', N'Desamparados'),
        (N'San José', N'Desamparados', N'San Miguel'),
        (N'San José', N'Desamparados', N'San Juan de Dios'),
        (N'San José', N'Desamparados', N'San Rafael Arriba'),
        (N'San José', N'Desamparados', N'San Antonio'),
        (N'San José', N'Desamparados', N'Frailes'),
        (N'San José', N'Desamparados', N'Patarrá'),
        (N'San José', N'Desamparados', N'San Cristobal'),
        (N'San José', N'Desamparados', N'Rosario'),
        (N'San José', N'Desamparados', N'Damas'),
        (N'San José', N'Desamparados', N'San Rafael Abajo'),
        (N'San José', N'Desamparados', N'Gravilias'),
        (N'San José', N'Desamparados', N'Los Guido'),
        (N'San José', N'Puriscal', N'Santiago'),
        (N'San José', N'Puriscal', N'Mercedes Sur'),
        (N'San José', N'Puriscal', N'Barbacoas'),
        (N'San José', N'Puriscal', N'Grifo Alto'),
        (N'San José', N'Puriscal', N'San Rafael'),
        (N'San José', N'Puriscal', N'Candelarita'),
        (N'San José', N'Puriscal', N'Desamparaditos'),
        (N'San José', N'Puriscal', N'San Antonio'),
        (N'San José', N'Puriscal', N'Chires'),
        (N'San José', N'Tarrazú', N'San Marcos'),
        (N'San José', N'Tarrazú', N'San Lorenzo'),
        (N'San José', N'Tarrazú', N'San Carlos'),
        (N'San José', N'Aserrí', N'Aserrí'),
        (N'San José', N'Aserrí', N'Tarbaca'),
        (N'San José', N'Aserrí', N'Vuelta de Jorco'),
        (N'San José', N'Aserrí', N'San Gabriel'),
        (N'San José', N'Aserrí', N'Legua'),
        (N'San José', N'Aserrí', N'Monterrey'),
        (N'San José', N'Aserrí', N'Salitrillos'),
        (N'San José', N'Mora', N'Colón'),
        (N'San José', N'Mora', N'Guayabo'),
        (N'San José', N'Mora', N'Tabarcia'),
        (N'San José', N'Mora', N'Piedras Negras'),
        (N'San José', N'Mora', N'Picagres'),
        (N'San José', N'Mora', N'Jaris'),
        (N'San José', N'Mora', N'Quitirrisí'),
        (N'San José', N'Goicoechea', N'Guadalupe'),
        (N'San José', N'Goicoechea', N'San Francisco'),
        (N'San José', N'Goicoechea', N'Calle Blancos'),
        (N'San José', N'Goicoechea', N'Mata de Plátano'),
        (N'San José', N'Goicoechea', N'Ipís'),
        (N'San José', N'Goicoechea', N'Rancho Redondo'),
        (N'San José', N'Goicoechea', N'Purral'),
        (N'San José', N'Santa Ana', N'Santa Ana'),
        (N'San José', N'Santa Ana', N'Salitral'),
        (N'San José', N'Santa Ana', N'Pozos'),
        (N'San José', N'Santa Ana', N'Uruca'),
        (N'San José', N'Santa Ana', N'Piedades'),
        (N'San José', N'Santa Ana', N'Brasil'),
        (N'San José', N'Alajuelita', N'Alajuelita'),
        (N'San José', N'Alajuelita', N'San Josecito'),
        (N'San José', N'Alajuelita', N'San Antonio'),
        (N'San José', N'Alajuelita', N'Concepción'),
        (N'San José', N'Alajuelita', N'San Felipe'),
        (N'San José', N'Vázquez de Coronado', N'San Isidro'),
        (N'San José', N'Vázquez de Coronado', N'San Rafael'),
        (N'San José', N'Vázquez de Coronado', N'Dulce Nombre de Jesús'),
        (N'San José', N'Vázquez de Coronado', N'Patalillo'),
        (N'San José', N'Vázquez de Coronado', N'Cascajal'),
        (N'San José', N'Acosta', N'San Ignacio'),
        (N'San José', N'Acosta', N'Guaitil'),
        (N'San José', N'Acosta', N'Palmichal'),
        (N'San José', N'Acosta', N'Cangrejal'),
        (N'San José', N'Acosta', N'Sabanillas'),
        (N'San José', N'Tibás', N'San Juan'),
        (N'San José', N'Tibás', N'Cinco Esquinas'),
        (N'San José', N'Tibás', N'Anselmo Llorente'),
        (N'San José', N'Tibás', N'León XIII'),
        (N'San José', N'Tibás', N'Colima'),
        (N'San José', N'Moravia', N'San Vicente'),
        (N'San José', N'Moravia', N'San Jerónimo'),
        (N'San José', N'Moravia', N'La Trinidad'),
        (N'San José', N'Montes de Oca', N'San Pedro'),
        (N'San José', N'Montes de Oca', N'Sabanilla'),
        (N'San José', N'Montes de Oca', N'Mercedes'),
        (N'San José', N'Montes de Oca', N'San Rafael'),
        (N'San José', N'Turrubares', N'San Pablo'),
        (N'San José', N'Turrubares', N'San Pedro'),
        (N'San José', N'Turrubares', N'San Juan de Mata'),
        (N'San José', N'Turrubares', N'San Luis'),
        (N'San José', N'Turrubares', N'Carara'),
        (N'San José', N'Dota', N'Santa María'),
        (N'San José', N'Dota', N'Jardín'),
        (N'San José', N'Dota', N'Copey'),
        (N'San José', N'Curridabat', N'Curridabat'),
        (N'San José', N'Curridabat', N'Granadilla'),
        (N'San José', N'Curridabat', N'Sánchez'),
        (N'San José', N'Curridabat', N'Tirrases'),
        (N'San José', N'Pérez Zeledón', N'San Isidro de El General'),
        (N'San José', N'Pérez Zeledón', N'El General'),
        (N'San José', N'Pérez Zeledón', N'Daniel Flores'),
        (N'San José', N'Pérez Zeledón', N'Rivas'),
        (N'San José', N'Pérez Zeledón', N'San Pedro'),
        (N'San José', N'Pérez Zeledón', N'Platanares'),
        (N'San José', N'Pérez Zeledón', N'Pejivalle'),
        (N'San José', N'Pérez Zeledón', N'Cajón'),
        (N'San José', N'Pérez Zeledón', N'Barú'),
        (N'San José', N'Pérez Zeledón', N'Río Nuevo'),
        (N'San José', N'Pérez Zeledón', N'Páramo'),
        (N'San José', N'Pérez Zeledón', N'La  Amistad'),
        (N'San José', N'León Cortés Castro', N'San Pablo'),
        (N'San José', N'León Cortés Castro', N'San Andrés'),
        (N'San José', N'León Cortés Castro', N'Llano Bonito'),
        (N'San José', N'León Cortés Castro', N'San Isidro'),
        (N'San José', N'León Cortés Castro', N'Santa Cruz'),
        (N'San José', N'León Cortés Castro', N'San Antonio'),
        (N'Alajuela', N'Alajuela', N'Alajuela'),
        (N'Alajuela', N'Alajuela', N'San José'),
        (N'Alajuela', N'Alajuela', N'Carrizal'),
        (N'Alajuela', N'Alajuela', N'San Antonio'),
        (N'Alajuela', N'Alajuela', N'Guácima'),
        (N'Alajuela', N'Alajuela', N'San Isidro'),
        (N'Alajuela', N'Alajuela', N'Sabanilla'),
        (N'Alajuela', N'Alajuela', N'San Rafael'),
        (N'Alajuela', N'Alajuela', N'Río Segundo'),
        (N'Alajuela', N'Alajuela', N'Desamparados'),
        (N'Alajuela', N'Alajuela', N'Turrúcares'),
        (N'Alajuela', N'Alajuela', N'Tambor'),
        (N'Alajuela', N'Alajuela', N'Garita'),
        (N'Alajuela', N'Alajuela', N'Sarapiquí'),
        (N'Alajuela', N'San Ramón', N'San Ramón'),
        (N'Alajuela', N'San Ramón', N'Santiago'),
        (N'Alajuela', N'San Ramón', N'San Juan'),
        (N'Alajuela', N'San Ramón', N'Piedades Norte'),
        (N'Alajuela', N'San Ramón', N'Piedades Sur'),
        (N'Alajuela', N'San Ramón', N'San Rafael'),
        (N'Alajuela', N'San Ramón', N'San Isidro'),
        (N'Alajuela', N'San Ramón', N'Ángeles'),
        (N'Alajuela', N'San Ramón', N'Alfaro'),
        (N'Alajuela', N'San Ramón', N'Volio'),
        (N'Alajuela', N'San Ramón', N'Concepción'),
        (N'Alajuela', N'San Ramón', N'Zapotal'),
        (N'Alajuela', N'San Ramón', N'Peñas Blancas'),
        (N'Alajuela', N'San Ramón', N'San Lorenzo'),
        (N'Alajuela', N'Grecia', N'Grecia'),
        (N'Alajuela', N'Grecia', N'San Isidro'),
        (N'Alajuela', N'Grecia', N'San José'),
        (N'Alajuela', N'Grecia', N'San Roque'),
        (N'Alajuela', N'Grecia', N'Tacares'),
        (N'Alajuela', N'Grecia', N'Puente de Piedra'),
        (N'Alajuela', N'Grecia', N'Bolivar'),
        (N'Alajuela', N'San Mateo', N'San Mateo'),
        (N'Alajuela', N'San Mateo', N'Desmonte'),
        (N'Alajuela', N'San Mateo', N'Jesús María'),
        (N'Alajuela', N'San Mateo', N'Labrador'),
        (N'Alajuela', N'Atenas', N'Atenas'),
        (N'Alajuela', N'Atenas', N'Jesús'),
        (N'Alajuela', N'Atenas', N'Mercedes'),
        (N'Alajuela', N'Atenas', N'San Isidro'),
        (N'Alajuela', N'Atenas', N'Concepción'),
        (N'Alajuela', N'Atenas', N'San José'),
        (N'Alajuela', N'Atenas', N'Santa Eulalia'),
        (N'Alajuela', N'Atenas', N'Escobal'),
        (N'Alajuela', N'Naranjo', N'Naranjo'),
        (N'Alajuela', N'Naranjo', N'San Miguel'),
        (N'Alajuela', N'Naranjo', N'San José'),
        (N'Alajuela', N'Naranjo', N'Cirrí Sur'),
        (N'Alajuela', N'Naranjo', N'San Jerónimo'),
        (N'Alajuela', N'Naranjo', N'San Juan'),
        (N'Alajuela', N'Naranjo', N'El Rosario'),
        (N'Alajuela', N'Naranjo', N'Palmitos'),
        (N'Alajuela', N'Palmares', N'Palmares'),
        (N'Alajuela', N'Palmares', N'Zaragoza'),
        (N'Alajuela', N'Palmares', N'Buenos Aires'),
        (N'Alajuela', N'Palmares', N'Santiago'),
        (N'Alajuela', N'Palmares', N'Candelaria'),
        (N'Alajuela', N'Palmares', N'Esquipulas'),
        (N'Alajuela', N'Palmares', N'La Granja'),
        (N'Alajuela', N'Poás', N'San Pedro'),
        (N'Alajuela', N'Poás', N'San Juan'),
        (N'Alajuela', N'Poás', N'San Rafael'),
        (N'Alajuela', N'Poás', N'Carrillos'),
        (N'Alajuela', N'Poás', N'Sabana Redonda'),
        (N'Alajuela', N'Orotina', N'Orotina'),
        (N'Alajuela', N'Orotina', N'El Mastate'),
        (N'Alajuela', N'Orotina', N'Hacienda Vieja'),
        (N'Alajuela', N'Orotina', N'Coyolar'),
        (N'Alajuela', N'Orotina', N'La Ceiba'),
        (N'Alajuela', N'San Carlos', N'Quesada'),
        (N'Alajuela', N'San Carlos', N'Florencia'),
        (N'Alajuela', N'San Carlos', N'Buenavista'),
        (N'Alajuela', N'San Carlos', N'Aguas Zarcas'),
        (N'Alajuela', N'San Carlos', N'Venecia'),
        (N'Alajuela', N'San Carlos', N'Pital'),
        (N'Alajuela', N'San Carlos', N'La Fortuna'),
        (N'Alajuela', N'San Carlos', N'La Tigra'),
        (N'Alajuela', N'San Carlos', N'La Palmera'),
        (N'Alajuela', N'San Carlos', N'Venado'),
        (N'Alajuela', N'San Carlos', N'Cutris'),
        (N'Alajuela', N'San Carlos', N'Monterrey'),
        (N'Alajuela', N'San Carlos', N'Pocosol'),
        (N'Alajuela', N'Zarcero', N'Zarcero'),
        (N'Alajuela', N'Zarcero', N'Laguna'),
        (N'Alajuela', N'Zarcero', N'Tapesco'),
        (N'Alajuela', N'Zarcero', N'Guadalupe'),
        (N'Alajuela', N'Zarcero', N'Palmira'),
        (N'Alajuela', N'Zarcero', N'Zapote'),
        (N'Alajuela', N'Zarcero', N'Brisas'),
        (N'Alajuela', N'Sarchí', N'Sarchí Norte'),
        (N'Alajuela', N'Sarchí', N'Sarchí Sur'),
        (N'Alajuela', N'Sarchí', N'Toro Amarillo'),
        (N'Alajuela', N'Sarchí', N'San Pedro'),
        (N'Alajuela', N'Sarchí', N'Rodríguez'),
        (N'Alajuela', N'Upala', N'Upala'),
        (N'Alajuela', N'Upala', N'Aguas Claras'),
        (N'Alajuela', N'Upala', N'San José O Pizote'),
        (N'Alajuela', N'Upala', N'Bijagua'),
        (N'Alajuela', N'Upala', N'Delicias'),
        (N'Alajuela', N'Upala', N'Dos Ríos'),
        (N'Alajuela', N'Upala', N'Yolillal'),
        (N'Alajuela', N'Upala', N'Canalete'),
        (N'Alajuela', N'Los Chiles', N'Los Chiles'),
        (N'Alajuela', N'Los Chiles', N'Caño Negro'),
        (N'Alajuela', N'Los Chiles', N'El Amparo'),
        (N'Alajuela', N'Los Chiles', N'San Jorge'),
        (N'Alajuela', N'Guatuso', N'San Rafael'),
        (N'Alajuela', N'Guatuso', N'Buenavista'),
        (N'Alajuela', N'Guatuso', N'Cote'),
        (N'Alajuela', N'Guatuso', N'Katira'),
        (N'Alajuela', N'Río Cuarto', N'Río Cuarto'),
        (N'Alajuela', N'Río Cuarto', N'Santa Rita'),
        (N'Alajuela', N'Río Cuarto', N'Santa Isabel'),
        (N'Cartago', N'Cartago', N'Oriental'),
        (N'Cartago', N'Cartago', N'Occidental'),
        (N'Cartago', N'Cartago', N'Carmen'),
        (N'Cartago', N'Cartago', N'San Nicolás'),
        (N'Cartago', N'Cartago', N'Aguacaliente o San Francisco'),
        (N'Cartago', N'Cartago', N'Guadalupe o Arenilla'),
        (N'Cartago', N'Cartago', N'Corralillo'),
        (N'Cartago', N'Cartago', N'Tierra Blanca'),
        (N'Cartago', N'Cartago', N'Dulce Nombre'),
        (N'Cartago', N'Cartago', N'Llano Grande'),
        (N'Cartago', N'Cartago', N'Quebradilla'),
        (N'Cartago', N'Paraíso', N'Paraíso'),
        (N'Cartago', N'Paraíso', N'Santiago'),
        (N'Cartago', N'Paraíso', N'Orosi'),
        (N'Cartago', N'Paraíso', N'Cachí'),
        (N'Cartago', N'Paraíso', N'Llanos de Santa Lucía'),
        (N'Cartago', N'Paraíso', N'Birrisito'),
        (N'Cartago', N'La Unión', N'Tres Ríos'),
        (N'Cartago', N'La Unión', N'San Diego'),
        (N'Cartago', N'La Unión', N'San Juan'),
        (N'Cartago', N'La Unión', N'San Rafael'),
        (N'Cartago', N'La Unión', N'Concepción'),
        (N'Cartago', N'La Unión', N'Dulce Nombre'),
        (N'Cartago', N'La Unión', N'San Ramón'),
        (N'Cartago', N'La Unión', N'Río Azul'),
        (N'Cartago', N'Jiménez', N'Juan Viñas'),
        (N'Cartago', N'Jiménez', N'Tucurrique'),
        (N'Cartago', N'Jiménez', N'Pejibaye'),
        (N'Cartago', N'Jiménez', N'La Victoria'),
        (N'Cartago', N'Turrialba', N'Turrialba'),
        (N'Cartago', N'Turrialba', N'La Suiza'),
        (N'Cartago', N'Turrialba', N'Peralta'),
        (N'Cartago', N'Turrialba', N'Santa Cruz'),
        (N'Cartago', N'Turrialba', N'Santa Teresita'),
        (N'Cartago', N'Turrialba', N'Pavones'),
        (N'Cartago', N'Turrialba', N'Tuis'),
        (N'Cartago', N'Turrialba', N'Tayutic'),
        (N'Cartago', N'Turrialba', N'Santa Rosa'),
        (N'Cartago', N'Turrialba', N'Tres Equis'),
        (N'Cartago', N'Turrialba', N'La Isabel'),
        (N'Cartago', N'Turrialba', N'Chirripó'),
        (N'Cartago', N'Alvarado', N'Pacayas'),
        (N'Cartago', N'Alvarado', N'Cervantes'),
        (N'Cartago', N'Alvarado', N'Capellades'),
        (N'Cartago', N'Oreamuno', N'San Rafael'),
        (N'Cartago', N'Oreamuno', N'Cot'),
        (N'Cartago', N'Oreamuno', N'Potrero Cerrado'),
        (N'Cartago', N'Oreamuno', N'Cipreses'),
        (N'Cartago', N'Oreamuno', N'Santa Rosa'),
        (N'Cartago', N'El Guarco', N'El Tejar'),
        (N'Cartago', N'El Guarco', N'San Isidro'),
        (N'Cartago', N'El Guarco', N'Tobosi'),
        (N'Cartago', N'El Guarco', N'Patio de Agua'),
        (N'Heredia', N'Heredia', N'Heredia'),
        (N'Heredia', N'Heredia', N'Mercedes'),
        (N'Heredia', N'Heredia', N'San Francisco'),
        (N'Heredia', N'Heredia', N'Ulloa'),
        (N'Heredia', N'Heredia', N'Varablanca'),
        (N'Heredia', N'Barva', N'Barva'),
        (N'Heredia', N'Barva', N'San Pedro'),
        (N'Heredia', N'Barva', N'San Pablo'),
        (N'Heredia', N'Barva', N'San Roque'),
        (N'Heredia', N'Barva', N'Santa Lucía'),
        (N'Heredia', N'Barva', N'San José de la Montaña'),
        (N'Heredia', N'Barva', N'Puente Salas'),
        (N'Heredia', N'Santo Domingo', N'Santo Domingo'),
        (N'Heredia', N'Santo Domingo', N'San Vicente'),
        (N'Heredia', N'Santo Domingo', N'San Miguel'),
        (N'Heredia', N'Santo Domingo', N'Paracito'),
        (N'Heredia', N'Santo Domingo', N'Santo Tomás'),
        (N'Heredia', N'Santo Domingo', N'Santa Rosa'),
        (N'Heredia', N'Santo Domingo', N'Tures'),
        (N'Heredia', N'Santo Domingo', N'Pará'),
        (N'Heredia', N'Santa Bárbara', N'Santa Bárbara'),
        (N'Heredia', N'Santa Bárbara', N'San Pedro'),
        (N'Heredia', N'Santa Bárbara', N'San Juan'),
        (N'Heredia', N'Santa Bárbara', N'Jesús'),
        (N'Heredia', N'Santa Bárbara', N'Santo Domingo'),
        (N'Heredia', N'Santa Bárbara', N'Purabá'),
        (N'Heredia', N'San Rafael', N'San Rafael'),
        (N'Heredia', N'San Rafael', N'San Josecito'),
        (N'Heredia', N'San Rafael', N'Santiago'),
        (N'Heredia', N'San Rafael', N'Ángeles'),
        (N'Heredia', N'San Rafael', N'Concepción'),
        (N'Heredia', N'San Isidro', N'San Isidro'),
        (N'Heredia', N'San Isidro', N'San José'),
        (N'Heredia', N'San Isidro', N'Concepción'),
        (N'Heredia', N'San Isidro', N'San Francisco'),
        (N'Heredia', N'Belén', N'San Antonio'),
        (N'Heredia', N'Belén', N'La Ribera'),
        (N'Heredia', N'Belén', N'La Asunción'),
        (N'Heredia', N'Flores', N'San Joaquín'),
        (N'Heredia', N'Flores', N'Barrantes'),
        (N'Heredia', N'Flores', N'Llorente'),
        (N'Heredia', N'San Pablo', N'San Pablo'),
        (N'Heredia', N'San Pablo', N'Rincón de Sabanilla'),
        (N'Heredia', N'Sarapiquí', N'Puerto Viejo'),
        (N'Heredia', N'Sarapiquí', N'La Virgen'),
        (N'Heredia', N'Sarapiquí', N'Las Horquetas'),
        (N'Heredia', N'Sarapiquí', N'Llanuras del Gaspar'),
        (N'Heredia', N'Sarapiquí', N'Cureña'),
        (N'Guanacaste', N'Liberia', N'Liberia'),
        (N'Guanacaste', N'Liberia', N'Cañas Dulces'),
        (N'Guanacaste', N'Liberia', N'Mayorga'),
        (N'Guanacaste', N'Liberia', N'Nacascolo'),
        (N'Guanacaste', N'Liberia', N'Curubandé'),
        (N'Guanacaste', N'Nicoya', N'Nicoya'),
        (N'Guanacaste', N'Nicoya', N'Mansión'),
        (N'Guanacaste', N'Nicoya', N'San Antonio'),
        (N'Guanacaste', N'Nicoya', N'Quebrada Honda'),
        (N'Guanacaste', N'Nicoya', N'Sámara'),
        (N'Guanacaste', N'Nicoya', N'Nosara'),
        (N'Guanacaste', N'Nicoya', N'Belén de Nosarita'),
        (N'Guanacaste', N'Santa Cruz', N'Santa Cruz'),
        (N'Guanacaste', N'Santa Cruz', N'Bolsón'),
        (N'Guanacaste', N'Santa Cruz', N'Veintisiete de Abril'),
        (N'Guanacaste', N'Santa Cruz', N'Tempate'),
        (N'Guanacaste', N'Santa Cruz', N'Cartagena'),
        (N'Guanacaste', N'Santa Cruz', N'Cuajiniquil'),
        (N'Guanacaste', N'Santa Cruz', N'Diriá'),
        (N'Guanacaste', N'Santa Cruz', N'Cabo Velas'),
        (N'Guanacaste', N'Santa Cruz', N'Tamarindo'),
        (N'Guanacaste', N'Bagaces', N'Bagaces'),
        (N'Guanacaste', N'Bagaces', N'La Fortuna'),
        (N'Guanacaste', N'Bagaces', N'Mogote'),
        (N'Guanacaste', N'Bagaces', N'Río Naranjo'),
        (N'Guanacaste', N'Bagaces', N'Pijije'),
        (N'Guanacaste', N'Carrillo', N'Filadelfia'),
        (N'Guanacaste', N'Carrillo', N'Palmira'),
        (N'Guanacaste', N'Carrillo', N'Sardinal'),
        (N'Guanacaste', N'Carrillo', N'Belén'),
        (N'Guanacaste', N'Cañas', N'Cañas'),
        (N'Guanacaste', N'Cañas', N'Palmira'),
        (N'Guanacaste', N'Cañas', N'San Miguel'),
        (N'Guanacaste', N'Cañas', N'Bebedero'),
        (N'Guanacaste', N'Cañas', N'Porozal'),
        (N'Guanacaste', N'Abangares', N'Las Juntas'),
        (N'Guanacaste', N'Abangares', N'Sierra'),
        (N'Guanacaste', N'Abangares', N'San Juan'),
        (N'Guanacaste', N'Abangares', N'Colorado'),
        (N'Guanacaste', N'Tilarán', N'Tilarán'),
        (N'Guanacaste', N'Tilarán', N'Quebrada Grande'),
        (N'Guanacaste', N'Tilarán', N'Tronadora'),
        (N'Guanacaste', N'Tilarán', N'Santa Rosa'),
        (N'Guanacaste', N'Tilarán', N'Líbano'),
        (N'Guanacaste', N'Tilarán', N'Tierras Morenas'),
        (N'Guanacaste', N'Tilarán', N'Arenal'),
        (N'Guanacaste', N'Tilarán', N'Cabeceras'),
        (N'Guanacaste', N'Nandayure', N'Carmona'),
        (N'Guanacaste', N'Nandayure', N'Santa Rita'),
        (N'Guanacaste', N'Nandayure', N'Zapotal'),
        (N'Guanacaste', N'Nandayure', N'San Pablo'),
        (N'Guanacaste', N'Nandayure', N'Porvenir'),
        (N'Guanacaste', N'Nandayure', N'Bejuco'),
        (N'Guanacaste', N'La Cruz', N'La Cruz'),
        (N'Guanacaste', N'La Cruz', N'Santa Cecilia'),
        (N'Guanacaste', N'La Cruz', N'La Garita'),
        (N'Guanacaste', N'La Cruz', N'Santa Elena'),
        (N'Guanacaste', N'Hojancha', N'Hojancha'),
        (N'Guanacaste', N'Hojancha', N'Monte Romo'),
        (N'Guanacaste', N'Hojancha', N'Puerto Carrillo'),
        (N'Guanacaste', N'Hojancha', N'Huacas'),
        (N'Guanacaste', N'Hojancha', N'Matambú'),
        (N'Puntarenas', N'Puntarenas', N'Puntarenas'),
        (N'Puntarenas', N'Puntarenas', N'Pitahaya'),
        (N'Puntarenas', N'Puntarenas', N'Chomes'),
        (N'Puntarenas', N'Puntarenas', N'Lepanto'),
        (N'Puntarenas', N'Puntarenas', N'Paquera'),
        (N'Puntarenas', N'Puntarenas', N'Manzanillo'),
        (N'Puntarenas', N'Puntarenas', N'Guacimal'),
        (N'Puntarenas', N'Puntarenas', N'Barranca'),
        (N'Puntarenas', N'Puntarenas', N'Isla del Coco'),
        (N'Puntarenas', N'Puntarenas', N'Cóbano'),
        (N'Puntarenas', N'Puntarenas', N'Chacarita'),
        (N'Puntarenas', N'Puntarenas', N'Chira'),
        (N'Puntarenas', N'Puntarenas', N'Acapulco'),
        (N'Puntarenas', N'Puntarenas', N'El Roble'),
        (N'Puntarenas', N'Puntarenas', N'Arancibia'),
        (N'Puntarenas', N'Esparza', N'Espíritu Santo'),
        (N'Puntarenas', N'Esparza', N'San Juan Grande'),
        (N'Puntarenas', N'Esparza', N'Macacona'),
        (N'Puntarenas', N'Esparza', N'San Rafael'),
        (N'Puntarenas', N'Esparza', N'San Jerónimo'),
        (N'Puntarenas', N'Esparza', N'Caldera'),
        (N'Puntarenas', N'Buenos Aires', N'Buenos Aires'),
        (N'Puntarenas', N'Buenos Aires', N'Volcán'),
        (N'Puntarenas', N'Buenos Aires', N'Potrero Grande'),
        (N'Puntarenas', N'Buenos Aires', N'Boruca'),
        (N'Puntarenas', N'Buenos Aires', N'Pilas'),
        (N'Puntarenas', N'Buenos Aires', N'Colinas'),
        (N'Puntarenas', N'Buenos Aires', N'Chánguena'),
        (N'Puntarenas', N'Buenos Aires', N'Biolley'),
        (N'Puntarenas', N'Buenos Aires', N'Brunka'),
        (N'Puntarenas', N'Buenos Aires', N'Cabagra'),
        (N'Puntarenas', N'Montes de Oro', N'Miramar'),
        (N'Puntarenas', N'Montes de Oro', N'La Unión'),
        (N'Puntarenas', N'Montes de Oro', N'San Isidro'),
        (N'Puntarenas', N'Osa', N'Puerto Cortés'),
        (N'Puntarenas', N'Osa', N'Palmar'),
        (N'Puntarenas', N'Osa', N'Sierpe'),
        (N'Puntarenas', N'Osa', N'Bahía Ballena'),
        (N'Puntarenas', N'Osa', N'Piedras Blancas'),
        (N'Puntarenas', N'Osa', N'Bahía Drake'),
        (N'Puntarenas', N'Quepos', N'Quepos'),
        (N'Puntarenas', N'Quepos', N'Savegre'),
        (N'Puntarenas', N'Quepos', N'Naranjito'),
        (N'Puntarenas', N'Golfito', N'Golfito'),
        (N'Puntarenas', N'Golfito', N'Guaycará'),
        (N'Puntarenas', N'Golfito', N'Pavón'),
        (N'Puntarenas', N'Coto Brus', N'San Vito'),
        (N'Puntarenas', N'Coto Brus', N'Sabalito'),
        (N'Puntarenas', N'Coto Brus', N'Aguabuena'),
        (N'Puntarenas', N'Coto Brus', N'Limoncito'),
        (N'Puntarenas', N'Coto Brus', N'Pittier'),
        (N'Puntarenas', N'Coto Brus', N'Gutiérrez Braun'),
        (N'Puntarenas', N'Parrita', N'Parrita'),
        (N'Puntarenas', N'Corredores', N'Corredor'),
        (N'Puntarenas', N'Corredores', N'La Cuesta'),
        (N'Puntarenas', N'Corredores', N'Canoas'),
        (N'Puntarenas', N'Corredores', N'Laurel'),
        (N'Puntarenas', N'Garabito', N'Jacó'),
        (N'Puntarenas', N'Garabito', N'Tárcoles'),
        (N'Puntarenas', N'Garabito', N'Lagunillas'),
        (N'Puntarenas', N'Monteverde', N'Monteverde'),
        (N'Puntarenas', N'Puerto Jiménez', N'Puerto Jiménez'),
        (N'Limón', N'Limón', N'Limón'),
        (N'Limón', N'Limón', N'Valle La Estrella'),
        (N'Limón', N'Limón', N'Río Blanco'),
        (N'Limón', N'Limón', N'Matama'),
        (N'Limón', N'Pococí', N'Guápiles'),
        (N'Limón', N'Pococí', N'Jiménez'),
        (N'Limón', N'Pococí', N'Rita'),
        (N'Limón', N'Pococí', N'Roxana'),
        (N'Limón', N'Pococí', N'Cariari'),
        (N'Limón', N'Pococí', N'Colorado'),
        (N'Limón', N'Pococí', N'La Colonia'),
        (N'Limón', N'Siquirres', N'Siquirres'),
        (N'Limón', N'Siquirres', N'Pacuarito'),
        (N'Limón', N'Siquirres', N'Florida'),
        (N'Limón', N'Siquirres', N'Germania'),
        (N'Limón', N'Siquirres', N'El Cairo'),
        (N'Limón', N'Siquirres', N'Alegría'),
        (N'Limón', N'Siquirres', N'Reventazón'),
        (N'Limón', N'Talamanca', N'Bratsi'),
        (N'Limón', N'Talamanca', N'Sixaola'),
        (N'Limón', N'Talamanca', N'Cahuita'),
        (N'Limón', N'Talamanca', N'Telire'),
        (N'Limón', N'Matina', N'Matina'),
        (N'Limón', N'Matina', N'Batán'),
        (N'Limón', N'Matina', N'Carrandí'),
        (N'Limón', N'Guácimo', N'Guácimo'),
        (N'Limón', N'Guácimo', N'Mercedes'),
        (N'Limón', N'Guácimo', N'Pocora'),
        (N'Limón', N'Guácimo', N'Río Jiménez'),
        (N'Limón', N'Guácimo', N'Duacarí');

IF EXISTS
(
    SELECT 1
    FROM @Distritos AS Datos
    LEFT JOIN dbo.TB_PROVINCIA AS P
        ON P.Nombre = Datos.Provincia
    LEFT JOIN dbo.TB_CANTON AS C
        ON C.IdProvincia = P.IdProvincia
       AND C.Nombre = Datos.Canton
    WHERE P.IdProvincia IS NULL
       OR C.IdCanton IS NULL
)
BEGIN
    THROW 50011, N'No fue posible relacionar uno o mas distritos con su canton.', 1;
END;

INSERT INTO dbo.TB_DISTRITO (IdCanton, Nombre)
SELECT C.IdCanton, Datos.Distrito
FROM @Distritos AS Datos
INNER JOIN dbo.TB_PROVINCIA AS P
    ON P.Nombre = Datos.Provincia
INNER JOIN dbo.TB_CANTON AS C
    ON C.IdProvincia = P.IdProvincia
   AND C.Nombre = Datos.Canton
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.TB_DISTRITO AS D
    WHERE D.IdCanton = C.IdCanton
      AND D.Nombre = Datos.Distrito
)
ORDER BY C.IdCanton, Datos.Distrito;

COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

/* ------------------------------------------------------------
   Resumen de lo cargado. Es un PRINT y no un SELECT para que la salida
   quepa en una linea y no llene la pantalla de resultados.
   ------------------------------------------------------------ */
DECLARE @Resumen NVARCHAR(400) =
    N'02_catalogos.sql: listo. Roles=' + CAST((SELECT COUNT(*) FROM dbo.TB_ROL) AS NVARCHAR(10))
  + N' EstadosPedido=' + CAST((SELECT COUNT(*) FROM dbo.TB_ESTADO_PEDIDO) AS NVARCHAR(10))
  + N' EstadosPago=' + CAST((SELECT COUNT(*) FROM dbo.TB_ESTADO_PAGO) AS NVARCHAR(10))
  + N' EstadosFactura=' + CAST((SELECT COUNT(*) FROM dbo.TB_ESTADO_FACTURA) AS NVARCHAR(10))
  + N' MetodosPago=' + CAST((SELECT COUNT(*) FROM dbo.TB_METODO_PAGO) AS NVARCHAR(10))
  + N' Provincias=' + CAST((SELECT COUNT(*) FROM dbo.TB_PROVINCIA) AS NVARCHAR(10))
  + N' Cantones=' + CAST((SELECT COUNT(*) FROM dbo.TB_CANTON) AS NVARCHAR(10))
  + N' Distritos=' + CAST((SELECT COUNT(*) FROM dbo.TB_DISTRITO) AS NVARCHAR(10));

PRINT @Resumen;
GO

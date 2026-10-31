/* ============================================================
   LEN Accessories - 03 Procedimientos almacenados
   Correr despues de 01_esquema.sql y 02_catalogos.sql

   Un solo archivo con todos los procedimientos del sistema, dividido
   por seccion de modulo. Todos van con CREATE OR ALTER, asi que el
   script se puede correr las veces que haga falta.

   Para agregar uno nuevo: buscar la seccion del modulo y escribirlo
   ahi. Si el que ya existe no calza con lo que necesita tu modulo,
   editalo. No crear uno paralelo.

   ------------------------------------------------------------
   Convenciones, validas para todos

   Nombre: SP_Entidad_Accion. Siempre se invoca con esquema:
   EXEC dbo.SP_Entidad_Accion.

   Errores de regla de negocio: THROW con numero 50000. El API traduce
   50000 o mayor a 400 Bad Request; cualquier otro error es 500. Esa es
   la unica forma de distinguir "el usuario hizo algo invalido" de "se
   rompio algo". Los procedimientos NO devuelven columnas Exitoso ni
   Mensaje: si no lanzaron, salio bien.

   Escrituras: primer parametro @IdUsuarioAccion, quien hace el cambio.
   Toda escritura sobre una entidad de negocio deja bitacora dentro de
   la misma transaccion (RG-94), asi que ningun modulo se la puede
   saltar.

   Contrasenas: el hash se calcula siempre en la aplicacion. Aqui solo
   se guarda, nunca se recibe en claro y nunca se escribe en bitacora.

   Ids de catalogo que se usan como constantes:
     TB_ROL             1 Administrador  2 Cliente
     TB_ESTADO_PEDIDO   1 Pagado  2 En preparacion  3 Entregado  4 Cancelado
     TB_ESTADO_PAGO     1 Pendiente  2 Pagado  3 Anulado
     TB_ESTADO_FACTURA  1 Emitida  2 Anulada

   Un pedido cuenta como venta cuando su estado es 1, 2 o 3. Cancelado
   no suma en ningun reporte ni en el tablero.

   Disponibilidad de producto: no hay columna de estado. Activo es la
   baja logica y "Disponible / Agotado" se calcula de Stock.
   ------------------------------------------------------------

   Autenticacion: no esta aqui a proposito. Leer el usuario por correo
   para comparar el hash va por Entity Framework, como manda la seccion
   6 del archivo general. Lo que si esta es la escritura de contrasena,
   porque tiene que dejar bitacora.

   Log de errores: tampoco esta aqui. TB_ERROR la escribe la aplicacion
   con su propia conexion, porque tiene que registrar el fallo aunque la
   transaccion se haya revertido, y un rollback se llevaria el registro.
   ============================================================ */

USE BD_LEN;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ============================================================
   1. Usuarios y roles
   ============================================================ */

/* Lista para el panel de administracion de usuarios.
   Devuelve: IdUsuario, Nombre, Apellido, Cedula, Telefono, Correo,
             IdRol, NombreRol, Activo, FechaRegistro
   Ejemplo:  EXEC dbo.SP_Usuario_Listar; */
CREATE OR ALTER PROCEDURE dbo.SP_Usuario_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        U.IdUsuario,
        U.Nombre,
        U.Apellido,
        U.Cedula,
        U.Telefono,
        U.Correo,
        U.IdRol,
        R.Nombre AS NombreRol,
        U.Activo,
        U.FechaRegistro
    FROM dbo.TB_USUARIO AS U
    INNER JOIN dbo.TB_ROL AS R
        ON R.IdRol = U.IdRol
    ORDER BY U.FechaRegistro DESC, U.Nombre, U.Apellido;
END;
GO

/* Un usuario por Id, para la pantalla de detalle y para llenar el
   formulario de edicion. No devuelve el hash.
   Devuelve una fila, o ninguna si el Id no existe. */
CREATE OR ALTER PROCEDURE dbo.SP_Usuario_ObtenerPorId
    @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        U.IdUsuario,
        U.Nombre,
        U.Apellido,
        U.Cedula,
        U.Telefono,
        U.Correo,
        U.IdRol,
        R.Nombre AS NombreRol,
        U.Activo,
        U.TieneContrasenaTemporal,
        U.VigenciaContrasenaTemporal,
        U.FechaRegistro
    FROM dbo.TB_USUARIO AS U
    INNER JOIN dbo.TB_ROL AS R
        ON R.IdRol = U.IdRol
    WHERE U.IdUsuario = @IdUsuario;
END;
GO

/* Para poblar el campo de seleccion de rol */
CREATE OR ALTER PROCEDURE dbo.SP_Rol_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT IdRol, Nombre, Descripcion
    FROM dbo.TB_ROL
    WHERE Activo = 1
    ORDER BY IdRol;
END;
GO

/* Registro publico: el que se crea desde la pantalla de registro de la
   tienda. Siempre queda con rol Cliente y activo; el rol no es
   parametro justamente para que no se pueda pedir Administrador desde
   el formulario publico.

   Recibe:   los datos del formulario y el hash ya calculado
   Devuelve: IdUsuario del creado
   Errores:  50000 si el correo o la cedula ya estan registrados
   Ejemplo:
     EXEC dbo.SP_Usuario_Registrar
          @Nombre = N'Ana', @Apellido = N'Mora',
          @Cedula = N'118880001', @Telefono = N'88880001',
          @Correo = N'ana@example.com', @ContrasenaHash = N'<hash>'; */
CREATE OR ALTER PROCEDURE dbo.SP_Usuario_Registrar
    @Nombre         NVARCHAR(100),
    @Apellido       NVARCHAR(100),
    @Cedula         NVARCHAR(20) = NULL,
    @Telefono       NVARCHAR(20) = NULL,
    @Correo         NVARCHAR(150),
    @ContrasenaHash NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @Nombre   = LTRIM(RTRIM(@Nombre));
    SET @Apellido = LTRIM(RTRIM(@Apellido));
    SET @Cedula   = NULLIF(LTRIM(RTRIM(@Cedula)), N'');
    SET @Telefono = NULLIF(LTRIM(RTRIM(@Telefono)), N'');
    SET @Correo   = LOWER(LTRIM(RTRIM(@Correo)));

    IF @Nombre = N'' OR @Apellido = N''
        THROW 50000, N'El nombre y el apellido son obligatorios.', 1;

    IF EXISTS (SELECT 1 FROM dbo.TB_USUARIO WHERE Correo = @Correo)
        THROW 50000, N'El correo electrónico ya está registrado.', 1;

    IF @Cedula IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.TB_USUARIO WHERE Cedula = @Cedula)
        THROW 50000, N'La cédula ya está registrada.', 1;

    DECLARE @IdUsuario INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.TB_USUARIO
            (IdRol, Nombre, Apellido, Cedula, Telefono, Correo, ContrasenaHash)
        VALUES
            (2, @Nombre, @Apellido, @Cedula, @Telefono, @Correo, @ContrasenaHash);

        SET @IdUsuario = CAST(SCOPE_IDENTITY() AS INT);

        /* El actor es el usuario recien creado: se registro el mismo */
        INSERT INTO dbo.TB_BITACORA (IdUsuario, Entidad, IdRegistro, Accion, ValorNuevo)
        VALUES (@IdUsuario, N'TB_USUARIO', @IdUsuario, N'Registro publico',
                NULLIF((SELECT @Nombre AS Nombre, @Apellido AS Apellido, @Correo AS Correo,
                               2 AS IdRol FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N''));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;

    SELECT @IdUsuario AS IdUsuario;
END;
GO

/* Alta desde el panel. A diferencia del registro publico, aqui si se
   elige rol y si se puede crear inactivo.
   Errores: 50000 por correo o cedula repetidos, o rol invalido */
CREATE OR ALTER PROCEDURE dbo.SP_Usuario_RegistrarDesdeAdministracion
    @IdUsuarioAccion INT,
    @Nombre          NVARCHAR(100),
    @Apellido        NVARCHAR(100),
    @Cedula          NVARCHAR(20) = NULL,
    @Telefono        NVARCHAR(20) = NULL,
    @Correo          NVARCHAR(150),
    @ContrasenaHash  NVARCHAR(255),
    @IdRol           INT,
    @Activo          BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @Nombre   = LTRIM(RTRIM(@Nombre));
    SET @Apellido = LTRIM(RTRIM(@Apellido));
    SET @Cedula   = NULLIF(LTRIM(RTRIM(@Cedula)), N'');
    SET @Telefono = NULLIF(LTRIM(RTRIM(@Telefono)), N'');
    SET @Correo   = LOWER(LTRIM(RTRIM(@Correo)));

    IF @Nombre = N'' OR @Apellido = N''
        THROW 50000, N'El nombre y el apellido son obligatorios.', 1;

    IF @Correo IS NULL OR @Correo = N''
        THROW 50000, N'El correo electrónico es obligatorio.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.TB_ROL WHERE IdRol = @IdRol AND Activo = 1)
        THROW 50000, N'El rol seleccionado no es válido.', 1;

    IF EXISTS (SELECT 1 FROM dbo.TB_USUARIO WHERE Correo = @Correo)
        THROW 50000, N'El correo electrónico ya está registrado.', 1;

    IF @Cedula IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.TB_USUARIO WHERE Cedula = @Cedula)
        THROW 50000, N'La cédula ya está registrada.', 1;

    DECLARE @IdUsuario INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.TB_USUARIO
            (IdRol, Nombre, Apellido, Cedula, Telefono, Correo, ContrasenaHash, Activo)
        VALUES
            (@IdRol, @Nombre, @Apellido, @Cedula, @Telefono, @Correo, @ContrasenaHash, @Activo);

        SET @IdUsuario = CAST(SCOPE_IDENTITY() AS INT);

        INSERT INTO dbo.TB_BITACORA (IdUsuario, Entidad, IdRegistro, Accion, ValorNuevo)
        VALUES (@IdUsuarioAccion, N'TB_USUARIO', @IdUsuario, N'Crear',
                NULLIF((SELECT @Nombre AS Nombre, @Apellido AS Apellido, @Correo AS Correo,
                               @IdRol AS IdRol, @Activo AS Activo
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N''));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;

    SELECT @IdUsuario AS IdUsuario;
END;
GO

/* Edicion de datos del usuario, desde el panel o desde "mi cuenta".
   No toca la contrasena: para eso estan los dos de abajo.
   Errores: 50000 si el usuario no existe, o si el correo o la cedula
            ya los tiene otro */
CREATE OR ALTER PROCEDURE dbo.SP_Usuario_Actualizar
    @IdUsuarioAccion INT,
    @IdUsuario       INT,
    @Nombre          NVARCHAR(100),
    @Apellido        NVARCHAR(100),
    @Cedula          NVARCHAR(20) = NULL,
    @Telefono        NVARCHAR(20) = NULL,
    @Correo          NVARCHAR(150),
    @IdRol           INT,
    @Activo          BIT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @Nombre   = LTRIM(RTRIM(@Nombre));
    SET @Apellido = LTRIM(RTRIM(@Apellido));
    SET @Cedula   = NULLIF(LTRIM(RTRIM(@Cedula)), N'');
    SET @Telefono = NULLIF(LTRIM(RTRIM(@Telefono)), N'');
    SET @Correo   = LOWER(LTRIM(RTRIM(@Correo)));

    IF NOT EXISTS (SELECT 1 FROM dbo.TB_USUARIO WHERE IdUsuario = @IdUsuario)
        THROW 50000, N'El usuario indicado no existe.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.TB_ROL WHERE IdRol = @IdRol AND Activo = 1)
        THROW 50000, N'El rol seleccionado no es válido.', 1;

    IF EXISTS (SELECT 1 FROM dbo.TB_USUARIO WHERE Correo = @Correo AND IdUsuario <> @IdUsuario)
        THROW 50000, N'El correo electrónico ya está registrado.', 1;

    IF @Cedula IS NOT NULL
       AND EXISTS (SELECT 1 FROM dbo.TB_USUARIO WHERE Cedula = @Cedula AND IdUsuario <> @IdUsuario)
        THROW 50000, N'La cédula ya está registrada.', 1;

    /* Ultimo administrador activo: si se le quita el rol o se desactiva,
       nadie puede volver a entrar al panel. */
    IF EXISTS (SELECT 1 FROM dbo.TB_USUARIO WHERE IdUsuario = @IdUsuario AND IdRol = 1 AND Activo = 1)
       AND (@IdRol <> 1 OR @Activo = 0)
       AND NOT EXISTS (SELECT 1 FROM dbo.TB_USUARIO
                       WHERE IdRol = 1 AND Activo = 1 AND IdUsuario <> @IdUsuario)
        THROW 50000, N'Es el único administrador activo: no se puede cambiar su rol ni desactivarlo.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @Anterior NVARCHAR(MAX) =
            NULLIF((SELECT Nombre, Apellido, Cedula, Telefono, Correo, IdRol, Activo
                    FROM dbo.TB_USUARIO WHERE IdUsuario = @IdUsuario
                    FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N'');

        UPDATE dbo.TB_USUARIO
        SET Nombre   = @Nombre,
            Apellido = @Apellido,
            Cedula   = @Cedula,
            Telefono = @Telefono,
            Correo   = @Correo,
            IdRol    = @IdRol,
            Activo   = @Activo
        WHERE IdUsuario = @IdUsuario;

        INSERT INTO dbo.TB_BITACORA (IdUsuario, Entidad, IdRegistro, Accion, ValorAnterior, ValorNuevo)
        VALUES (@IdUsuarioAccion, N'TB_USUARIO', @IdUsuario, N'Actualizar', @Anterior,
                NULLIF((SELECT Nombre, Apellido, Cedula, Telefono, Correo, IdRol, Activo
                        FROM dbo.TB_USUARIO WHERE IdUsuario = @IdUsuario
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N''));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

/* Baja y alta logica del usuario (RG-97). Nunca borrado fisico: sus
   pedidos y facturas tienen valor historico.
   Errores: 50000 si no existe, o si es el ultimo administrador activo */
CREATE OR ALTER PROCEDURE dbo.SP_Usuario_CambiarActivo
    @IdUsuarioAccion INT,
    @IdUsuario       INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @ActivoActual BIT, @IdRol INT;

    SELECT @ActivoActual = Activo, @IdRol = IdRol
    FROM dbo.TB_USUARIO
    WHERE IdUsuario = @IdUsuario;

    IF @ActivoActual IS NULL
        THROW 50000, N'El usuario indicado no existe.', 1;

    IF @ActivoActual = 1 AND @IdRol = 1
       AND NOT EXISTS (SELECT 1 FROM dbo.TB_USUARIO
                       WHERE IdRol = 1 AND Activo = 1 AND IdUsuario <> @IdUsuario)
        THROW 50000, N'Es el único administrador activo: no se puede desactivar.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.TB_USUARIO
        SET Activo = CASE WHEN @ActivoActual = 1 THEN 0 ELSE 1 END
        WHERE IdUsuario = @IdUsuario;

        INSERT INTO dbo.TB_BITACORA (IdUsuario, Entidad, IdRegistro, Accion, ValorAnterior, ValorNuevo)
        VALUES (@IdUsuarioAccion, N'TB_USUARIO', @IdUsuario,
                CASE WHEN @ActivoActual = 1 THEN N'Desactivar' ELSE N'Activar' END,
                NULLIF((SELECT @ActivoActual AS Activo FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N''),
                NULLIF((SELECT CASE WHEN @ActivoActual = 1 THEN 0 ELSE 1 END AS Activo
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N''));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

/* Contrasena temporal para el flujo de "olvide mi contrasena". El hash
   y la vigencia los calcula la aplicacion.
   La bitacora registra que se genero, nunca el hash (RD-15). */
CREATE OR ALTER PROCEDURE dbo.SP_Usuario_AsignarContrasenaTemporal
    @IdUsuarioAccion INT,
    @IdUsuario       INT,
    @ContrasenaHash  NVARCHAR(255),
    @Vigencia        DATETIME2(0)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @Vigencia IS NULL
        THROW 50000, N'La contraseña temporal necesita una fecha de vigencia.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.TB_USUARIO WHERE IdUsuario = @IdUsuario AND Activo = 1)
        THROW 50000, N'El usuario indicado no existe o está inactivo.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.TB_USUARIO
        SET ContrasenaHash             = @ContrasenaHash,
            TieneContrasenaTemporal    = 1,
            VigenciaContrasenaTemporal = @Vigencia
        WHERE IdUsuario = @IdUsuario;

        INSERT INTO dbo.TB_BITACORA (IdUsuario, Entidad, IdRegistro, Accion, ValorNuevo)
        VALUES (@IdUsuarioAccion, N'TB_USUARIO', @IdUsuario, N'Contrasena temporal generada',
                NULLIF((SELECT @Vigencia AS VigenciaContrasenaTemporal
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N''));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

/* Contrasena definitiva: la usa tanto el cambio desde "mi cuenta" como
   el canje de una contrasena temporal. Al guardarla se limpia la marca
   de temporal. */
CREATE OR ALTER PROCEDURE dbo.SP_Usuario_RestablecerContrasena
    @IdUsuarioAccion INT,
    @IdUsuario       INT,
    @ContrasenaHash  NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.TB_USUARIO WHERE IdUsuario = @IdUsuario)
        THROW 50000, N'El usuario indicado no existe.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.TB_USUARIO
        SET ContrasenaHash             = @ContrasenaHash,
            TieneContrasenaTemporal    = 0,
            VigenciaContrasenaTemporal = NULL
        WHERE IdUsuario = @IdUsuario;

        INSERT INTO dbo.TB_BITACORA (IdUsuario, Entidad, IdRegistro, Accion)
        VALUES (@IdUsuarioAccion, N'TB_USUARIO', @IdUsuario, N'Contrasena restablecida');

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

/* ============================================================
   2. Ubicacion y direcciones de envio

   Los tres primeros alimentan los campos de seleccion en cascada
   provincia -> canton -> distrito del formulario de direccion.
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.SP_Provincia_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT IdProvincia, Nombre
    FROM dbo.TB_PROVINCIA
    ORDER BY Nombre;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_Canton_ListarPorProvincia
    @IdProvincia INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT IdCanton, IdProvincia, Nombre
    FROM dbo.TB_CANTON
    WHERE IdProvincia = @IdProvincia
    ORDER BY Nombre;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_Distrito_ListarPorCanton
    @IdCanton INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT IdDistrito, IdCanton, Nombre
    FROM dbo.TB_DISTRITO
    WHERE IdCanton = @IdCanton
    ORDER BY Nombre;
END;
GO

/* Direcciones del usuario, con la ubicacion ya resuelta para no tener
   que armarla en la vista. La principal sale primero.
   @SoloActivas = 1 es lo que necesita el checkout; el panel de "mi
   cuenta" las pide todas para poder mostrar las dadas de baja. */
CREATE OR ALTER PROCEDURE dbo.SP_Direccion_ListarPorUsuario
    @IdUsuario   INT,
    @SoloActivas BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        DE.IdDireccion,
        DE.IdUsuario,
        P.IdProvincia,
        P.Nombre  AS NombreProvincia,
        C.IdCanton,
        C.Nombre  AS NombreCanton,
        D.IdDistrito,
        D.Nombre  AS NombreDistrito,
        DE.NombreDestinatario,
        DE.DireccionExacta,
        DE.Referencia,
        DE.TelefonoContacto,
        DE.EsPrincipal,
        DE.Activo
    FROM dbo.TB_DIRECCION_ENVIO AS DE
    INNER JOIN dbo.TB_DISTRITO  AS D ON D.IdDistrito  = DE.IdDistrito
    INNER JOIN dbo.TB_CANTON    AS C ON C.IdCanton    = D.IdCanton
    INNER JOIN dbo.TB_PROVINCIA AS P ON P.IdProvincia = C.IdProvincia
    WHERE DE.IdUsuario = @IdUsuario
      AND (@SoloActivas = 0 OR DE.Activo = 1)
    ORDER BY DE.EsPrincipal DESC, DE.IdDireccion DESC;
END;
GO

/* Una direccion concreta. Pide el IdUsuario a proposito: asi un cliente
   no puede leer la direccion de otro cambiando el Id en la URL. */
CREATE OR ALTER PROCEDURE dbo.SP_Direccion_ObtenerPorId
    @IdDireccion INT,
    @IdUsuario   INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        DE.IdDireccion,
        DE.IdUsuario,
        P.IdProvincia,
        P.Nombre  AS NombreProvincia,
        C.IdCanton,
        C.Nombre  AS NombreCanton,
        D.IdDistrito,
        D.Nombre  AS NombreDistrito,
        DE.NombreDestinatario,
        DE.DireccionExacta,
        DE.Referencia,
        DE.TelefonoContacto,
        DE.EsPrincipal,
        DE.Activo
    FROM dbo.TB_DIRECCION_ENVIO AS DE
    INNER JOIN dbo.TB_DISTRITO  AS D ON D.IdDistrito  = DE.IdDistrito
    INNER JOIN dbo.TB_CANTON    AS C ON C.IdCanton    = D.IdCanton
    INNER JOIN dbo.TB_PROVINCIA AS P ON P.IdProvincia = C.IdProvincia
    WHERE DE.IdDireccion = @IdDireccion
      AND DE.IdUsuario   = @IdUsuario;
END;
GO

/* Nueva direccion. Si viene marcada como principal, primero se le quita
   la marca a la que la tuviera: el indice unico filtrado del esquema no
   deja dos principales activas y reventaria.
   La primera direccion del usuario queda principal aunque no se pida.

   Devuelve: IdDireccion de la creada
   Errores:  50000 si el distrito no existe */
CREATE OR ALTER PROCEDURE dbo.SP_Direccion_Registrar
    @IdUsuarioAccion    INT,
    @IdUsuario          INT,
    @IdDistrito         INT,
    @NombreDestinatario NVARCHAR(150),
    @DireccionExacta    NVARCHAR(300),
    @Referencia         NVARCHAR(300) = NULL,
    @TelefonoContacto   NVARCHAR(20),
    @EsPrincipal        BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @NombreDestinatario = LTRIM(RTRIM(@NombreDestinatario));
    SET @DireccionExacta    = LTRIM(RTRIM(@DireccionExacta));
    SET @Referencia         = NULLIF(LTRIM(RTRIM(@Referencia)), N'');
    SET @TelefonoContacto   = LTRIM(RTRIM(@TelefonoContacto));

    IF NOT EXISTS (SELECT 1 FROM dbo.TB_DISTRITO WHERE IdDistrito = @IdDistrito)
        THROW 50000, N'El distrito seleccionado no es válido.', 1;

    IF @NombreDestinatario = N'' OR @DireccionExacta = N'' OR @TelefonoContacto = N''
        THROW 50000, N'El destinatario, la dirección exacta y el teléfono son obligatorios.', 1;

    /* Si es la primera del usuario, principal por fuerza */
    IF NOT EXISTS (SELECT 1 FROM dbo.TB_DIRECCION_ENVIO
                   WHERE IdUsuario = @IdUsuario AND Activo = 1)
        SET @EsPrincipal = 1;

    DECLARE @IdDireccion INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @EsPrincipal = 1
        BEGIN
            UPDATE dbo.TB_DIRECCION_ENVIO
            SET EsPrincipal = 0
            WHERE IdUsuario = @IdUsuario AND EsPrincipal = 1;
        END;

        INSERT INTO dbo.TB_DIRECCION_ENVIO
            (IdUsuario, IdDistrito, NombreDestinatario, DireccionExacta,
             Referencia, TelefonoContacto, EsPrincipal)
        VALUES
            (@IdUsuario, @IdDistrito, @NombreDestinatario, @DireccionExacta,
             @Referencia, @TelefonoContacto, @EsPrincipal);

        SET @IdDireccion = CAST(SCOPE_IDENTITY() AS INT);

        INSERT INTO dbo.TB_BITACORA (IdUsuario, Entidad, IdRegistro, Accion, ValorNuevo)
        VALUES (@IdUsuarioAccion, N'TB_DIRECCION_ENVIO', @IdDireccion, N'Crear',
                NULLIF((SELECT IdUsuario, IdDistrito, NombreDestinatario, DireccionExacta,
                               TelefonoContacto, EsPrincipal
                        FROM dbo.TB_DIRECCION_ENVIO WHERE IdDireccion = @IdDireccion
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N''));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;

    SELECT @IdDireccion AS IdDireccion;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_Direccion_Actualizar
    @IdUsuarioAccion    INT,
    @IdDireccion        INT,
    @IdUsuario          INT,
    @IdDistrito         INT,
    @NombreDestinatario NVARCHAR(150),
    @DireccionExacta    NVARCHAR(300),
    @Referencia         NVARCHAR(300) = NULL,
    @TelefonoContacto   NVARCHAR(20),
    @EsPrincipal        BIT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @NombreDestinatario = LTRIM(RTRIM(@NombreDestinatario));
    SET @DireccionExacta    = LTRIM(RTRIM(@DireccionExacta));
    SET @Referencia         = NULLIF(LTRIM(RTRIM(@Referencia)), N'');
    SET @TelefonoContacto   = LTRIM(RTRIM(@TelefonoContacto));

    IF NOT EXISTS (SELECT 1 FROM dbo.TB_DIRECCION_ENVIO
                   WHERE IdDireccion = @IdDireccion AND IdUsuario = @IdUsuario AND Activo = 1)
        THROW 50000, N'La dirección indicada no existe o no pertenece al usuario.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.TB_DISTRITO WHERE IdDistrito = @IdDistrito)
        THROW 50000, N'El distrito seleccionado no es válido.', 1;

    IF @NombreDestinatario = N'' OR @DireccionExacta = N'' OR @TelefonoContacto = N''
        THROW 50000, N'El destinatario, la dirección exacta y el teléfono son obligatorios.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @Anterior NVARCHAR(MAX) =
            NULLIF((SELECT IdDistrito, NombreDestinatario, DireccionExacta, Referencia,
                           TelefonoContacto, EsPrincipal
                    FROM dbo.TB_DIRECCION_ENVIO WHERE IdDireccion = @IdDireccion
                    FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N'');

        IF @EsPrincipal = 1
        BEGIN
            UPDATE dbo.TB_DIRECCION_ENVIO
            SET EsPrincipal = 0
            WHERE IdUsuario = @IdUsuario
              AND IdDireccion <> @IdDireccion
              AND EsPrincipal = 1;
        END;

        UPDATE dbo.TB_DIRECCION_ENVIO
        SET IdDistrito         = @IdDistrito,
            NombreDestinatario = @NombreDestinatario,
            DireccionExacta    = @DireccionExacta,
            Referencia         = @Referencia,
            TelefonoContacto   = @TelefonoContacto,
            EsPrincipal        = @EsPrincipal
        WHERE IdDireccion = @IdDireccion
          AND IdUsuario   = @IdUsuario;

        INSERT INTO dbo.TB_BITACORA (IdUsuario, Entidad, IdRegistro, Accion, ValorAnterior, ValorNuevo)
        VALUES (@IdUsuarioAccion, N'TB_DIRECCION_ENVIO', @IdDireccion, N'Actualizar', @Anterior,
                NULLIF((SELECT IdDistrito, NombreDestinatario, DireccionExacta, Referencia,
                               TelefonoContacto, EsPrincipal
                        FROM dbo.TB_DIRECCION_ENVIO WHERE IdDireccion = @IdDireccion
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N''));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

/* Baja logica de la direccion. No se borra fisico porque los pedidos
   viejos apuntan a ella y hay que poder ver a donde se enviaron.

   Si la que se da de baja era la principal, se promueve la mas reciente
   de las que quedan: si no, el checkout se queda sin direccion marcada
   y el cliente tiene que ir a "mi cuenta" a arreglarlo sin saber por que. */
CREATE OR ALTER PROCEDURE dbo.SP_Direccion_Desactivar
    @IdUsuarioAccion INT,
    @IdDireccion     INT,
    @IdUsuario       INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @EraPrincipal BIT;

    SELECT @EraPrincipal = EsPrincipal
    FROM dbo.TB_DIRECCION_ENVIO
    WHERE IdDireccion = @IdDireccion AND IdUsuario = @IdUsuario AND Activo = 1;

    IF @EraPrincipal IS NULL
        THROW 50000, N'La dirección indicada no existe o ya está dada de baja.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.TB_DIRECCION_ENVIO
        SET Activo = 0, EsPrincipal = 0
        WHERE IdDireccion = @IdDireccion AND IdUsuario = @IdUsuario;

        IF @EraPrincipal = 1
        BEGIN
            UPDATE dbo.TB_DIRECCION_ENVIO
            SET EsPrincipal = 1
            WHERE IdDireccion =
            (
                SELECT TOP (1) IdDireccion
                FROM dbo.TB_DIRECCION_ENVIO
                WHERE IdUsuario = @IdUsuario AND Activo = 1
                ORDER BY IdDireccion DESC
            );
        END;

        INSERT INTO dbo.TB_BITACORA (IdUsuario, Entidad, IdRegistro, Accion)
        VALUES (@IdUsuarioAccion, N'TB_DIRECCION_ENVIO', @IdDireccion, N'Desactivar');

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

/* ============================================================
   3. Catalogo publico

   Para el cliente, un producto existe si Activo = 1. Un producto dado
   de baja por el administrador no aparece ni en el catalogo ni en el
   detalle: para el cliente es como si no existiera.

   Agotado no es lo mismo que dado de baja. El agotado se sigue
   mostrando, con su etiqueta, porque vuelve a haber stock; el dado de
   baja no vuelve.
   ============================================================ */

/* Categorias con al menos un producto visible. Alimenta el menu lateral
   y el campo de seleccion del filtro. */
CREATE OR ALTER PROCEDURE dbo.SP_Categoria_ListarActivas
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        C.IdCategoria,
        C.Nombre,
        C.Descripcion,
        (
            SELECT COUNT(*)
            FROM dbo.TB_PRODUCTO AS P
            WHERE P.IdCategoria = C.IdCategoria
              AND P.Activo = 1
        ) AS CantidadProductos
    FROM dbo.TB_CATEGORIA AS C
    WHERE C.Activo = 1
    ORDER BY C.Nombre;
END;
GO

/* Catalogo con busqueda, filtro y paginacion.

   Recibe:
     @Busqueda        texto libre sobre el nombre, NULL para no filtrar
     @IdCategoria     NULL para todas
     @Color           filtra por color de variante activa, NULL para todos
     @IdUsuarioActual para saber que productos ya son favoritos del que
                      esta viendo; NULL cuando nadie inicio sesion
     @Pagina          arranca en 1
     @TamanoPagina    12 por defecto

   Devuelve una fila por producto con TotalFilas repetido en cada una,
   que es lo que necesita la paginacion para saber cuantas paginas hay.

   Ejemplo:
     EXEC dbo.SP_Catalogo_Listar @Busqueda = N'collar', @Pagina = 1,
                                 @TamanoPagina = 12; */
CREATE OR ALTER PROCEDURE dbo.SP_Catalogo_Listar
    @Busqueda        NVARCHAR(150) = NULL,
    @IdCategoria     INT = NULL,
    @Color           NVARCHAR(50) = NULL,
    @IdUsuarioActual INT = NULL,
    @Pagina          INT = 1,
    @TamanoPagina    INT = 12
AS
BEGIN
    SET NOCOUNT ON;

    IF @Pagina IS NULL OR @Pagina < 1 SET @Pagina = 1;
    IF @TamanoPagina IS NULL OR @TamanoPagina < 1 SET @TamanoPagina = 12;

    SET @Busqueda = NULLIF(LTRIM(RTRIM(@Busqueda)), N'');
    SET @Color    = NULLIF(LTRIM(RTRIM(@Color)), N'');

    SELECT
        P.IdProducto,
        P.Nombre AS NombreProducto,
        P.Precio,
        P.Stock,
        P.EsPiezaUnica,
        P.Destacado,
        P.IdCategoria,
        C.Nombre AS NombreCategoria,
        CASE WHEN P.Stock > 0 THEN N'Disponible' ELSE N'Agotado' END AS NombreEstado,
        (
            SELECT TOP (1) I.RutaImagen
            FROM dbo.TB_IMAGEN_PRODUCTO AS I
            WHERE I.IdProducto = P.IdProducto
              AND I.Activo = 1
            ORDER BY I.EsPrincipal DESC, I.IdImagen
        ) AS RutaImagen,
        CASE
            WHEN @IdUsuarioActual IS NULL THEN CAST(0 AS BIT)
            WHEN EXISTS (SELECT 1 FROM dbo.TB_FAVORITO AS F
                         WHERE F.IdUsuario = @IdUsuarioActual
                           AND F.IdProducto = P.IdProducto) THEN CAST(1 AS BIT)
            ELSE CAST(0 AS BIT)
        END AS EsFavorito,
        COUNT(*) OVER () AS TotalFilas
    FROM dbo.TB_PRODUCTO AS P
    INNER JOIN dbo.TB_CATEGORIA AS C
        ON C.IdCategoria = P.IdCategoria
    WHERE P.Activo = 1
      AND (@Busqueda IS NULL OR P.Nombre LIKE N'%' + @Busqueda + N'%')
      AND (@IdCategoria IS NULL OR P.IdCategoria = @IdCategoria)
      AND (@Color IS NULL OR EXISTS (SELECT 1
                                     FROM dbo.TB_VARIANTE_PRODUCTO AS V
                                     WHERE V.IdProducto = P.IdProducto
                                       AND V.Activo = 1
                                       AND V.Color = @Color))
    ORDER BY P.Destacado DESC, P.FechaCreacion DESC, P.IdProducto DESC
    OFFSET (@Pagina - 1) * @TamanoPagina ROWS
    FETCH NEXT @TamanoPagina ROWS ONLY;
END;
GO

/* Detalle de un producto para la tienda. No devuelve fila si el
   producto esta dado de baja. */
CREATE OR ALTER PROCEDURE dbo.SP_Producto_ObtenerDetalle
    @IdProducto      INT,
    @IdUsuarioActual INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        P.IdProducto,
        P.Nombre AS NombreProducto,
        P.Descripcion,
        P.Precio,
        P.Stock,
        P.EsPiezaUnica,
        P.IdCategoria,
        C.Nombre AS NombreCategoria,
        CASE WHEN P.Stock > 0 THEN N'Disponible' ELSE N'Agotado' END AS NombreEstado,
        CASE
            WHEN @IdUsuarioActual IS NULL THEN CAST(0 AS BIT)
            WHEN EXISTS (SELECT 1 FROM dbo.TB_FAVORITO AS F
                         WHERE F.IdUsuario = @IdUsuarioActual
                           AND F.IdProducto = P.IdProducto) THEN CAST(1 AS BIT)
            ELSE CAST(0 AS BIT)
        END AS EsFavorito
    FROM dbo.TB_PRODUCTO AS P
    INNER JOIN dbo.TB_CATEGORIA AS C
        ON C.IdCategoria = P.IdCategoria
    WHERE P.IdProducto = @IdProducto
      AND P.Activo = 1;
END;
GO

/* Galeria del producto en la tienda. La principal primero, que es la
   que el visor de imagenes muestra abierta. */
CREATE OR ALTER PROCEDURE dbo.SP_Catalogo_ListarImagenes
    @IdProducto INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        I.IdImagen,
        I.IdProducto,
        I.RutaImagen,
        I.TextoAlternativo,
        I.EsPrincipal
    FROM dbo.TB_IMAGEN_PRODUCTO AS I
    WHERE I.IdProducto = @IdProducto
      AND I.Activo = 1
    ORDER BY I.EsPrincipal DESC, I.IdImagen;
END;
GO

/* Variantes con stock propio. El precio no sale aqui porque es del
   producto, no de la variante. */
CREATE OR ALTER PROCEDURE dbo.SP_Variante_ListarPorProducto
    @IdProducto INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        V.IdVariante,
        V.IdProducto,
        V.Color,
        V.Talla,
        V.Modelo,
        V.Stock
    FROM dbo.TB_VARIANTE_PRODUCTO AS V
    WHERE V.IdProducto = @IdProducto
      AND V.Activo = 1
    ORDER BY V.IdVariante;
END;
GO

/* Destacados de la portada. Solo con stock: mostrar un destacado
   agotado en la primera pantalla es la peor vitrina posible. */
CREATE OR ALTER PROCEDURE dbo.SP_Producto_ListarDestacados
    @Cantidad INT = 4
AS
BEGIN
    SET NOCOUNT ON;

    IF @Cantidad IS NULL OR @Cantidad < 1 SET @Cantidad = 4;

    SELECT TOP (@Cantidad)
        P.IdProducto,
        P.Nombre AS NombreProducto,
        P.Precio,
        P.IdCategoria,
        C.Nombre AS NombreCategoria,
        (
            SELECT TOP (1) I.RutaImagen
            FROM dbo.TB_IMAGEN_PRODUCTO AS I
            WHERE I.IdProducto = P.IdProducto
              AND I.Activo = 1
            ORDER BY I.EsPrincipal DESC, I.IdImagen
        ) AS RutaImagen
    FROM dbo.TB_PRODUCTO AS P
    INNER JOIN dbo.TB_CATEGORIA AS C
        ON C.IdCategoria = P.IdCategoria
    WHERE P.Destacado = 1
      AND P.Activo = 1
      AND P.Stock > 0
    ORDER BY P.FechaCreacion DESC, P.IdProducto DESC;
END;
GO

/* ============================================================
   4. Favoritos

   Tabla puente sin reglas: el borrado es fisico (RG-97) y no deja
   bitacora. Que alguien quite un corazon no es un hecho auditable.
   ============================================================ */

/* Agregar es idempotente: si ya estaba, no revienta ni duplica. El
   indice unico (IdUsuario, IdProducto) es el que lo garantiza. */
CREATE OR ALTER PROCEDURE dbo.SP_Favorito_Agregar
    @IdUsuario  INT,
    @IdProducto INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.TB_PRODUCTO WHERE IdProducto = @IdProducto AND Activo = 1)
        THROW 50000, N'El producto indicado no existe.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.TB_FAVORITO
                   WHERE IdUsuario = @IdUsuario AND IdProducto = @IdProducto)
    BEGIN
        INSERT INTO dbo.TB_FAVORITO (IdUsuario, IdProducto)
        VALUES (@IdUsuario, @IdProducto);
    END;

    SELECT (SELECT COUNT(*) FROM dbo.TB_FAVORITO WHERE IdUsuario = @IdUsuario) AS TotalFavoritos;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_Favorito_Eliminar
    @IdUsuario  INT,
    @IdProducto INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.TB_FAVORITO
    WHERE IdUsuario = @IdUsuario AND IdProducto = @IdProducto;

    SELECT (SELECT COUNT(*) FROM dbo.TB_FAVORITO WHERE IdUsuario = @IdUsuario) AS TotalFavoritos;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_Favorito_ListarPorUsuario
    @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        P.IdProducto,
        P.Nombre AS NombreProducto,
        P.Precio,
        P.Stock,
        C.Nombre AS NombreCategoria,
        CASE WHEN P.Stock > 0 THEN N'Disponible' ELSE N'Agotado' END AS NombreEstado,
        (
            SELECT TOP (1) I.RutaImagen
            FROM dbo.TB_IMAGEN_PRODUCTO AS I
            WHERE I.IdProducto = P.IdProducto
              AND I.Activo = 1
            ORDER BY I.EsPrincipal DESC, I.IdImagen
        ) AS RutaImagen,
        F.FechaAgregado
    FROM dbo.TB_FAVORITO AS F
    INNER JOIN dbo.TB_PRODUCTO  AS P ON P.IdProducto  = F.IdProducto
    INNER JOIN dbo.TB_CATEGORIA AS C ON C.IdCategoria = P.IdCategoria
    WHERE F.IdUsuario = @IdUsuario
      AND P.Activo = 1
    ORDER BY F.FechaAgregado DESC;
END;
GO

/* ============================================================
   5. Carrito

   Un solo carrito activo por usuario, permanente: al confirmar la
   compra no se borra, se le vacian las lineas y se reutiliza. El
   indice unico filtrado UX_Carrito_UsuarioActivo es lo que garantiza
   que no haya dos.

   Las lineas del carrito no dejan bitacora a proposito: son datos
   volatiles sin valor historico. Lo que se audita es el pedido, que es
   el hecho que importa.
   ============================================================ */

/* Devuelve el carrito activo del usuario y lo crea si no tiene.

   El UPDLOCK con HOLDLOCK es necesario: sin el, dos peticiones
   simultaneas del mismo usuario (dos pestanias, o un doble clic)
   pueden pasar las dos por el IF y la segunda insercion revienta
   contra el indice unico. */
CREATE OR ALTER PROCEDURE dbo.SP_Carrito_ObtenerOCrear
    @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @IdCarrito INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT @IdCarrito = IdCarrito
        FROM dbo.TB_CARRITO WITH (UPDLOCK, HOLDLOCK)
        WHERE IdUsuario = @IdUsuario AND Activo = 1;

        IF @IdCarrito IS NULL
        BEGIN
            INSERT INTO dbo.TB_CARRITO (IdUsuario) VALUES (@IdUsuario);
            SET @IdCarrito = CAST(SCOPE_IDENTITY() AS INT);
        END;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;

    SELECT @IdCarrito AS IdCarrito;
END;
GO

/* Contenido del carrito. Disponible = 0 marca la linea que ya no se
   puede comprar, para poder avisarlo en la pantalla antes de que el
   cliente llegue al checkout y se lo rechacen. */
CREATE OR ALTER PROCEDURE dbo.SP_Carrito_Consultar
    @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        C.IdCarrito,
        P.IdProducto,
        P.Nombre AS NombreProducto,
        (
            SELECT TOP (1) I.RutaImagen
            FROM dbo.TB_IMAGEN_PRODUCTO AS I
            WHERE I.IdProducto = P.IdProducto
              AND I.Activo = 1
            ORDER BY I.EsPrincipal DESC, I.IdImagen
        ) AS RutaImagen,
        P.Precio AS PrecioUnitario,
        DC.Cantidad,
        CONVERT(DECIMAL(10,2), P.Precio * DC.Cantidad) AS SubtotalLinea,
        P.Stock AS StockDisponible,
        P.EsPiezaUnica,
        CAST(CASE WHEN P.Activo = 1 AND P.Stock >= DC.Cantidad THEN 1 ELSE 0 END AS BIT) AS Disponible
    FROM dbo.TB_CARRITO AS C
    INNER JOIN dbo.TB_DETALLE_CARRITO AS DC ON DC.IdCarrito  = C.IdCarrito
    INNER JOIN dbo.TB_PRODUCTO        AS P  ON P.IdProducto  = DC.IdProducto
    WHERE C.IdUsuario = @IdUsuario
      AND C.Activo = 1
    ORDER BY P.Nombre;
END;
GO

/* Agrega al carrito, o suma a la linea que ya existia.

   Recibe:   @Cantidad, las unidades que se agregan (no el total)
   Devuelve: CantidadItems, la suma de unidades del carrito, para el
             contador del encabezado
   Errores:  50000 si la cantidad no es valida, el producto no esta
             disponible, o lo pedido pasa del stock */
CREATE OR ALTER PROCEDURE dbo.SP_Carrito_AgregarProducto
    @IdUsuario  INT,
    @IdProducto INT,
    @Cantidad   INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @Cantidad IS NULL OR @Cantidad <= 0
        THROW 50000, N'La cantidad debe ser mayor que cero.', 1;

    DECLARE @Stock INT, @IdCarrito INT, @CantidadActual INT;

    SELECT @Stock = Stock
    FROM dbo.TB_PRODUCTO
    WHERE IdProducto = @IdProducto AND Activo = 1;

    IF @Stock IS NULL
        THROW 50000, N'El producto no está disponible.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT @IdCarrito = IdCarrito
        FROM dbo.TB_CARRITO WITH (UPDLOCK, HOLDLOCK)
        WHERE IdUsuario = @IdUsuario AND Activo = 1;

        IF @IdCarrito IS NULL
        BEGIN
            INSERT INTO dbo.TB_CARRITO (IdUsuario) VALUES (@IdUsuario);
            SET @IdCarrito = CAST(SCOPE_IDENTITY() AS INT);
        END;

        SELECT @CantidadActual = Cantidad
        FROM dbo.TB_DETALLE_CARRITO
        WHERE IdCarrito = @IdCarrito AND IdProducto = @IdProducto;

        IF ISNULL(@CantidadActual, 0) + @Cantidad > @Stock
        BEGIN
            DECLARE @Restante NVARCHAR(10) = CAST(@Stock - ISNULL(@CantidadActual, 0) AS NVARCHAR(10));
            DECLARE @MensajeStock NVARCHAR(200) =
                N'No hay stock suficiente. Disponible: ' + @Restante + N' unidad(es).';
            THROW 50000, @MensajeStock, 1;
        END;

        IF @CantidadActual IS NULL
        BEGIN
            INSERT INTO dbo.TB_DETALLE_CARRITO (IdCarrito, IdProducto, Cantidad)
            VALUES (@IdCarrito, @IdProducto, @Cantidad);
        END
        ELSE
        BEGIN
            UPDATE dbo.TB_DETALLE_CARRITO
            SET Cantidad = Cantidad + @Cantidad
            WHERE IdCarrito = @IdCarrito AND IdProducto = @IdProducto;
        END;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;

    SELECT ISNULL(SUM(Cantidad), 0) AS CantidadItems
    FROM dbo.TB_DETALLE_CARRITO
    WHERE IdCarrito = @IdCarrito;
END;
GO

/* Fija la cantidad de una linea. @Cantidad en cero o menos elimina la
   linea, que es lo que espera el control de cantidad de la pantalla del
   carrito cuando se baja de 1. */
CREATE OR ALTER PROCEDURE dbo.SP_Carrito_ActualizarCantidad
    @IdUsuario  INT,
    @IdProducto INT,
    @Cantidad   INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @IdCarrito INT, @Stock INT;

    SELECT @IdCarrito = IdCarrito
    FROM dbo.TB_CARRITO
    WHERE IdUsuario = @IdUsuario AND Activo = 1;

    IF @IdCarrito IS NULL
       OR NOT EXISTS (SELECT 1 FROM dbo.TB_DETALLE_CARRITO
                      WHERE IdCarrito = @IdCarrito AND IdProducto = @IdProducto)
        THROW 50000, N'El producto no está en el carrito.', 1;

    IF @Cantidad IS NULL OR @Cantidad <= 0
    BEGIN
        DELETE FROM dbo.TB_DETALLE_CARRITO
        WHERE IdCarrito = @IdCarrito AND IdProducto = @IdProducto;
    END
    ELSE
    BEGIN
        SELECT @Stock = Stock
        FROM dbo.TB_PRODUCTO
        WHERE IdProducto = @IdProducto AND Activo = 1;

        IF @Stock IS NULL
            THROW 50000, N'El producto no está disponible.', 1;

        IF @Cantidad > @Stock
        BEGIN
            DECLARE @MensajeStock NVARCHAR(200) =
                N'No hay stock suficiente. Disponible: ' + CAST(@Stock AS NVARCHAR(10)) + N' unidad(es).';
            THROW 50000, @MensajeStock, 1;
        END;

        UPDATE dbo.TB_DETALLE_CARRITO
        SET Cantidad = @Cantidad
        WHERE IdCarrito = @IdCarrito AND IdProducto = @IdProducto;
    END;

    SELECT ISNULL(SUM(Cantidad), 0) AS CantidadItems
    FROM dbo.TB_DETALLE_CARRITO
    WHERE IdCarrito = @IdCarrito;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_Carrito_EliminarProducto
    @IdUsuario  INT,
    @IdProducto INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @IdCarrito INT;

    SELECT @IdCarrito = IdCarrito
    FROM dbo.TB_CARRITO
    WHERE IdUsuario = @IdUsuario AND Activo = 1;

    IF @IdCarrito IS NULL
        THROW 50000, N'El producto no está en el carrito.', 1;

    DELETE FROM dbo.TB_DETALLE_CARRITO
    WHERE IdCarrito = @IdCarrito AND IdProducto = @IdProducto;

    IF @@ROWCOUNT = 0
        THROW 50000, N'El producto no está en el carrito.', 1;

    SELECT ISNULL(SUM(Cantidad), 0) AS CantidadItems
    FROM dbo.TB_DETALLE_CARRITO
    WHERE IdCarrito = @IdCarrito;
END;
GO

/* ============================================================
   6. Checkout y pedidos del cliente
   ============================================================ */

/* Metodos de pago activos, con la etiqueta de lo que hay que pedirle al
   cliente como comprobante simulado. */
CREATE OR ALTER PROCEDURE dbo.SP_MetodoPago_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT IdMetodoPago, Nombre, Descripcion, EtiquetaDatoPago
    FROM dbo.TB_METODO_PAGO
    WHERE Activo = 1
    ORDER BY IdMetodoPago;
END;
GO

/* Resumen previo a confirmar: las lineas del carrito validadas contra la
   direccion elegida. Si la direccion no es del usuario o esta dada de
   baja, no devuelve ninguna fila, y el checkout no puede continuar. */
CREATE OR ALTER PROCEDURE dbo.SP_Checkout_ConsultarResumen
    @IdUsuario   INT,
    @IdDireccion INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        C.IdCarrito,
        P.IdProducto,
        P.Nombre AS NombreProducto,
        (
            SELECT TOP (1) I.RutaImagen
            FROM dbo.TB_IMAGEN_PRODUCTO AS I
            WHERE I.IdProducto = P.IdProducto
              AND I.Activo = 1
            ORDER BY I.EsPrincipal DESC, I.IdImagen
        ) AS RutaImagen,
        P.Precio AS PrecioUnitario,
        DC.Cantidad,
        CONVERT(DECIMAL(10,2), P.Precio * DC.Cantidad) AS SubtotalLinea,
        P.Stock AS StockDisponible,
        CAST(CASE WHEN P.Activo = 1 AND P.Stock >= DC.Cantidad THEN 1 ELSE 0 END AS BIT) AS Disponible
    FROM dbo.TB_CARRITO AS C
    INNER JOIN dbo.TB_DETALLE_CARRITO AS DC ON DC.IdCarrito = C.IdCarrito
    INNER JOIN dbo.TB_PRODUCTO        AS P  ON P.IdProducto = DC.IdProducto
    WHERE C.IdUsuario = @IdUsuario
      AND C.Activo = 1
      AND EXISTS (SELECT 1
                  FROM dbo.TB_DIRECCION_ENVIO AS DE
                  WHERE DE.IdDireccion = @IdDireccion
                    AND DE.IdUsuario   = @IdUsuario
                    AND DE.Activo      = 1)
    ORDER BY P.Nombre;
END;
GO

/* ------------------------------------------------------------
   Confirmar la compra. Es la operacion mas delicada del sistema:
   toca seis tablas y tiene que quedar todo o nada.

   Que hace, en una sola transaccion:
     1. bloquea las filas de producto del carrito y revalida stock
     2. crea el pedido en estado Pagado
     3. copia las lineas del carrito al detalle, congelando el precio
     4. descuenta el stock
     5. registra el pago con su comprobante simulado
     6. emite la factura con numero FAC-000000
     7. vacia las lineas del carrito, que queda activo para reutilizarse
     8. deja bitacora

   El paso 1 es el que evita vender dos veces la misma pieza unica: sin
   el bloqueo, dos clientes que confirman al mismo tiempo pasan los dos
   la validacion de stock y el segundo deja el stock en negativo.

   La revalidacion es necesaria aunque el carrito ya se haya validado al
   agregar: entre que el cliente agrego y confirmo pudo pasar media hora
   y otra persona pudo comprarse la pieza.

   Recibe:
     @CostoEnvio        lo calcula la aplicacion segun la provincia
     @DatoPagoSimulado  lo que escribio el cliente segun el metodo:
                        ultimos digitos, referencia o telefono. Si viene
                        vacio se genera un comprobante SIM-000000.
   Devuelve: IdPedido y NumeroFactura
   Errores:  50000 si el costo de envio es invalido, la direccion o el
             metodo de pago no son validos, el carrito esta vacio, o
             algun producto se quedo sin stock
   ------------------------------------------------------------ */
CREATE OR ALTER PROCEDURE dbo.SP_Pedido_Confirmar
    @IdUsuario        INT,
    @IdDireccion      INT,
    @IdMetodoPago     INT,
    @CostoEnvio       DECIMAL(10,2),
    @DatoPagoSimulado NVARCHAR(150) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @CostoEnvio IS NULL OR @CostoEnvio < 0
        THROW 50000, N'El costo de envío no es válido.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.TB_DIRECCION_ENVIO
                   WHERE IdDireccion = @IdDireccion
                     AND IdUsuario   = @IdUsuario
                     AND Activo      = 1)
        THROW 50000, N'La dirección seleccionada no es válida.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.TB_METODO_PAGO
                   WHERE IdMetodoPago = @IdMetodoPago AND Activo = 1)
        THROW 50000, N'El método de pago seleccionado no es válido.', 1;

    DECLARE @IdCarrito INT;

    SELECT @IdCarrito = IdCarrito
    FROM dbo.TB_CARRITO
    WHERE IdUsuario = @IdUsuario AND Activo = 1;

    IF @IdCarrito IS NULL
       OR NOT EXISTS (SELECT 1 FROM dbo.TB_DETALLE_CARRITO WHERE IdCarrito = @IdCarrito)
        THROW 50000, N'El carrito está vacío.', 1;

    DECLARE @IdPedido INT, @NumeroFactura NVARCHAR(50);

    BEGIN TRY
        BEGIN TRANSACTION;

        /* Bloqueo de las filas de producto del carrito y revalidacion
           final de stock y disponibilidad */
        IF EXISTS
        (
            SELECT 1
            FROM dbo.TB_DETALLE_CARRITO AS DC
            INNER JOIN dbo.TB_PRODUCTO AS P WITH (UPDLOCK, ROWLOCK, HOLDLOCK)
                ON P.IdProducto = DC.IdProducto
            WHERE DC.IdCarrito = @IdCarrito
              AND (P.Stock < DC.Cantidad OR P.Activo = 0)
        )
            THROW 50000, N'Algunos productos ya no cuentan con stock suficiente. Por favor revise su carrito.', 1;

        DECLARE @Subtotal DECIMAL(10,2);

        SELECT @Subtotal = CONVERT(DECIMAL(10,2), SUM(P.Precio * DC.Cantidad))
        FROM dbo.TB_DETALLE_CARRITO AS DC
        INNER JOIN dbo.TB_PRODUCTO AS P ON P.IdProducto = DC.IdProducto
        WHERE DC.IdCarrito = @IdCarrito;

        DECLARE @Total DECIMAL(10,2) = @Subtotal + @CostoEnvio;

        /* 1 es Pagado: el pago es simulado y se confirma en el mismo acto */
        INSERT INTO dbo.TB_PEDIDO
            (IdUsuario, IdDireccion, IdEstadoPedido, Subtotal, CostoEnvio, Total)
        VALUES
            (@IdUsuario, @IdDireccion, 1, @Subtotal, @CostoEnvio, @Total);

        SET @IdPedido = CAST(SCOPE_IDENTITY() AS INT);

        /* El precio se copia, no se referencia: si manana sube, este
           pedido tiene que seguir valiendo lo que valia hoy */
        INSERT INTO dbo.TB_DETALLE_PEDIDO
            (IdPedido, IdProducto, Cantidad, PrecioUnitario, SubtotalLinea)
        SELECT
            @IdPedido,
            DC.IdProducto,
            DC.Cantidad,
            P.Precio,
            CONVERT(DECIMAL(10,2), P.Precio * DC.Cantidad)
        FROM dbo.TB_DETALLE_CARRITO AS DC
        INNER JOIN dbo.TB_PRODUCTO AS P ON P.IdProducto = DC.IdProducto
        WHERE DC.IdCarrito = @IdCarrito;

        UPDATE P
        SET P.Stock = P.Stock - DC.Cantidad
        FROM dbo.TB_PRODUCTO AS P
        INNER JOIN dbo.TB_DETALLE_CARRITO AS DC ON DC.IdProducto = P.IdProducto
        WHERE DC.IdCarrito = @IdCarrito;

        SET @NumeroFactura = N'FAC-' + RIGHT(N'000000' + CAST(@IdPedido AS NVARCHAR(10)), 6);

        DECLARE @Comprobante NVARCHAR(255) =
            ISNULL(NULLIF(LTRIM(RTRIM(@DatoPagoSimulado)), N''),
                   N'SIM-' + RIGHT(N'000000' + CAST(@IdPedido AS NVARCHAR(10)), 6));

        /* 2 es Pagado en TB_ESTADO_PAGO */
        INSERT INTO dbo.TB_PAGO (IdPedido, IdMetodoPago, IdEstadoPago, Monto, Comprobante)
        VALUES (@IdPedido, @IdMetodoPago, 2, @Total, @Comprobante);

        /* 1 es Emitida en TB_ESTADO_FACTURA */
        INSERT INTO dbo.TB_FACTURA
            (IdPedido, IdEstadoFactura, NumeroFactura, Subtotal, CostoEnvio, Total)
        VALUES
            (@IdPedido, 1, @NumeroFactura, @Subtotal, @CostoEnvio, @Total);

        /* El carrito queda vacio pero activo, para reutilizarse */
        DELETE FROM dbo.TB_DETALLE_CARRITO WHERE IdCarrito = @IdCarrito;

        INSERT INTO dbo.TB_BITACORA (IdUsuario, Entidad, IdRegistro, Accion, ValorNuevo)
        VALUES (@IdUsuario, N'TB_PEDIDO', @IdPedido, N'Confirmar compra',
                NULLIF((SELECT @Subtotal AS Subtotal, @CostoEnvio AS CostoEnvio, @Total AS Total,
                               @IdMetodoPago AS IdMetodoPago, @NumeroFactura AS NumeroFactura
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N''));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;

    SELECT @IdPedido AS IdPedido, @NumeroFactura AS NumeroFactura;
END;
GO

/* ------------------------------------------------------------
   Los tres de abajo devuelven una fila por producto con el encabezado
   del pedido repetido en cada una. Es la convencion del proyecto para
   los detalles: un solo resultset, mas facil de consumir desde el
   repositorio que dos consultas separadas.

   Todos piden @IdUsuario ademas del @IdPedido: asi un cliente no puede
   abrir el pedido de otro cambiando el numero en la direccion.

   EstadoVisible es el nombre que ve el cliente, mas suave que el
   interno: Pagado se muestra como Comprada, En preparacion como En
   camino y Entregado como Recibido.
   ------------------------------------------------------------ */

/* Pantalla de "gracias por su compra", despues de confirmar */
CREATE OR ALTER PROCEDURE dbo.SP_Pedido_ConsultarConfirmacion
    @IdPedido  INT,
    @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        PE.IdPedido,
        F.NumeroFactura,
        PE.FechaPedido,
        PE.Subtotal,
        PE.CostoEnvio,
        PE.Total,
        EP.Nombre AS NombreEstado,
        CASE PE.IdEstadoPedido
             WHEN 1 THEN N'Comprada'
             WHEN 2 THEN N'En camino'
             WHEN 3 THEN N'Recibido'
             ELSE EP.Nombre
        END AS EstadoVisible,
        MP.Nombre AS NombreMetodoPago,
        PG.Comprobante AS ComprobantePago,
        DE.NombreDestinatario,
        DE.DireccionExacta,
        DE.Referencia,
        DI.Nombre AS NombreDistrito,
        CA.Nombre AS NombreCanton,
        PR.Nombre AS NombreProvincia,
        P.IdProducto,
        P.Nombre AS NombreProducto,
        DP.Cantidad,
        DP.PrecioUnitario,
        DP.SubtotalLinea,
        (
            SELECT TOP (1) I.RutaImagen
            FROM dbo.TB_IMAGEN_PRODUCTO AS I
            WHERE I.IdProducto = P.IdProducto AND I.Activo = 1
            ORDER BY I.EsPrincipal DESC, I.IdImagen
        ) AS RutaImagen
    FROM dbo.TB_PEDIDO AS PE
    INNER JOIN dbo.TB_ESTADO_PEDIDO   AS EP ON EP.IdEstadoPedido = PE.IdEstadoPedido
    INNER JOIN dbo.TB_DETALLE_PEDIDO  AS DP ON DP.IdPedido       = PE.IdPedido
    INNER JOIN dbo.TB_PRODUCTO        AS P  ON P.IdProducto      = DP.IdProducto
    INNER JOIN dbo.TB_DIRECCION_ENVIO AS DE ON DE.IdDireccion    = PE.IdDireccion
    INNER JOIN dbo.TB_DISTRITO        AS DI ON DI.IdDistrito     = DE.IdDistrito
    INNER JOIN dbo.TB_CANTON          AS CA ON CA.IdCanton       = DI.IdCanton
    INNER JOIN dbo.TB_PROVINCIA       AS PR ON PR.IdProvincia    = CA.IdProvincia
    LEFT  JOIN dbo.TB_FACTURA         AS F  ON F.IdPedido        = PE.IdPedido
    LEFT  JOIN dbo.TB_PAGO            AS PG ON PG.IdPedido       = PE.IdPedido
    LEFT  JOIN dbo.TB_METODO_PAGO     AS MP ON MP.IdMetodoPago   = PG.IdMetodoPago
    WHERE PE.IdPedido  = @IdPedido
      AND PE.IdUsuario = @IdUsuario
    ORDER BY P.Nombre;
END;
GO

/* Historial paginado del cliente */
CREATE OR ALTER PROCEDURE dbo.SP_Pedido_ListarPorUsuario
    @IdUsuario    INT,
    @Pagina       INT = 1,
    @TamanoPagina INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @Pagina IS NULL OR @Pagina < 1 SET @Pagina = 1;
    IF @TamanoPagina IS NULL OR @TamanoPagina < 1 SET @TamanoPagina = 10;

    SELECT
        PE.IdPedido,
        F.NumeroFactura,
        PE.FechaPedido,
        PE.Total,
        PE.IdEstadoPedido,
        EP.Nombre AS NombreEstado,
        EP.ClaseCss AS ClaseCssEstado,
        CASE PE.IdEstadoPedido
             WHEN 1 THEN N'Comprada'
             WHEN 2 THEN N'En camino'
             WHEN 3 THEN N'Recibido'
             ELSE EP.Nombre
        END AS EstadoVisible,
        (
            SELECT ISNULL(SUM(DP.Cantidad), 0)
            FROM dbo.TB_DETALLE_PEDIDO AS DP
            WHERE DP.IdPedido = PE.IdPedido
        ) AS CantidadProductos,
        COUNT(*) OVER () AS TotalFilas
    FROM dbo.TB_PEDIDO AS PE
    INNER JOIN dbo.TB_ESTADO_PEDIDO AS EP ON EP.IdEstadoPedido = PE.IdEstadoPedido
    LEFT  JOIN dbo.TB_FACTURA       AS F  ON F.IdPedido        = PE.IdPedido
    WHERE PE.IdUsuario = @IdUsuario
    ORDER BY PE.FechaPedido DESC, PE.IdPedido DESC
    OFFSET (@Pagina - 1) * @TamanoPagina ROWS
    FETCH NEXT @TamanoPagina ROWS ONLY;
END;
GO

/* Detalle del pedido tal como lo ve el cliente */
CREATE OR ALTER PROCEDURE dbo.SP_Pedido_ObtenerDetalle
    @IdPedido  INT,
    @IdUsuario INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        PE.IdPedido,
        F.NumeroFactura,
        PE.FechaPedido,
        PE.Subtotal,
        PE.CostoEnvio,
        PE.Total,
        PE.IdEstadoPedido,
        EP.Nombre AS NombreEstado,
        EP.ClaseCss AS ClaseCssEstado,
        CASE PE.IdEstadoPedido
             WHEN 1 THEN N'Comprada'
             WHEN 2 THEN N'En camino'
             WHEN 3 THEN N'Recibido'
             ELSE EP.Nombre
        END AS EstadoVisible,
        PE.MotivoCancelacion,
        PG.FechaPago,
        MP.Nombre  AS NombreMetodoPago,
        PG.Comprobante AS ComprobantePago,
        EPG.Nombre AS EstadoPago,
        DE.NombreDestinatario,
        DE.TelefonoContacto,
        DE.DireccionExacta,
        DE.Referencia,
        DI.Nombre AS NombreDistrito,
        CA.Nombre AS NombreCanton,
        PR.Nombre AS NombreProvincia,
        P.IdProducto,
        P.Nombre AS NombreProducto,
        DP.Cantidad,
        DP.PrecioUnitario,
        DP.SubtotalLinea,
        (
            SELECT TOP (1) I.RutaImagen
            FROM dbo.TB_IMAGEN_PRODUCTO AS I
            WHERE I.IdProducto = P.IdProducto AND I.Activo = 1
            ORDER BY I.EsPrincipal DESC, I.IdImagen
        ) AS RutaImagen
    FROM dbo.TB_PEDIDO AS PE
    INNER JOIN dbo.TB_ESTADO_PEDIDO   AS EP  ON EP.IdEstadoPedido = PE.IdEstadoPedido
    INNER JOIN dbo.TB_DETALLE_PEDIDO  AS DP  ON DP.IdPedido       = PE.IdPedido
    INNER JOIN dbo.TB_PRODUCTO        AS P   ON P.IdProducto      = DP.IdProducto
    INNER JOIN dbo.TB_DIRECCION_ENVIO AS DE  ON DE.IdDireccion    = PE.IdDireccion
    INNER JOIN dbo.TB_DISTRITO        AS DI  ON DI.IdDistrito     = DE.IdDistrito
    INNER JOIN dbo.TB_CANTON          AS CA  ON CA.IdCanton       = DI.IdCanton
    INNER JOIN dbo.TB_PROVINCIA       AS PR  ON PR.IdProvincia    = CA.IdProvincia
    LEFT  JOIN dbo.TB_FACTURA         AS F   ON F.IdPedido        = PE.IdPedido
    LEFT  JOIN dbo.TB_PAGO            AS PG  ON PG.IdPedido       = PE.IdPedido
    LEFT  JOIN dbo.TB_METODO_PAGO     AS MP  ON MP.IdMetodoPago   = PG.IdMetodoPago
    LEFT  JOIN dbo.TB_ESTADO_PAGO     AS EPG ON EPG.IdEstadoPago  = PG.IdEstadoPago
    WHERE PE.IdPedido  = @IdPedido
      AND PE.IdUsuario = @IdUsuario
    ORDER BY P.Nombre;
END;
GO

/* ============================================================
   7. Administracion de categorias, productos e imagenes
   ============================================================ */

/* Todas las categorias, activas e inactivas, con el conteo de productos
   que cuelgan de cada una. El conteo es lo que permite avisar en la
   pantalla por que una categoria no se puede desactivar. */
CREATE OR ALTER PROCEDURE dbo.SP_Categoria_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        C.IdCategoria,
        C.Nombre,
        C.Descripcion,
        C.Activo,
        (
            SELECT COUNT(*)
            FROM dbo.TB_PRODUCTO AS P
            WHERE P.IdCategoria = C.IdCategoria
        ) AS CantidadProductos,
        (
            SELECT COUNT(*)
            FROM dbo.TB_PRODUCTO AS P
            WHERE P.IdCategoria = C.IdCategoria AND P.Activo = 1
        ) AS CantidadProductosActivos
    FROM dbo.TB_CATEGORIA AS C
    ORDER BY C.Nombre;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_Categoria_Registrar
    @IdUsuarioAccion INT,
    @Nombre          NVARCHAR(100),
    @Descripcion     NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @Nombre      = LTRIM(RTRIM(@Nombre));
    SET @Descripcion = NULLIF(LTRIM(RTRIM(@Descripcion)), N'');

    IF @Nombre = N''
        THROW 50000, N'El nombre de la categoría es obligatorio.', 1;

    /* La comparacion es insensible a mayusculas y acentos por la
       intercalacion de la base, asi que "Collares" y "collares" chocan */
    IF EXISTS (SELECT 1 FROM dbo.TB_CATEGORIA WHERE Nombre = @Nombre)
        THROW 50000, N'Ya existe una categoría con ese nombre.', 1;

    DECLARE @IdCategoria INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.TB_CATEGORIA (Nombre, Descripcion)
        VALUES (@Nombre, @Descripcion);

        SET @IdCategoria = CAST(SCOPE_IDENTITY() AS INT);

        INSERT INTO dbo.TB_BITACORA (IdUsuario, Entidad, IdRegistro, Accion, ValorNuevo)
        VALUES (@IdUsuarioAccion, N'TB_CATEGORIA', @IdCategoria, N'Crear',
                NULLIF((SELECT @Nombre AS Nombre, @Descripcion AS Descripcion
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N''));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;

    SELECT @IdCategoria AS IdCategoria;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_Categoria_Actualizar
    @IdUsuarioAccion INT,
    @IdCategoria     INT,
    @Nombre          NVARCHAR(100),
    @Descripcion     NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @Nombre      = LTRIM(RTRIM(@Nombre));
    SET @Descripcion = NULLIF(LTRIM(RTRIM(@Descripcion)), N'');

    IF NOT EXISTS (SELECT 1 FROM dbo.TB_CATEGORIA WHERE IdCategoria = @IdCategoria)
        THROW 50000, N'La categoría indicada no existe.', 1;

    IF @Nombre = N''
        THROW 50000, N'El nombre de la categoría es obligatorio.', 1;

    IF EXISTS (SELECT 1 FROM dbo.TB_CATEGORIA
               WHERE Nombre = @Nombre AND IdCategoria <> @IdCategoria)
        THROW 50000, N'Ya existe otra categoría con ese nombre.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @Anterior NVARCHAR(MAX) =
            NULLIF((SELECT Nombre, Descripcion FROM dbo.TB_CATEGORIA
                    WHERE IdCategoria = @IdCategoria
                    FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N'');

        UPDATE dbo.TB_CATEGORIA
        SET Nombre = @Nombre, Descripcion = @Descripcion
        WHERE IdCategoria = @IdCategoria;

        INSERT INTO dbo.TB_BITACORA (IdUsuario, Entidad, IdRegistro, Accion, ValorAnterior, ValorNuevo)
        VALUES (@IdUsuarioAccion, N'TB_CATEGORIA', @IdCategoria, N'Actualizar', @Anterior,
                NULLIF((SELECT @Nombre AS Nombre, @Descripcion AS Descripcion
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N''));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

/* Alterna activo e inactivo.

   No deja desactivar una categoria que todavia tiene productos activos:
   esos productos quedarian colgando de una categoria que no se ve, y en
   el catalogo aparecerian sin rama. Primero hay que desactivar o mover
   los productos. */
CREATE OR ALTER PROCEDURE dbo.SP_Categoria_CambiarActivo
    @IdUsuarioAccion INT,
    @IdCategoria     INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @ActivoActual BIT;

    SELECT @ActivoActual = Activo
    FROM dbo.TB_CATEGORIA
    WHERE IdCategoria = @IdCategoria;

    IF @ActivoActual IS NULL
        THROW 50000, N'La categoría indicada no existe.', 1;

    IF @ActivoActual = 1
       AND EXISTS (SELECT 1 FROM dbo.TB_PRODUCTO
                   WHERE IdCategoria = @IdCategoria AND Activo = 1)
        THROW 50000, N'No es posible desactivar la categoría porque tiene productos activos asociados.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.TB_CATEGORIA
        SET Activo = CASE WHEN @ActivoActual = 1 THEN 0 ELSE 1 END
        WHERE IdCategoria = @IdCategoria;

        INSERT INTO dbo.TB_BITACORA (IdUsuario, Entidad, IdRegistro, Accion, ValorAnterior, ValorNuevo)
        VALUES (@IdUsuarioAccion, N'TB_CATEGORIA', @IdCategoria,
                CASE WHEN @ActivoActual = 1 THEN N'Desactivar' ELSE N'Activar' END,
                NULLIF((SELECT @ActivoActual AS Activo FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N''),
                NULLIF((SELECT CASE WHEN @ActivoActual = 1 THEN 0 ELSE 1 END AS Activo
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N''));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

/* Listado del panel de productos. Trae activos e inactivos, a
   diferencia del catalogo, y con paginacion. */
CREATE OR ALTER PROCEDURE dbo.SP_Producto_Listar
    @Busqueda     NVARCHAR(150) = NULL,
    @IdCategoria  INT = NULL,
    @SoloActivos  BIT = 0,
    @Pagina       INT = 1,
    @TamanoPagina INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @Pagina IS NULL OR @Pagina < 1 SET @Pagina = 1;
    IF @TamanoPagina IS NULL OR @TamanoPagina < 1 SET @TamanoPagina = 10;

    SET @Busqueda = NULLIF(LTRIM(RTRIM(@Busqueda)), N'');

    SELECT
        P.IdProducto,
        P.Nombre AS NombreProducto,
        P.Precio,
        P.Stock,
        P.EsPiezaUnica,
        P.Destacado,
        P.Activo,
        P.IdCategoria,
        C.Nombre AS NombreCategoria,
        CASE WHEN P.Activo = 0 THEN N'Inactivo'
             WHEN P.Stock > 0 THEN N'Disponible'
             ELSE N'Agotado'
        END AS NombreEstado,
        P.FechaCreacion,
        (
            SELECT TOP (1) I.RutaImagen
            FROM dbo.TB_IMAGEN_PRODUCTO AS I
            WHERE I.IdProducto = P.IdProducto AND I.Activo = 1
            ORDER BY I.EsPrincipal DESC, I.IdImagen
        ) AS RutaImagen,
        (
            SELECT COUNT(*)
            FROM dbo.TB_IMAGEN_PRODUCTO AS I
            WHERE I.IdProducto = P.IdProducto AND I.Activo = 1
        ) AS CantidadImagenes,
        COUNT(*) OVER () AS TotalFilas
    FROM dbo.TB_PRODUCTO AS P
    INNER JOIN dbo.TB_CATEGORIA AS C ON C.IdCategoria = P.IdCategoria
    WHERE (@Busqueda IS NULL OR P.Nombre LIKE N'%' + @Busqueda + N'%')
      AND (@IdCategoria IS NULL OR P.IdCategoria = @IdCategoria)
      AND (@SoloActivos = 0 OR P.Activo = 1)
    ORDER BY P.FechaCreacion DESC, P.IdProducto DESC
    OFFSET (@Pagina - 1) * @TamanoPagina ROWS
    FETCH NEXT @TamanoPagina ROWS ONLY;
END;
GO

/* Un producto por Id para llenar el formulario de edicion. A diferencia
   de SP_Producto_ObtenerDetalle, este si devuelve los inactivos: el
   administrador tiene que poder editar lo que dio de baja. */
CREATE OR ALTER PROCEDURE dbo.SP_Producto_ObtenerPorId
    @IdProducto INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        P.IdProducto,
        P.Nombre AS NombreProducto,
        P.Descripcion,
        P.Precio,
        P.Stock,
        P.EsPiezaUnica,
        P.Destacado,
        P.Activo,
        P.IdCategoria,
        C.Nombre AS NombreCategoria,
        P.FechaCreacion
    FROM dbo.TB_PRODUCTO AS P
    INNER JOIN dbo.TB_CATEGORIA AS C ON C.IdCategoria = P.IdCategoria
    WHERE P.IdProducto = @IdProducto;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_Producto_Registrar
    @IdUsuarioAccion INT,
    @Nombre          NVARCHAR(150),
    @Descripcion     NVARCHAR(MAX) = NULL,
    @Precio          DECIMAL(10,2),
    @Stock           INT,
    @EsPiezaUnica    BIT = 0,
    @Destacado       BIT = 0,
    @IdCategoria     INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @Nombre      = LTRIM(RTRIM(@Nombre));
    SET @Descripcion = NULLIF(LTRIM(RTRIM(@Descripcion)), N'');

    IF @Nombre = N''
        THROW 50000, N'El nombre del producto es obligatorio.', 1;

    IF @Precio IS NULL OR @Precio < 0
        THROW 50000, N'El precio debe ser mayor o igual a cero.', 1;

    IF @Stock IS NULL OR @Stock < 0
        THROW 50000, N'El stock debe ser mayor o igual a cero.', 1;

    IF @EsPiezaUnica = 1 AND @Stock > 1
        THROW 50000, N'Una pieza única no puede tener stock mayor a 1.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.TB_CATEGORIA
                   WHERE IdCategoria = @IdCategoria AND Activo = 1)
        THROW 50000, N'La categoría seleccionada no es válida.', 1;

    DECLARE @IdProducto INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.TB_PRODUCTO
            (IdCategoria, Nombre, Descripcion, Precio, Stock, EsPiezaUnica, Destacado)
        VALUES
            (@IdCategoria, @Nombre, @Descripcion, @Precio, @Stock, @EsPiezaUnica, @Destacado);

        SET @IdProducto = CAST(SCOPE_IDENTITY() AS INT);

        INSERT INTO dbo.TB_BITACORA (IdUsuario, Entidad, IdRegistro, Accion, ValorNuevo)
        VALUES (@IdUsuarioAccion, N'TB_PRODUCTO', @IdProducto, N'Crear',
                NULLIF((SELECT IdCategoria, Nombre, Precio, Stock, EsPiezaUnica, Destacado
                        FROM dbo.TB_PRODUCTO WHERE IdProducto = @IdProducto
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N''));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;

    SELECT @IdProducto AS IdProducto;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_Producto_Actualizar
    @IdUsuarioAccion INT,
    @IdProducto      INT,
    @Nombre          NVARCHAR(150),
    @Descripcion     NVARCHAR(MAX) = NULL,
    @Precio          DECIMAL(10,2),
    @Stock           INT,
    @EsPiezaUnica    BIT,
    @Destacado       BIT,
    @IdCategoria     INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @Nombre      = LTRIM(RTRIM(@Nombre));
    SET @Descripcion = NULLIF(LTRIM(RTRIM(@Descripcion)), N'');

    IF NOT EXISTS (SELECT 1 FROM dbo.TB_PRODUCTO WHERE IdProducto = @IdProducto)
        THROW 50000, N'El producto indicado no existe.', 1;

    IF @Nombre = N''
        THROW 50000, N'El nombre del producto es obligatorio.', 1;

    IF @Precio IS NULL OR @Precio < 0 OR @Stock IS NULL OR @Stock < 0
        THROW 50000, N'El precio y el stock deben ser mayores o iguales a cero.', 1;

    IF @EsPiezaUnica = 1 AND @Stock > 1
        THROW 50000, N'Una pieza única no puede tener stock mayor a 1.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.TB_CATEGORIA
                   WHERE IdCategoria = @IdCategoria AND Activo = 1)
        THROW 50000, N'La categoría seleccionada no es válida.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @Anterior NVARCHAR(MAX) =
            NULLIF((SELECT IdCategoria, Nombre, Precio, Stock, EsPiezaUnica, Destacado
                    FROM dbo.TB_PRODUCTO WHERE IdProducto = @IdProducto
                    FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N'');

        UPDATE dbo.TB_PRODUCTO
        SET IdCategoria  = @IdCategoria,
            Nombre       = @Nombre,
            Descripcion  = @Descripcion,
            Precio       = @Precio,
            Stock        = @Stock,
            EsPiezaUnica = @EsPiezaUnica,
            Destacado    = @Destacado
        WHERE IdProducto = @IdProducto;

        INSERT INTO dbo.TB_BITACORA (IdUsuario, Entidad, IdRegistro, Accion, ValorAnterior, ValorNuevo)
        VALUES (@IdUsuarioAccion, N'TB_PRODUCTO', @IdProducto, N'Actualizar', @Anterior,
                NULLIF((SELECT IdCategoria, Nombre, Precio, Stock, EsPiezaUnica, Destacado
                        FROM dbo.TB_PRODUCTO WHERE IdProducto = @IdProducto
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N''));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

/* Baja y alta logica del producto. Al desactivarlo desaparece del
   catalogo, pero sigue en los pedidos viejos con su nombre y su precio
   congelados. */
CREATE OR ALTER PROCEDURE dbo.SP_Producto_CambiarActivo
    @IdUsuarioAccion INT,
    @IdProducto      INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @ActivoActual BIT;

    SELECT @ActivoActual = Activo
    FROM dbo.TB_PRODUCTO
    WHERE IdProducto = @IdProducto;

    IF @ActivoActual IS NULL
        THROW 50000, N'El producto indicado no existe.', 1;

    /* Reactivar un producto cuya categoria esta inactiva lo dejaria
       fuera del catalogo igual, y sin explicacion visible */
    IF @ActivoActual = 0
       AND NOT EXISTS (SELECT 1 FROM dbo.TB_CATEGORIA AS C
                       INNER JOIN dbo.TB_PRODUCTO AS P ON P.IdCategoria = C.IdCategoria
                       WHERE P.IdProducto = @IdProducto AND C.Activo = 1)
        THROW 50000, N'No es posible activar el producto porque su categoría está inactiva.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.TB_PRODUCTO
        SET Activo = CASE WHEN @ActivoActual = 1 THEN 0 ELSE 1 END
        WHERE IdProducto = @IdProducto;

        INSERT INTO dbo.TB_BITACORA (IdUsuario, Entidad, IdRegistro, Accion, ValorAnterior, ValorNuevo)
        VALUES (@IdUsuarioAccion, N'TB_PRODUCTO', @IdProducto,
                CASE WHEN @ActivoActual = 1 THEN N'Desactivar' ELSE N'Activar' END,
                NULLIF((SELECT @ActivoActual AS Activo FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N''),
                NULLIF((SELECT CASE WHEN @ActivoActual = 1 THEN 0 ELSE 1 END AS Activo
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N''));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

/* Ajuste de inventario desde la pantalla de existencias. Es aparte de
   SP_Producto_Actualizar porque se usa desde una tabla, campo por campo,
   sin pasar por el formulario completo del producto. */
CREATE OR ALTER PROCEDURE dbo.SP_Producto_ActualizarStock
    @IdUsuarioAccion INT,
    @IdProducto      INT,
    @Stock           INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @EsPiezaUnica BIT, @StockAnterior INT;

    SELECT @EsPiezaUnica = EsPiezaUnica, @StockAnterior = Stock
    FROM dbo.TB_PRODUCTO
    WHERE IdProducto = @IdProducto;

    IF @EsPiezaUnica IS NULL
        THROW 50000, N'El producto indicado no existe.', 1;

    IF @Stock IS NULL OR @Stock < 0
        THROW 50000, N'El stock debe ser mayor o igual a cero.', 1;

    IF @EsPiezaUnica = 1 AND @Stock > 1
        THROW 50000, N'Una pieza única no puede tener stock mayor a 1.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.TB_PRODUCTO
        SET Stock = @Stock
        WHERE IdProducto = @IdProducto;

        INSERT INTO dbo.TB_BITACORA (IdUsuario, Entidad, IdRegistro, Accion, ValorAnterior, ValorNuevo)
        VALUES (@IdUsuarioAccion, N'TB_PRODUCTO', @IdProducto, N'Ajustar inventario',
                NULLIF((SELECT @StockAnterior AS Stock FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N''),
                NULLIF((SELECT @Stock AS Stock FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N''));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

/* Alerta de existencias. Bajo stock es una consulta, no un estado
   guardado: se calcula contra el umbral que manda la pantalla. */
CREATE OR ALTER PROCEDURE dbo.SP_Producto_ListarBajoStock
    @StockMinimo INT = 3
AS
BEGIN
    SET NOCOUNT ON;

    IF @StockMinimo IS NULL OR @StockMinimo < 0 SET @StockMinimo = 3;

    SELECT
        P.IdProducto,
        P.Nombre AS NombreProducto,
        P.Stock,
        P.EsPiezaUnica,
        P.IdCategoria,
        C.Nombre AS NombreCategoria,
        CASE WHEN P.Stock > 0 THEN N'Disponible' ELSE N'Agotado' END AS NombreEstado
    FROM dbo.TB_PRODUCTO AS P
    INNER JOIN dbo.TB_CATEGORIA AS C ON C.IdCategoria = P.IdCategoria
    WHERE P.Activo = 1
      AND P.Stock <= @StockMinimo
    ORDER BY P.Stock, P.Nombre;
END;
GO

/* Imagenes de un producto para el panel. Es igual que
   SP_Catalogo_ListarImagenes pero se deja aparte porque el panel puede
   necesitar despues mostrar tambien las dadas de baja. */
CREATE OR ALTER PROCEDURE dbo.SP_ImagenProducto_ListarPorProducto
    @IdProducto INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        I.IdImagen,
        I.IdProducto,
        I.RutaImagen,
        I.TextoAlternativo,
        I.EsPrincipal
    FROM dbo.TB_IMAGEN_PRODUCTO AS I
    WHERE I.IdProducto = @IdProducto
      AND I.Activo = 1
    ORDER BY I.EsPrincipal DESC, I.IdImagen;
END;
GO

/* Agrega una imagen. La primera del producto queda principal sola: si
   no, el catalogo mostraria una tarjeta sin imagen hasta que alguien se
   acuerde de marcarla. */
CREATE OR ALTER PROCEDURE dbo.SP_ImagenProducto_Registrar
    @IdUsuarioAccion  INT,
    @IdProducto       INT,
    @RutaImagen       NVARCHAR(255),
    @TextoAlternativo NVARCHAR(150) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @RutaImagen       = LTRIM(RTRIM(@RutaImagen));
    SET @TextoAlternativo = NULLIF(LTRIM(RTRIM(@TextoAlternativo)), N'');

    IF NOT EXISTS (SELECT 1 FROM dbo.TB_PRODUCTO WHERE IdProducto = @IdProducto)
        THROW 50000, N'El producto indicado no existe.', 1;

    IF @RutaImagen = N''
        THROW 50000, N'La ruta de la imagen es obligatoria.', 1;

    DECLARE @IdImagen INT, @EsPrincipal BIT;

    BEGIN TRY
        BEGIN TRANSACTION;

        SET @EsPrincipal =
            CASE WHEN EXISTS (SELECT 1 FROM dbo.TB_IMAGEN_PRODUCTO
                              WHERE IdProducto = @IdProducto
                                AND EsPrincipal = 1
                                AND Activo = 1)
                 THEN 0 ELSE 1 END;

        INSERT INTO dbo.TB_IMAGEN_PRODUCTO (IdProducto, RutaImagen, TextoAlternativo, EsPrincipal)
        VALUES (@IdProducto, @RutaImagen, @TextoAlternativo, @EsPrincipal);

        SET @IdImagen = CAST(SCOPE_IDENTITY() AS INT);

        INSERT INTO dbo.TB_BITACORA (IdUsuario, Entidad, IdRegistro, Accion, ValorNuevo)
        VALUES (@IdUsuarioAccion, N'TB_IMAGEN_PRODUCTO', @IdImagen, N'Crear',
                NULLIF((SELECT @IdProducto AS IdProducto, @RutaImagen AS RutaImagen,
                               @EsPrincipal AS EsPrincipal
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N''));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;

    SELECT @IdImagen AS IdImagen, @EsPrincipal AS EsPrincipal;
END;
GO

/* Cambia cual es la imagen principal. Primero se le quita la marca a la
   que la tenia y despues se pone: el indice unico filtrado del esquema
   no admite dos principales activas y el orden importa. */
CREATE OR ALTER PROCEDURE dbo.SP_ImagenProducto_MarcarPrincipal
    @IdUsuarioAccion INT,
    @IdImagen        INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @IdProducto INT;

    SELECT @IdProducto = IdProducto
    FROM dbo.TB_IMAGEN_PRODUCTO
    WHERE IdImagen = @IdImagen AND Activo = 1;

    IF @IdProducto IS NULL
        THROW 50000, N'La imagen indicada no existe o está dada de baja.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.TB_IMAGEN_PRODUCTO
        SET EsPrincipal = 0
        WHERE IdProducto = @IdProducto AND EsPrincipal = 1;

        UPDATE dbo.TB_IMAGEN_PRODUCTO
        SET EsPrincipal = 1
        WHERE IdImagen = @IdImagen;

        INSERT INTO dbo.TB_BITACORA (IdUsuario, Entidad, IdRegistro, Accion, ValorNuevo)
        VALUES (@IdUsuarioAccion, N'TB_IMAGEN_PRODUCTO', @IdImagen, N'Marcar principal',
                NULLIF((SELECT @IdProducto AS IdProducto, 1 AS EsPrincipal
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N''));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

/* Baja logica de la imagen. Si la que se quita era la principal se
   promueve otra activa, para que el producto no quede sin imagen de
   portada. */
CREATE OR ALTER PROCEDURE dbo.SP_ImagenProducto_Desactivar
    @IdUsuarioAccion INT,
    @IdImagen        INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @IdProducto INT, @EraPrincipal BIT;

    SELECT @IdProducto = IdProducto, @EraPrincipal = EsPrincipal
    FROM dbo.TB_IMAGEN_PRODUCTO
    WHERE IdImagen = @IdImagen AND Activo = 1;

    IF @IdProducto IS NULL
        THROW 50000, N'La imagen indicada no existe o ya está dada de baja.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.TB_IMAGEN_PRODUCTO
        SET Activo = 0, EsPrincipal = 0
        WHERE IdImagen = @IdImagen;

        IF @EraPrincipal = 1
        BEGIN
            UPDATE dbo.TB_IMAGEN_PRODUCTO
            SET EsPrincipal = 1
            WHERE IdImagen =
            (
                SELECT TOP (1) IdImagen
                FROM dbo.TB_IMAGEN_PRODUCTO
                WHERE IdProducto = @IdProducto AND Activo = 1
                ORDER BY IdImagen
            );
        END;

        INSERT INTO dbo.TB_BITACORA (IdUsuario, Entidad, IdRegistro, Accion)
        VALUES (@IdUsuarioAccion, N'TB_IMAGEN_PRODUCTO', @IdImagen, N'Desactivar');

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

/* ============================================================
   8. Administracion de pedidos, pagos y facturas
   ============================================================ */

/* Estados a los que se puede mover un pedido, para el campo de
   seleccion de la pantalla de pedidos. */
CREATE OR ALTER PROCEDURE dbo.SP_EstadoPedido_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT IdEstadoPedido, Nombre, Orden, ClaseCss
    FROM dbo.TB_ESTADO_PEDIDO
    ORDER BY Orden;
END;
GO

/* Bandeja de pedidos del panel, con busqueda por cliente o numero de
   factura, filtro por estado y por rango de fechas, y paginacion.

   Ejemplo:
     EXEC dbo.SP_Pedido_Listar @IdEstadoPedido = 1, @Pagina = 1,
                               @TamanoPagina = 10; */
CREATE OR ALTER PROCEDURE dbo.SP_Pedido_Listar
    @Busqueda       NVARCHAR(150) = NULL,
    @IdEstadoPedido INT = NULL,
    @FechaInicio    DATE = NULL,
    @FechaFin       DATE = NULL,
    @Pagina         INT = 1,
    @TamanoPagina   INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @Pagina IS NULL OR @Pagina < 1 SET @Pagina = 1;
    IF @TamanoPagina IS NULL OR @TamanoPagina < 1 SET @TamanoPagina = 10;

    SET @Busqueda = NULLIF(LTRIM(RTRIM(@Busqueda)), N'');

    SELECT
        PE.IdPedido,
        F.NumeroFactura,
        PE.FechaPedido,
        PE.Subtotal,
        PE.CostoEnvio,
        PE.Total,
        PE.IdEstadoPedido,
        EP.Nombre   AS NombreEstado,
        EP.ClaseCss AS ClaseCssEstado,
        U.IdUsuario,
        U.Nombre + N' ' + U.Apellido AS NombreCliente,
        U.Correo AS CorreoCliente,
        MP.Nombre AS NombreMetodoPago,
        (
            SELECT ISNULL(SUM(DP.Cantidad), 0)
            FROM dbo.TB_DETALLE_PEDIDO AS DP
            WHERE DP.IdPedido = PE.IdPedido
        ) AS CantidadProductos,
        COUNT(*) OVER () AS TotalFilas
    FROM dbo.TB_PEDIDO AS PE
    INNER JOIN dbo.TB_USUARIO       AS U  ON U.IdUsuario       = PE.IdUsuario
    INNER JOIN dbo.TB_ESTADO_PEDIDO AS EP ON EP.IdEstadoPedido = PE.IdEstadoPedido
    LEFT  JOIN dbo.TB_FACTURA       AS F  ON F.IdPedido        = PE.IdPedido
    LEFT  JOIN dbo.TB_PAGO          AS PG ON PG.IdPedido       = PE.IdPedido
    LEFT  JOIN dbo.TB_METODO_PAGO   AS MP ON MP.IdMetodoPago   = PG.IdMetodoPago
    WHERE (@Busqueda IS NULL
           OR U.Nombre + N' ' + U.Apellido LIKE N'%' + @Busqueda + N'%'
           OR U.Correo LIKE N'%' + @Busqueda + N'%'
           OR F.NumeroFactura LIKE N'%' + @Busqueda + N'%')
      AND (@IdEstadoPedido IS NULL OR PE.IdEstadoPedido = @IdEstadoPedido)
      AND (@FechaInicio IS NULL OR PE.FechaPedido >= @FechaInicio)
      AND (@FechaFin IS NULL OR PE.FechaPedido < DATEADD(DAY, 1, @FechaFin))
    ORDER BY PE.FechaPedido DESC, PE.IdPedido DESC
    OFFSET (@Pagina - 1) * @TamanoPagina ROWS
    FETCH NEXT @TamanoPagina ROWS ONLY;
END;
GO

/* Detalle del pedido para el panel. A diferencia del que ve el cliente,
   este no pide @IdUsuario: el administrador ve cualquiera. Trae los
   datos del cliente y el motivo de cancelacion. */
CREATE OR ALTER PROCEDURE dbo.SP_Pedido_ObtenerDetalleAdministracion
    @IdPedido INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        PE.IdPedido,
        F.NumeroFactura,
        PE.FechaPedido,
        PE.Subtotal,
        PE.CostoEnvio,
        PE.Total,
        PE.IdEstadoPedido,
        EP.Nombre   AS NombreEstado,
        EP.ClaseCss AS ClaseCssEstado,
        PE.MotivoCancelacion,
        U.IdUsuario,
        U.Nombre + N' ' + U.Apellido AS NombreCliente,
        U.Correo   AS CorreoCliente,
        U.Telefono AS TelefonoCliente,
        DE.NombreDestinatario,
        DE.TelefonoContacto,
        DE.DireccionExacta,
        DE.Referencia,
        DI.Nombre AS NombreDistrito,
        CA.Nombre AS NombreCanton,
        PR.Nombre AS NombreProvincia,
        MP.Nombre AS NombreMetodoPago,
        PG.Comprobante AS ComprobantePago,
        PG.FechaPago,
        EPG.Nombre AS EstadoPago,
        DP.IdProducto,
        P.Nombre AS NombreProducto,
        DP.Cantidad,
        DP.PrecioUnitario,
        DP.SubtotalLinea
    FROM dbo.TB_PEDIDO AS PE
    INNER JOIN dbo.TB_USUARIO         AS U   ON U.IdUsuario        = PE.IdUsuario
    INNER JOIN dbo.TB_ESTADO_PEDIDO   AS EP  ON EP.IdEstadoPedido  = PE.IdEstadoPedido
    INNER JOIN dbo.TB_DIRECCION_ENVIO AS DE  ON DE.IdDireccion     = PE.IdDireccion
    INNER JOIN dbo.TB_DISTRITO        AS DI  ON DI.IdDistrito      = DE.IdDistrito
    INNER JOIN dbo.TB_CANTON          AS CA  ON CA.IdCanton        = DI.IdCanton
    INNER JOIN dbo.TB_PROVINCIA       AS PR  ON PR.IdProvincia     = CA.IdProvincia
    INNER JOIN dbo.TB_DETALLE_PEDIDO  AS DP  ON DP.IdPedido        = PE.IdPedido
    INNER JOIN dbo.TB_PRODUCTO        AS P   ON P.IdProducto       = DP.IdProducto
    LEFT  JOIN dbo.TB_FACTURA         AS F   ON F.IdPedido         = PE.IdPedido
    LEFT  JOIN dbo.TB_PAGO            AS PG  ON PG.IdPedido        = PE.IdPedido
    LEFT  JOIN dbo.TB_METODO_PAGO     AS MP  ON MP.IdMetodoPago    = PG.IdMetodoPago
    LEFT  JOIN dbo.TB_ESTADO_PAGO     AS EPG ON EPG.IdEstadoPago   = PG.IdEstadoPago
    WHERE PE.IdPedido = @IdPedido
    ORDER BY DP.IdDetallePedido;
END;
GO

/* ------------------------------------------------------------
   Cambio de estado del pedido.

   Transiciones validas, las mismas del proyecto anterior:
     Pagado (1)         -> En preparacion (2) o Cancelado (4)
     En preparacion (2) -> Entregado (3) o Cancelado (4)
     Entregado (3) y Cancelado (4) son finales

   Al cancelar pasan tres cosas en la misma transaccion:
     - se devuelve el stock de cada producto del pedido
     - se anula el pago y la factura
     - se guarda el motivo

   La anulacion del pago y la factura es un cambio respecto del proyecto
   anterior, que dejaba el pago como Pagado y la factura como emitida
   aunque el pedido estuviera cancelado. Con eso el resumen de pagos
   contaba como recaudado el dinero de pedidos cancelados. Ahora el
   recaudado solo suma pagos en estado Pagado.

   Errores: 50000 si el pedido no existe, si ya esta en un estado final,
            o si la transicion no es valida
   ------------------------------------------------------------ */
CREATE OR ALTER PROCEDURE dbo.SP_Pedido_CambiarEstado
    @IdUsuarioAccion   INT,
    @IdPedido          INT,
    @IdEstadoNuevo     INT,
    @MotivoCancelacion NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @IdEstadoActual INT, @NombreActual NVARCHAR(50), @NombreNuevo NVARCHAR(50);

    SELECT @IdEstadoActual = IdEstadoPedido
    FROM dbo.TB_PEDIDO
    WHERE IdPedido = @IdPedido;

    IF @IdEstadoActual IS NULL
        THROW 50000, N'El pedido indicado no existe.', 1;

    SELECT @NombreActual = Nombre FROM dbo.TB_ESTADO_PEDIDO WHERE IdEstadoPedido = @IdEstadoActual;
    SELECT @NombreNuevo  = Nombre FROM dbo.TB_ESTADO_PEDIDO WHERE IdEstadoPedido = @IdEstadoNuevo;

    IF @NombreNuevo IS NULL
        THROW 50000, N'El estado seleccionado no es válido para un pedido.', 1;

    IF @IdEstadoActual IN (3, 4)
    BEGIN
        DECLARE @MensajeFinal NVARCHAR(300) =
            N'El pedido ya está ' + LOWER(@NombreActual) + N' y no admite más cambios de estado.';
        THROW 50000, @MensajeFinal, 1;
    END;

    IF NOT ((@IdEstadoActual = 1 AND @IdEstadoNuevo IN (2, 4))
         OR (@IdEstadoActual = 2 AND @IdEstadoNuevo IN (3, 4)))
    BEGIN
        DECLARE @MensajeTransicion NVARCHAR(300) =
            N'No es posible pasar de "' + @NombreActual + N'" a "' + @NombreNuevo + N'".';
        THROW 50000, @MensajeTransicion, 1;
    END;

    IF @IdEstadoNuevo = 4 AND NULLIF(LTRIM(RTRIM(@MotivoCancelacion)), N'') IS NULL
        THROW 50000, N'Para cancelar un pedido hay que indicar el motivo.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @IdEstadoNuevo = 4
        BEGIN
            UPDATE P
            SET P.Stock = P.Stock + DP.Cantidad
            FROM dbo.TB_PRODUCTO AS P
            INNER JOIN dbo.TB_DETALLE_PEDIDO AS DP ON DP.IdProducto = P.IdProducto
            WHERE DP.IdPedido = @IdPedido;

            /* 3 es Anulado en TB_ESTADO_PAGO */
            UPDATE dbo.TB_PAGO
            SET IdEstadoPago = 3
            WHERE IdPedido = @IdPedido;

            /* 2 es Anulada en TB_ESTADO_FACTURA */
            UPDATE dbo.TB_FACTURA
            SET IdEstadoFactura = 2
            WHERE IdPedido = @IdPedido;
        END;

        UPDATE dbo.TB_PEDIDO
        SET IdEstadoPedido    = @IdEstadoNuevo,
            MotivoCancelacion = CASE WHEN @IdEstadoNuevo = 4
                                     THEN LTRIM(RTRIM(@MotivoCancelacion))
                                     ELSE NULL END
        WHERE IdPedido = @IdPedido;

        INSERT INTO dbo.TB_BITACORA (IdUsuario, Entidad, IdRegistro, Accion, ValorAnterior, ValorNuevo)
        VALUES (@IdUsuarioAccion, N'TB_PEDIDO', @IdPedido, N'Cambiar estado',
                NULLIF((SELECT @IdEstadoActual AS IdEstadoPedido, @NombreActual AS NombreEstado
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N''),
                NULLIF((SELECT @IdEstadoNuevo AS IdEstadoPedido, @NombreNuevo AS NombreEstado,
                               LTRIM(RTRIM(@MotivoCancelacion)) AS MotivoCancelacion
                        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), N''));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

/* Bandeja de pagos del panel, con los mismos filtros que los pedidos. */
CREATE OR ALTER PROCEDURE dbo.SP_Pago_Listar
    @Busqueda     NVARCHAR(150) = NULL,
    @IdEstadoPago INT = NULL,
    @FechaInicio  DATE = NULL,
    @FechaFin     DATE = NULL,
    @Pagina       INT = 1,
    @TamanoPagina INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @Pagina IS NULL OR @Pagina < 1 SET @Pagina = 1;
    IF @TamanoPagina IS NULL OR @TamanoPagina < 1 SET @TamanoPagina = 10;

    SET @Busqueda = NULLIF(LTRIM(RTRIM(@Busqueda)), N'');

    SELECT
        PG.IdPago,
        PG.IdPedido,
        F.NumeroFactura,
        U.Nombre + N' ' + U.Apellido AS NombreCliente,
        U.Correo AS CorreoCliente,
        MP.Nombre AS NombreMetodoPago,
        PG.Monto,
        PG.FechaPago,
        PG.Comprobante,
        PG.IdEstadoPago,
        EPG.Nombre AS NombreEstado,
        COUNT(*) OVER () AS TotalFilas
    FROM dbo.TB_PAGO AS PG
    INNER JOIN dbo.TB_PEDIDO      AS PE  ON PE.IdPedido      = PG.IdPedido
    INNER JOIN dbo.TB_USUARIO     AS U   ON U.IdUsuario      = PE.IdUsuario
    INNER JOIN dbo.TB_METODO_PAGO AS MP  ON MP.IdMetodoPago  = PG.IdMetodoPago
    INNER JOIN dbo.TB_ESTADO_PAGO AS EPG ON EPG.IdEstadoPago = PG.IdEstadoPago
    LEFT  JOIN dbo.TB_FACTURA     AS F   ON F.IdPedido       = PG.IdPedido
    WHERE (@Busqueda IS NULL
           OR U.Nombre + N' ' + U.Apellido LIKE N'%' + @Busqueda + N'%'
           OR U.Correo LIKE N'%' + @Busqueda + N'%'
           OR PG.Comprobante LIKE N'%' + @Busqueda + N'%'
           OR F.NumeroFactura LIKE N'%' + @Busqueda + N'%')
      AND (@IdEstadoPago IS NULL OR PG.IdEstadoPago = @IdEstadoPago)
      AND (@FechaInicio IS NULL OR PG.FechaPago >= @FechaInicio)
      AND (@FechaFin IS NULL OR PG.FechaPago < DATEADD(DAY, 1, @FechaFin))
    ORDER BY PG.FechaPago DESC, PG.IdPago DESC
    OFFSET (@Pagina - 1) * @TamanoPagina ROWS
    FETCH NEXT @TamanoPagina ROWS ONLY;
END;
GO

/* Tarjetas de arriba de la pantalla de pagos. El recaudado cuenta solo
   los pagos en estado Pagado: un pago anulado por cancelacion no es
   dinero que entro. */
CREATE OR ALTER PROCEDURE dbo.SP_Pago_ConsultarResumen
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Hoy DATE = CAST(SYSDATETIME() AS DATE);

    SELECT
        (SELECT COUNT(*) FROM dbo.TB_PAGO) AS TotalPagos,

        CAST(ISNULL((SELECT SUM(Monto) FROM dbo.TB_PAGO WHERE IdEstadoPago = 2), 0)
             AS DECIMAL(18,2)) AS TotalRecaudado,

        (SELECT COUNT(*) FROM dbo.TB_PAGO
         WHERE IdEstadoPago = 2 AND CAST(FechaPago AS DATE) = @Hoy) AS PagosHoy,

        CAST(ISNULL((SELECT SUM(Monto) FROM dbo.TB_PAGO
                     WHERE IdEstadoPago = 2 AND CAST(FechaPago AS DATE) = @Hoy), 0)
             AS DECIMAL(18,2)) AS RecaudadoHoy,

        (SELECT COUNT(*) FROM dbo.TB_PAGO WHERE IdEstadoPago = 3) AS PagosAnulados;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_Factura_Listar
    @Busqueda     NVARCHAR(150) = NULL,
    @FechaInicio  DATE = NULL,
    @FechaFin     DATE = NULL,
    @Pagina       INT = 1,
    @TamanoPagina INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @Pagina IS NULL OR @Pagina < 1 SET @Pagina = 1;
    IF @TamanoPagina IS NULL OR @TamanoPagina < 1 SET @TamanoPagina = 10;

    SET @Busqueda = NULLIF(LTRIM(RTRIM(@Busqueda)), N'');

    SELECT
        F.IdFactura,
        F.IdPedido,
        F.NumeroFactura,
        F.FechaFactura,
        F.Subtotal,
        F.CostoEnvio,
        F.Total,
        F.IdEstadoFactura,
        EF.Nombre AS NombreEstado,
        U.Nombre + N' ' + U.Apellido AS NombreCliente,
        U.Correo AS CorreoCliente,
        MP.Nombre AS NombreMetodoPago,
        PG.Comprobante,
        COUNT(*) OVER () AS TotalFilas
    FROM dbo.TB_FACTURA AS F
    INNER JOIN dbo.TB_PEDIDO         AS PE ON PE.IdPedido        = F.IdPedido
    INNER JOIN dbo.TB_USUARIO        AS U  ON U.IdUsuario        = PE.IdUsuario
    INNER JOIN dbo.TB_ESTADO_FACTURA AS EF ON EF.IdEstadoFactura = F.IdEstadoFactura
    LEFT  JOIN dbo.TB_PAGO           AS PG ON PG.IdPedido        = F.IdPedido
    LEFT  JOIN dbo.TB_METODO_PAGO    AS MP ON MP.IdMetodoPago    = PG.IdMetodoPago
    WHERE (@Busqueda IS NULL
           OR F.NumeroFactura LIKE N'%' + @Busqueda + N'%'
           OR U.Nombre + N' ' + U.Apellido LIKE N'%' + @Busqueda + N'%'
           OR U.Correo LIKE N'%' + @Busqueda + N'%')
      AND (@FechaInicio IS NULL OR F.FechaFactura >= @FechaInicio)
      AND (@FechaFin IS NULL OR F.FechaFactura < DATEADD(DAY, 1, @FechaFin))
    ORDER BY F.FechaFactura DESC, F.IdFactura DESC
    OFFSET (@Pagina - 1) * @TamanoPagina ROWS
    FETCH NEXT @TamanoPagina ROWS ONLY;
END;
GO

/* Factura para imprimir o ver en pantalla. Una fila por producto con el
   encabezado repetido, igual que los otros detalles. */
CREATE OR ALTER PROCEDURE dbo.SP_Factura_ObtenerDetalle
    @IdFactura INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        F.IdFactura,
        F.IdPedido,
        F.NumeroFactura,
        F.FechaFactura,
        F.Subtotal,
        F.CostoEnvio,
        F.Total,
        EF.Nombre AS NombreEstado,
        U.IdUsuario,
        U.Nombre + N' ' + U.Apellido AS NombreCliente,
        U.Correo   AS CorreoCliente,
        U.Telefono AS TelefonoCliente,
        U.Cedula   AS CedulaCliente,
        DE.NombreDestinatario,
        DE.DireccionExacta,
        DE.Referencia,
        PR.Nombre AS NombreProvincia,
        CA.Nombre AS NombreCanton,
        DI.Nombre AS NombreDistrito,
        MP.Nombre AS NombreMetodoPago,
        PG.Comprobante,
        PG.FechaPago,
        P.IdProducto,
        P.Nombre AS NombreProducto,
        DP.Cantidad,
        DP.PrecioUnitario,
        DP.SubtotalLinea
    FROM dbo.TB_FACTURA AS F
    INNER JOIN dbo.TB_PEDIDO          AS PE ON PE.IdPedido        = F.IdPedido
    INNER JOIN dbo.TB_USUARIO         AS U  ON U.IdUsuario        = PE.IdUsuario
    INNER JOIN dbo.TB_ESTADO_FACTURA  AS EF ON EF.IdEstadoFactura = F.IdEstadoFactura
    INNER JOIN dbo.TB_DETALLE_PEDIDO  AS DP ON DP.IdPedido        = F.IdPedido
    INNER JOIN dbo.TB_PRODUCTO        AS P  ON P.IdProducto       = DP.IdProducto
    INNER JOIN dbo.TB_DIRECCION_ENVIO AS DE ON DE.IdDireccion     = PE.IdDireccion
    INNER JOIN dbo.TB_DISTRITO        AS DI ON DI.IdDistrito      = DE.IdDistrito
    INNER JOIN dbo.TB_CANTON          AS CA ON CA.IdCanton        = DI.IdCanton
    INNER JOIN dbo.TB_PROVINCIA       AS PR ON PR.IdProvincia     = CA.IdProvincia
    LEFT  JOIN dbo.TB_PAGO            AS PG ON PG.IdPedido        = F.IdPedido
    LEFT  JOIN dbo.TB_METODO_PAGO     AS MP ON MP.IdMetodoPago    = PG.IdMetodoPago
    WHERE F.IdFactura = @IdFactura
    ORDER BY P.Nombre;
END;
GO

/* ============================================================
   9. Reportes y tablero

   Un pedido cuenta como venta cuando su estado es Pagado (1), En
   preparacion (2) o Entregado (3). Cancelado (4) no suma en ningun
   reporte: el dinero se devolvio y el stock volvio a la bodega.

   @FechaFin se compara con DATEADD(DAY, 1, @FechaFin) y menor estricto,
   no con <=. FechaPedido es DATETIME2 y lleva hora: con <= se perderian
   todos los pedidos del ultimo dia del rango salvo los de medianoche
   exacta.
   ============================================================ */

CREATE OR ALTER PROCEDURE dbo.SP_Reporte_ResumenVentas
    @FechaInicio DATE,
    @FechaFin    DATE
AS
BEGIN
    SET NOCOUNT ON;

    IF @FechaInicio IS NULL OR @FechaFin IS NULL
        THROW 50000, N'El reporte necesita la fecha de inicio y la de fin.', 1;

    IF @FechaInicio > @FechaFin
        THROW 50000, N'La fecha de inicio no puede ser posterior a la de fin.', 1;

    SELECT
        CAST(ISNULL(SUM(PE.Total), 0) AS DECIMAL(18,2)) AS TotalVentas,
        COUNT(*) AS CantidadPedidos,
        CAST(ISNULL(AVG(PE.Total), 0) AS DECIMAL(18,2)) AS TicketPromedio,
        ISNULL((SELECT SUM(DP.Cantidad)
                FROM dbo.TB_DETALLE_PEDIDO AS DP
                INNER JOIN dbo.TB_PEDIDO AS P2 ON P2.IdPedido = DP.IdPedido
                WHERE P2.IdEstadoPedido IN (1, 2, 3)
                  AND P2.FechaPedido >= @FechaInicio
                  AND P2.FechaPedido < DATEADD(DAY, 1, @FechaFin)), 0) AS ProductosVendidos
    FROM dbo.TB_PEDIDO AS PE
    WHERE PE.IdEstadoPedido IN (1, 2, 3)
      AND PE.FechaPedido >= @FechaInicio
      AND PE.FechaPedido < DATEADD(DAY, 1, @FechaFin);
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_Reporte_VentasPorDia
    @FechaInicio DATE,
    @FechaFin    DATE
AS
BEGIN
    SET NOCOUNT ON;

    IF @FechaInicio IS NULL OR @FechaFin IS NULL
        THROW 50000, N'El reporte necesita la fecha de inicio y la de fin.', 1;

    IF @FechaInicio > @FechaFin
        THROW 50000, N'La fecha de inicio no puede ser posterior a la de fin.', 1;

    SELECT
        CAST(PE.FechaPedido AS DATE) AS Fecha,
        COUNT(*) AS CantidadPedidos,
        CAST(ISNULL(SUM(PE.Total), 0) AS DECIMAL(18,2)) AS TotalVentas
    FROM dbo.TB_PEDIDO AS PE
    WHERE PE.IdEstadoPedido IN (1, 2, 3)
      AND PE.FechaPedido >= @FechaInicio
      AND PE.FechaPedido < DATEADD(DAY, 1, @FechaFin)
    GROUP BY CAST(PE.FechaPedido AS DATE)
    ORDER BY Fecha;
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_Reporte_ProductosMasVendidos
    @FechaInicio DATE,
    @FechaFin    DATE,
    @Cantidad    INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @FechaInicio IS NULL OR @FechaFin IS NULL
        THROW 50000, N'El reporte necesita la fecha de inicio y la de fin.', 1;

    IF @FechaInicio > @FechaFin
        THROW 50000, N'La fecha de inicio no puede ser posterior a la de fin.', 1;

    IF @Cantidad IS NULL OR @Cantidad < 1 SET @Cantidad = 10;

    SELECT TOP (@Cantidad)
        P.IdProducto,
        P.Nombre AS NombreProducto,
        C.Nombre AS NombreCategoria,
        SUM(DP.Cantidad) AS UnidadesVendidas,
        CAST(SUM(DP.SubtotalLinea) AS DECIMAL(18,2)) AS TotalVendido
    FROM dbo.TB_DETALLE_PEDIDO AS DP
    INNER JOIN dbo.TB_PEDIDO    AS PE ON PE.IdPedido    = DP.IdPedido
    INNER JOIN dbo.TB_PRODUCTO  AS P  ON P.IdProducto   = DP.IdProducto
    INNER JOIN dbo.TB_CATEGORIA AS C  ON C.IdCategoria  = P.IdCategoria
    WHERE PE.IdEstadoPedido IN (1, 2, 3)
      AND PE.FechaPedido >= @FechaInicio
      AND PE.FechaPedido < DATEADD(DAY, 1, @FechaFin)
    GROUP BY P.IdProducto, P.Nombre, C.Nombre
    ORDER BY UnidadesVendidas DESC, TotalVendido DESC;
END;
GO

/* Tarjetas del tablero */
CREATE OR ALTER PROCEDURE dbo.SP_Tablero_ConsultarResumen
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CAST(ISNULL((SELECT SUM(Total) FROM dbo.TB_PEDIDO
                     WHERE IdEstadoPedido IN (1, 2, 3)), 0)
             AS DECIMAL(18,2)) AS TotalVentas,

        (SELECT COUNT(*) FROM dbo.TB_PEDIDO WHERE IdEstadoPedido <> 4) AS TotalPedidos,

        (SELECT COUNT(*) FROM dbo.TB_PEDIDO WHERE IdEstadoPedido = 1) AS PedidosPorPreparar,

        (SELECT COUNT(*) FROM dbo.TB_USUARIO WHERE IdRol = 2 AND Activo = 1) AS TotalClientesActivos,

        (SELECT COUNT(*) FROM dbo.TB_PRODUCTO WHERE Activo = 1 AND Stock = 0) AS TotalProductosAgotados,

        (SELECT COUNT(*) FROM dbo.TB_PRODUCTO WHERE Activo = 1) AS TotalProductosActivos;
END;
GO

/* Grafico de ventas del anio. Devuelve los doce meses siempre, con cero
   donde no hubo ventas: si se devolvieran solo los meses con datos, el
   grafico saldria con el eje incompleto. */
CREATE OR ALTER PROCEDURE dbo.SP_Tablero_ConsultarVentasMensuales
    @Anio INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Anio IS NULL SET @Anio = YEAR(SYSDATETIME());

    ;WITH Meses AS
    (
        SELECT * FROM (VALUES
            (1, N'Ene'), (2, N'Feb'), (3, N'Mar'), (4, N'Abr'),
            (5, N'May'), (6, N'Jun'), (7, N'Jul'), (8, N'Ago'),
            (9, N'Sep'), (10, N'Oct'), (11, N'Nov'), (12, N'Dic')
        ) AS M (NumeroMes, NombreMes)
    ),
    Ventas AS
    (
        SELECT
            MONTH(PE.FechaPedido) AS NumeroMes,
            SUM(PE.Total) AS TotalVentas,
            COUNT(*) AS CantidadPedidos
        FROM dbo.TB_PEDIDO AS PE
        WHERE YEAR(PE.FechaPedido) = @Anio
          AND PE.IdEstadoPedido IN (1, 2, 3)
        GROUP BY MONTH(PE.FechaPedido)
    )
    SELECT
        M.NumeroMes,
        M.NombreMes,
        CAST(ISNULL(V.TotalVentas, 0) AS DECIMAL(18,2)) AS TotalVentas,
        ISNULL(V.CantidadPedidos, 0) AS CantidadPedidos
    FROM Meses AS M
    LEFT JOIN Ventas AS V ON V.NumeroMes = M.NumeroMes
    ORDER BY M.NumeroMes;
END;
GO

/* Ultimos movimientos para la lista de actividad del tablero */
CREATE OR ALTER PROCEDURE dbo.SP_Tablero_ConsultarActividadReciente
    @Cantidad INT = 5
AS
BEGIN
    SET NOCOUNT ON;

    IF @Cantidad IS NULL OR @Cantidad < 1 SET @Cantidad = 5;

    SELECT TOP (@Cantidad)
        PE.IdPedido,
        CONCAT(N'Pedido #', PE.IdPedido) AS Titulo,
        CONCAT(U.Nombre, N' ', U.Apellido, N' realizó un pedido por ₡',
               CONVERT(NVARCHAR(30), CAST(PE.Total AS DECIMAL(18,2)))) AS Descripcion,
        PE.FechaPedido AS Fecha,
        EP.Nombre   AS Tipo,
        EP.ClaseCss AS ClaseCssEstado
    FROM dbo.TB_PEDIDO AS PE
    INNER JOIN dbo.TB_USUARIO       AS U  ON U.IdUsuario       = PE.IdUsuario
    INNER JOIN dbo.TB_ESTADO_PEDIDO AS EP ON EP.IdEstadoPedido = PE.IdEstadoPedido
    ORDER BY PE.FechaPedido DESC, PE.IdPedido DESC;
END;
GO

PRINT N'03_procedimientos.sql: listo.';
GO

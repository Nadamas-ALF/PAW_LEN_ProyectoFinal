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

DECLARE @Resumen NVARCHAR(200) =
    N'05_datos_prueba.sql: listo. Administradores de prueba=' + CAST((SELECT COUNT(*) FROM dbo.TB_USUARIO WHERE Correo = N'admin@len.test' AND IdRol = 1) AS NVARCHAR(10))
  + N' Clientes de prueba=' + CAST((SELECT COUNT(*) FROM dbo.TB_USUARIO WHERE Correo = N'cliente@len.test' AND IdRol = 2) AS NVARCHAR(10));

PRINT @Resumen;
GO

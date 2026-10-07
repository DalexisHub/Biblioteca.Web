-- MVC con Dapper SEMANA08
-- Fuente: Biblioteca-ArquitecturaCapas-Semana07/Database/01_Schema.sql.
-- Ejecutar solo sobre BibliotecaDB existente. No ejecutar Schema/Seed de semana 7.
USE [BibliotecaDB];
GO

CREATE OR ALTER PROCEDURE dbo.S08_Libros_Listar

AS
BEGIN
 SET NOCOUNT ON;
 SELECT l.LibroId, l.Titulo, l.ISBN, l.AutorId, l.Ejemplares, l.Activo, a.Nombre AS AutorNombre FROM dbo.Libros l INNER JOIN dbo.Autores a ON a.AutorId = l.AutorId WHERE l.Activo = 1 ORDER BY l.Titulo, l.LibroId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.S08_Libros_Buscar
 @Titulo nvarchar(200)
AS
BEGIN
 SET NOCOUNT ON;
 SELECT l.LibroId, l.Titulo, l.ISBN, l.AutorId, l.Ejemplares, l.Activo, a.Nombre AS AutorNombre FROM dbo.Libros l INNER JOIN dbo.Autores a ON a.AutorId = l.AutorId WHERE l.Activo = 1 AND CHARINDEX(@Titulo, l.Titulo) > 0 ORDER BY l.Titulo, l.LibroId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.S08_Libros_Obtener
 @LibroId int
AS
BEGIN
 SET NOCOUNT ON;
 SELECT l.LibroId, l.Titulo, l.ISBN, l.AutorId, l.Ejemplares, l.Activo, a.Nombre AS AutorNombre FROM dbo.Libros l INNER JOIN dbo.Autores a ON a.AutorId = l.AutorId WHERE l.LibroId = @LibroId AND l.Activo = 1;
END;
GO

CREATE OR ALTER PROCEDURE dbo.S08_Libros_Insertar
 @Titulo nvarchar(200), @ISBN nvarchar(20), @AutorId int, @Ejemplares int
AS
BEGIN
 SET NOCOUNT ON;
 IF NOT EXISTS (SELECT 1 FROM dbo.Autores WHERE AutorId = @AutorId AND Activo = 1)
  THROW 50001, N'Selecciona un autor activo.', 1;
 IF NULLIF(LTRIM(RTRIM(@Titulo)), N'') IS NULL OR NULLIF(LTRIM(RTRIM(@ISBN)), N'') IS NULL OR @Ejemplares < 0
  THROW 50003, N'Comprueba el título, ISBN y número de ejemplares.', 1;
 INSERT INTO dbo.Libros (Titulo, ISBN, AutorId, Ejemplares, Activo)
 VALUES (@Titulo, @ISBN, @AutorId, @Ejemplares, 1);
END;
GO

CREATE OR ALTER PROCEDURE dbo.S08_Libros_Actualizar
 @LibroId int,
 @Titulo nvarchar(200), @ISBN nvarchar(20), @AutorId int, @Ejemplares int
AS
BEGIN
 SET NOCOUNT ON;
 IF NOT EXISTS (SELECT 1 FROM dbo.Autores WHERE AutorId = @AutorId AND Activo = 1)
  THROW 50001, N'Selecciona un autor activo.', 1;
 IF NULLIF(LTRIM(RTRIM(@Titulo)), N'') IS NULL OR NULLIF(LTRIM(RTRIM(@ISBN)), N'') IS NULL OR @Ejemplares < 0
  THROW 50003, N'Comprueba el título, ISBN y número de ejemplares.', 1;
 UPDATE dbo.Libros SET Titulo = @Titulo, ISBN = @ISBN, AutorId = @AutorId, Ejemplares = @Ejemplares
 WHERE LibroId = @LibroId AND Activo = 1;
 IF @@ROWCOUNT = 0 THROW 50002, N'El libro ya no está activo o no existe.', 1;
END;
GO

CREATE OR ALTER PROCEDURE dbo.S08_Libros_Eliminar
 @LibroId int
AS
BEGIN
 SET NOCOUNT ON;
 -- Conserva la regla de semana 7 y todo el historial.
 SET XACT_ABORT ON;
 BEGIN TRY
  BEGIN TRANSACTION;
  IF NOT EXISTS (SELECT 1 FROM dbo.Libros WITH (UPDLOCK, HOLDLOCK) WHERE LibroId = @LibroId AND Activo = 1)
   THROW 50002, N'El libro ya no está activo o no existe.', 1;
  IF EXISTS (SELECT 1 FROM dbo.DetallePrestamo d INNER JOIN dbo.Prestamos p ON p.PrestamoId = d.PrestamoId
    WHERE d.LibroId = @LibroId AND d.FechaDevolucion IS NULL AND p.Estado = N'Pendiente')
   THROW 50003, N'No se puede dar de baja un libro con préstamos pendientes.', 1;
  UPDATE dbo.Libros SET Activo = 0 WHERE LibroId = @LibroId AND Activo = 1;
  COMMIT TRANSACTION;
 END TRY
 BEGIN CATCH
  IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
  THROW;
 END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.S08_Autores_Listar

AS
BEGIN
 SET NOCOUNT ON;
 SELECT AutorId, Nombre FROM dbo.Autores WHERE Activo = 1 ORDER BY Nombre, AutorId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.S08_Socios_Listar

AS
BEGIN
 SET NOCOUNT ON;
 SELECT SocioId, DNI, Nombre, Email, Activo FROM dbo.Socios WHERE Activo = 1 ORDER BY Nombre, SocioId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.S08_Socios_Insertar
 @DNI nvarchar(15), @Nombre nvarchar(150), @Email nvarchar(150)
AS
BEGIN
 SET NOCOUNT ON;
 -- UQ_Socios_DNI protege también contra registros concurrentes e inactivos.
 INSERT INTO dbo.Socios (DNI, Nombre, Email, Activo) VALUES (@DNI, @Nombre, @Email, 1);
END;
GO

CREATE OR ALTER PROCEDURE dbo.S08_Prestamos_Reporte
 @Desde date, @Hasta date
AS
BEGIN
 SET NOCOUNT ON;
 IF @Desde IS NULL OR @Hasta IS NULL OR @Desde > @Hasta
  THROW 50003, N'El intervalo de fechas no es válido.', 1;
 -- Una fila por libro prestado; incluye el día Hasta completo sin desbordar 9999-12-31.
 -- Conserva préstamos históricos aunque el libro o socio esté inactivo.
 SELECT p.PrestamoId, s.Nombre AS SocioNombre, l.Titulo AS LibroTitulo,
        p.FechaPrestamo, p.FechaLimite, p.Estado
 FROM dbo.Prestamos p
 INNER JOIN dbo.DetallePrestamo d ON d.PrestamoId = p.PrestamoId
 INNER JOIN dbo.Libros l ON l.LibroId = d.LibroId
 INNER JOIN dbo.Socios s ON s.SocioId = p.SocioId
 WHERE p.FechaPrestamo >= @Desde AND CONVERT(date, p.FechaPrestamo) <= @Hasta
 ORDER BY p.FechaPrestamo DESC, p.PrestamoId, l.Titulo;
END;
GO

-- Inicialización local autorizada: copia del esquema/datos de semana 7.
-- Ejecutar con sqlcmd. Se detiene si BibliotecaDB ya existe; nunca elimina tablas.
:ON ERROR EXIT
USE master;
GO
IF DB_ID(N'BibliotecaDB') IS NOT NULL
 THROW 51000, N'BibliotecaDB ya existe. No se modificó. Usa el script de procedimientos.', 1;
GO
CREATE DATABASE BibliotecaDB;
GO
USE BibliotecaDB;
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;
GO
CREATE TABLE dbo.Autores
(
    AutorId       INT IDENTITY(1,1) PRIMARY KEY,
    Nombre        NVARCHAR(150)   NOT NULL,
    Nacionalidad  NVARCHAR(100)   NOT NULL,
    Activo        BIT             NOT NULL CONSTRAINT DF_Autores_Activo DEFAULT (1)
);
GO

CREATE TABLE dbo.Libros
(
    LibroId     INT IDENTITY(1,1) PRIMARY KEY,
    Titulo      NVARCHAR(200)   NOT NULL,
    ISBN        NVARCHAR(20)    NOT NULL,
    AutorId     INT             NOT NULL,
    Ejemplares  INT             NOT NULL,
    Activo      BIT             NOT NULL CONSTRAINT DF_Libros_Activo DEFAULT (1),
    CONSTRAINT UQ_Libros_ISBN UNIQUE (ISBN),
    CONSTRAINT FK_Libros_Autores FOREIGN KEY (AutorId) REFERENCES dbo.Autores(AutorId),
    CONSTRAINT CK_Libros_Ejemplares CHECK (Ejemplares >= 0)
);
GO

CREATE TABLE dbo.Socios
(
    SocioId INT IDENTITY(1,1) PRIMARY KEY,
    DNI     NVARCHAR(15)    NOT NULL,
    Nombre  NVARCHAR(150)   NOT NULL,
    Email   NVARCHAR(150)   NOT NULL,
    Activo  BIT             NOT NULL CONSTRAINT DF_Socios_Activo DEFAULT (1),
    CONSTRAINT UQ_Socios_DNI UNIQUE (DNI)
);
GO

CREATE TABLE dbo.Prestamos
(
    PrestamoId    INT IDENTITY(1,1) PRIMARY KEY,
    SocioId       INT             NOT NULL,
    FechaPrestamo DATETIME2       NOT NULL,
    FechaLimite   DATETIME2       NOT NULL,
    Estado        NVARCHAR(20)    NOT NULL,
    CONSTRAINT FK_Prestamos_Socios FOREIGN KEY (SocioId) REFERENCES dbo.Socios(SocioId),
    CONSTRAINT CK_Prestamos_Estado CHECK (Estado IN (N'Pendiente', N'Devuelto'))
);
GO

CREATE TABLE dbo.DetallePrestamo
(
    PrestamoId      INT             NOT NULL,
    LibroId         INT             NOT NULL,
    FechaDevolucion DATETIME2       NULL,
    CONSTRAINT PK_DetallePrestamo PRIMARY KEY (PrestamoId, LibroId),
    CONSTRAINT FK_DetallePrestamo_Prestamos FOREIGN KEY (PrestamoId) REFERENCES dbo.Prestamos(PrestamoId),
    CONSTRAINT FK_DetallePrestamo_Libros FOREIGN KEY (LibroId) REFERENCES dbo.Libros(LibroId)
);
GO

INSERT INTO dbo.Autores (Nombre, Nacionalidad, Activo) VALUES
(N'Gabriel García Márquez', N'Colombia', 1),
(N'Mario Vargas Llosa',     N'Perú',     1),
(N'Isabel Allende',         N'Chile',    1),
(N'Jorge Luis Borges',      N'Argentina',1),
(N'Julio Cortázar',         N'Argentina',1),
(N'Pablo Neruda',           N'Chile',    1),
(N'Laura Esquivel',         N'México',   1),
(N'Octavio Paz',            N'México',   1);
GO

INSERT INTO dbo.Libros (Titulo, ISBN, AutorId, Ejemplares, Activo) VALUES
(N'Cien años de soledad',              N'978-0307474728', 1, 3, 1),
(N'El amor en los tiempos del cólera', N'978-1400034680', 1, 2, 1),
(N'Crónica de una muerte anunciada',   N'978-1400034956', 1, 4, 1),
(N'La ciudad y los perros',            N'978-8420471839', 2, 2, 1),
(N'La fiesta del Chivo',               N'978-8420471181', 2, 3, 1),
(N'Conversación en La Catedral',       N'978-8420471198', 2, 1, 1),
(N'La casa de los espíritus',          N'978-8401242131', 3, 3, 1),
(N'Eva Luna',                          N'978-8401242179', 3, 2, 1),
(N'Ficciones',                         N'978-8420633129', 4, 4, 1),
(N'El Aleph',                          N'978-8420633136', 4, 2, 1),
(N'El libro de arena',                 N'978-8420633143', 4, 1, 1),
(N'Rayuela',                           N'978-8437604572', 5, 2, 1),
(N'Bestiario',                         N'978-8437604589', 5, 0, 1),
(N'Veinte poemas de amor',             N'978-9561314601', 6, 5, 1),
(N'Canto general',                     N'978-9561314618', 6, 2, 1),
(N'Como agua para chocolate',          N'978-9681101002', 7, 3, 1),
(N'La ley del amor',                   N'978-9681101019', 7, 1, 1),
(N'El laberinto de la soledad',        N'978-9681602182', 8, 2, 1),
(N'Piedra de sol',                     N'978-9681602199', 8, 3, 1),
(N'Libertad bajo palabra',             N'978-9681602205', 8, 0, 1);
GO

INSERT INTO dbo.Socios (DNI, Nombre, Email, Activo) VALUES
(N'71234561', N'Ana Torres',     N'ana.torres@correo.com',     1),
(N'71234562', N'Luis Ramírez',   N'luis.ramirez@correo.com',   1),
(N'71234563', N'Carla Medina',   N'carla.medina@correo.com',   1),
(N'71234564', N'Jorge Salas',    N'jorge.salas@correo.com',    1),
(N'71234565', N'Patricia Vega',  N'patricia.vega@correo.com',  1),
(N'71234566', N'Miguel Rojas',   N'miguel.rojas@correo.com',   1),
(N'71234567', N'Sofía Castro',   N'sofia.castro@correo.com',   1),
(N'71234568', N'Diego Herrera',  N'diego.herrera@correo.com',  1),
(N'71234569', N'Valeria Núñez',  N'valeria.nunez@correo.com',  1),
(N'71234570', N'Andrés Flores',  N'andres.flores@correo.com',  1);
GO

-- Prestamo 1: Ana Torres (SocioId 1) con 3 libros pendientes -> prueba regla de maximo 3
INSERT INTO dbo.Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado) VALUES
(1, '2026-09-10', '2026-09-24', N'Pendiente');
INSERT INTO dbo.DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(1, 1, NULL),
(1, 4, NULL),
(1, 7, NULL);

-- Prestamo 2: Luis Ramirez, devuelto
INSERT INTO dbo.Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado) VALUES
(2, '2026-09-01', '2026-09-15', N'Devuelto');
INSERT INTO dbo.DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(2, 2, '2026-09-14');

-- Prestamo 3: Carla Medina, 2 libros pendientes
INSERT INTO dbo.Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado) VALUES
(3, '2026-09-05', '2026-09-19', N'Pendiente');
INSERT INTO dbo.DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(3, 9, NULL),
(3, 10, NULL);

-- Prestamo 4: Jorge Salas, vencido (para probar multa)
INSERT INTO dbo.Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado) VALUES
(4, '2026-08-20', '2026-09-03', N'Pendiente');
INSERT INTO dbo.DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(4, 12, NULL);

-- Prestamo 5: Patricia Vega, devuelto
INSERT INTO dbo.Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado) VALUES
(5, '2026-09-20', '2026-10-04', N'Devuelto');
INSERT INTO dbo.DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(5, 16, '2026-09-25');
GO

COMMIT TRANSACTION;
GO

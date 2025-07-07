CREATE DATABASE MiTabla;
GO

USE MiTabla;
GO

-- Crear tabla con valores por defecto en las fechas
CREATE TABLE RegistrosUsuarios (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Usuarios NVARCHAR(100),
    Profesores NVARCHAR(100),
    Cursos NVARCHAR(100),
    Alumnos NVARCHAR(100),
    Inscritos NVARCHAR(100),
    Constancias NVARCHAR(100),
    Pagos NVARCHAR(100),
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),  -- se llena al insertar
    FechaModificacion DATETIME NOT NULL DEFAULT GETDATE(), -- también al insertar
    Usuario NVARCHAR(50) NOT NULL
);
GO

-- Crear trigger para actualizar FechaModificacion automáticamente al hacer UPDATE
CREATE TRIGGER TRG_AutoUpdate_FechaModificacion
ON RegistrosUsuarios
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE RegistrosUsuarios
    SET FechaModificacion = GETDATE()
    FROM RegistrosUsuarios RU
    INNER JOIN inserted i ON RU.Id = i.Id;
END;
GO
-- Script para crear la tabla PREGUNTA en SQL Server
-- Ejecutar en tu base de datos DeltaTest

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PREGUNTA' and xtype='U')
BEGIN
    CREATE TABLE [dbo].[PREGUNTA]
    (
   [id_pregunta] INT IDENTITY(1,1) PRIMARY KEY,
        [texto] NVARCHAR(500) NOT NULL,
        [tipo_evaluacion] BIT NOT NULL DEFAULT 1
    );
    
    -- Crear índice para tipo_evaluacion si lo necesitas para queries frecuentes
    CREATE INDEX IX_PREGUNTA_TipoEvaluacion ON [dbo].[PREGUNTA]([tipo_evaluacion]);
    
PRINT 'Tabla PREGUNTA creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla PREGUNTA ya existe';
END
    
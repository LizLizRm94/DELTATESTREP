-- Script para agregar las columnas faltantes a la tabla PREGUNTA si no existen
-- Ejecutar en SQL Server en la base de datos DeltaTest

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PREGUNTA' AND COLUMN_NAME = 'opciones')
BEGIN
    ALTER TABLE [dbo].[PREGUNTA]
    ADD [opciones] NVARCHAR(MAX) NULL;
    PRINT 'Columna opciones agregada exitosamente';
END
ELSE
BEGIN
    PRINT 'Columna opciones ya existe';
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PREGUNTA' AND COLUMN_NAME = 'respuesta_correcta_index')
BEGIN
    ALTER TABLE [dbo].[PREGUNTA]
    ADD [respuesta_correcta_index] INT NULL;
    PRINT 'Columna respuesta_correcta_index agregada exitosamente';
END
ELSE
BEGIN
 PRINT 'Columna respuesta_correcta_index ya existe';
END

-- Verificar estructura actual de la tabla
SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PREGUNTA' ORDER BY ORDINAL_POSITION;

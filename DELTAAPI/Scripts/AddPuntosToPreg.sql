-- Script para agregar la columna 'puntos' a la tabla PREGUNTA
-- Fecha: 2025-01-16

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PREGUNTA' AND COLUMN_NAME = 'puntos')
BEGIN
    ALTER TABLE [PREGUNTA]
    ADD [puntos] INT NOT NULL DEFAULT 0;
    PRINT 'Columna puntos agregada correctamente a PREGUNTA';
END
ELSE
BEGIN
    PRINT 'La columna puntos ya existe en PREGUNTA';
END

-- Verificar que la columna se creó correctamente
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'PREGUNTA' 
ORDER BY ORDINAL_POSITION;

PRINT '? Script completado exitosamente';

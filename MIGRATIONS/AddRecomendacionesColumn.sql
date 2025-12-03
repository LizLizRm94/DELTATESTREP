-- Script para agregar la columna 'recomendaciones' a la tabla EVALUACION
-- Ejecutar esto en SQL Server Management Studio contra la BD DeltaTest

-- Verificar si la columna ya existe
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'EVALUACION' AND COLUMN_NAME = 'recomendaciones'
)
BEGIN
    ALTER TABLE EVALUACION
    ADD recomendaciones nvarchar(max) NULL;
    PRINT 'Columna recomendaciones agregada exitosamente';
END
ELSE
BEGIN
    PRINT 'La columna recomendaciones ya existe en la tabla EVALUACION';
END

-- Verificar que la columna fue creada correctamente
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'EVALUACION' AND COLUMN_NAME = 'recomendaciones';

-- Mostrar todas las columnas de EVALUACION
PRINT 'Columnas actuales de la tabla EVALUACION:';
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'EVALUACION'
ORDER BY ORDINAL_POSITION;

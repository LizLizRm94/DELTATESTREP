# Soluci�n: Error 500 al Guardar Evaluaciones

## Problema
El error "InternalServerError" ocurre porque la tabla `PREGUNTA` no existe en tu base de datos.

## Soluci�n - Opci�n 1: Ejecutar Script SQL (M�s r�pido)

1. Abre **SQL Server Management Studio (SSMS)**
2. Conecta al servidor configurado en `ConnectionStrings:DefaultConnection` (PostgreSQL/Supabase)
3. Selecciona la base de datos: `DeltaTest`
4. Ejecuta el contenido del archivo `DELTAAPI\Scripts\CreatePreguntaTable.sql`

## Soluci�n - Opci�n 2: Usar Entity Framework Migrations

Si prefieres usar migraciones (recomendado para el futuro):

1. Abre **Package Manager Console** en Visual Studio
2. Selecciona el proyecto `DELTAAPI`
3. Ejecuta estos comandos:

```powershell
Add-Migration AddPreguntaTable
Update-Database
```

## C�mo verificar que funcion�

Despu�s de ejecutar la migraci�n o script:

1. Ve a tu base de datos SQL Server
2. Expande `DeltaTest` ? `Tables`
3. Deber�as ver una tabla `PREGUNTA` con las columnas:
   - `id_pregunta` (INT, Primary Key)
   - `texto` (NVARCHAR(500))
   - `tipo_evaluacion` (BIT)

4. Intenta guardar una evaluaci�n desde la aplicaci�n

## Si a�n hay error

Abre la consola del navegador (F12) y verifica:
- El mensaje de error ahora mostrar� m�s detalles (InnerException y StackTrace)
- Esto te ayudar� a identificar exactamente qu� est� fallando

## Modelo de la tabla

```
CREATE TABLE [dbo].[PREGUNTA]
(
    [id_pregunta] INT IDENTITY(1,1) PRIMARY KEY,
 [texto] NVARCHAR(500) NOT NULL,
    [tipo_evaluacion] BIT NOT NULL DEFAULT 1
);
```

Aseg�rate de que esta tabla existe antes de intentar guardar preguntas.

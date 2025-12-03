# ?? Solución: Agregar Columna 'recomendaciones' a la Base de Datos

## ? Lo que ya hicimos

1. Modificamos el DbContext para **ignorar temporalmente** la columna `recomendaciones`
2. Ahora la aplicación funcionará sin errores
3. El campo de recomendaciones se mostrará vacío hasta que agregues la columna a la BD

## ?? Para que funcione COMPLETAMENTE

Necesitas agregar la columna `recomendaciones` a la tabla `EVALUACION` en SQL Server.

### **Opción 1: SQL Server Management Studio (Recomendado)**

1. **Abre SQL Server Management Studio**
2. **Conéctate a tu servidor** (Ej: LAPTOP-E31IL62H\DELTATEST)
3. **Abre una nueva consulta (Query)** y pega esto:

```sql
-- Agregar columna recomendaciones a la tabla EVALUACION
ALTER TABLE EVALUACION
ADD recomendaciones nvarchar(max) NULL;

-- Verificar que se creó correctamente
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'EVALUACION' AND COLUMN_NAME = 'recomendaciones';
```

4. **Presiona F5 o haz clic en "Execute"**
5. **Deberías ver**: Una fila que dice `recomendaciones | nvarchar | YES`

### **Opción 2: Usar PowerShell/CMD**

Si prefieres usar línea de comandos:

```powershell
# Conéctate a SQL Server y ejecuta el comando
sqlcmd -S LAPTOP-E31IL62H\DELTATEST -U sa -P [tu_password] -Q "ALTER TABLE EVALUACION ADD recomendaciones nvarchar(max) NULL;"
```

Reemplaza:
- `LAPTOP-E31IL62H\DELTATEST` con tu servidor
- `[tu_password]` con tu contraseña de sa

### **Opción 3: Script SQL incluido en el proyecto**

En el proyecto hay un archivo: `MIGRATIONS/AddRecomendacionesColumn.sql`

Puedes:
1. Abrirlo en SSMS
2. Copiar y ejecutar el contenido

## ? Pasos DESPUÉS de agregar la columna

### 1. **Actualizar el DbContext**

En `DELTAAPI/Models/DeltaTestContext.cs`, busca esta sección:

```csharp
// Ignorar Recomendaciones si la columna no existe en la BD
entity.Ignore(e => e.Recomendaciones);

// Configuración original (descomenta cuando la columna exista):
// entity.Property(e => e.Recomendaciones)
//     .HasColumnType("nvarchar(max)")
//     .HasColumnName("recomendaciones");
```

**Cámbialo a:**

```csharp
// Configuración de Recomendaciones
entity.Property(e => e.Recomendaciones)
    .HasColumnType("nvarchar(max)")
    .HasColumnName("recomendaciones");
```

### 2. **Recompilar el proyecto**

```
Build > Build Solution
```

### 3. **Reiniciar la aplicación**

Detén y reinicia tanto DELTAAPI como DELTATEST.

## ?? Prueba que funcione

1. Abre el navegador
2. Ve a una página de evaluación
3. Deberías ver ahora el detalle sin errores
4. El campo "Recomendaciones para Mejorar" debería mostrarse

## ?? Verificación rápida

Ejecuta esta query para ver todas las columnas de EVALUACION:

```sql
SELECT *
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'EVALUACION'
ORDER BY ORDINAL_POSITION;
```

Deberías ver `recomendaciones` en la lista.

## ? Si tienes problemas

1. **Error "Column already exists"**: La columna ya existe, sigue el paso "Después de agregar"
2. **Error "Permission denied"**: Asegúrate de tener permisos en la BD
3. **Sigue sin funcionar**: Comparte los logs del servidor en DELTAAPI

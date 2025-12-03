# Guía de Depuración: Error "InternalServerError" en DetalleEvaluacion

## Problema
Al intentar cargar una evaluación, aparece el error: `Error al cargar la evaluación: Error InternalServerError`

## Pasos para Depurar

### 1. **Verificar si existen evaluaciones en la base de datos**
   - Abre tu navegador
   - Ve a: `https://localhost:7287/api/evaluaciones`
   - Esto te mostrará todas las evaluaciones disponibles
   - Busca el `IdEvaluacion` que necesitas

### 2. **Ver los Logs en el Navegador (F12)**
   - Presiona `F12` en el navegador
   - Ve a la pestaña "Consola"
   - Navega a la página de detalle de evaluación
   - Busca mensajes como:
     ```
     Cargando evaluación con ID: [ID]
     Status Code: [Código]
     ```
   - Anota el código de estado HTTP

### 3. **Ver Logs del Servidor API**
   - Mira la consola de Visual Studio donde corre DELTAAPI
   - Busca mensajes de error con el stack trace completo
   - Nota cualquier excepción

### 4. **Causas Comunes**

#### ? Error 404 (Not Found)
- **Causa:** El IdEvaluacion no existe en la BD
- **Solución:** Verifica que el ID sea correcto en `api/evaluaciones`

#### ? Error 500 (Internal Server Error) con mensaje sobre `IdEvaluadoNavigation`
- **Causa:** La evaluación existe pero el usuario evaluado fue eliminado o no existe
- **Solución:** Asegúrate de que el usuario existe en la tabla USUARIO

#### ? Error 500 sin detalles
- **Causa:** Excepción no manejada en el servidor
- **Solución:** Revisa los logs del servidor API

### 5. **Prueba Manual del Endpoint**

Usa Postman o similar:

```
GET https://localhost:7287/api/evaluaciones/{IdEvaluacion}
Headers:
  - Content-Type: application/json
  - Cookie: [si requiere autenticación]
```

**Respuesta esperada si funciona:**
```json
{
  "idEvaluacion": 1,
  "idEvaluado": 1,
  "nombreEvaluado": "Juan Pérez",
  "ciEvaluado": "1234567",
  "nombreAdministrador": "Admin",
  "fechaEvaluacion": "2025-01-15",
  "nota": 85,
  "estadoEvaluacion": "Completada",
  "tipoEvaluacion": "Práctica",
  "recomendaciones": "Mejorar en..."
}
```

### 6. **Verificar Datos en la Base de Datos**

Ejecuta esta query en SQL Server:

```sql
SELECT 
    e.id_evaluacion,
    e.id_evaluado,
    u.nombre_completo,
    u.ci,
    e.fecha_evaluacion,
    e.nota,
    e.tipo_evaluacion,
    e.estado_evaluacion
FROM EVALUACION e
LEFT JOIN USUARIO u ON e.id_evaluado = u.id_usuario
ORDER BY e.id_evaluacion DESC;
```

Verifica que:
- Existan evaluaciones
- El `id_usuario` en EVALUACION existe en la tabla USUARIO
- Los datos estén completos

## Logs Agregados para Depuración

Se agregaron logs de consola en:
- `DELTATEST/Pages/Usuarios/DetalleEvaluacion.razor` - Muestra en navegador (F12)
- `DELTAAPI/Controllers/EvaluacionesController.cs` - Muestra en consola de VS

## Siguiente Paso

Una vez identifiques el error específico, comparte los logs aquí y podré ayudarte a resolverlo.

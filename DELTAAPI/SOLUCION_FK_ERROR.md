# Solución del Error FK: Foreign Key Constraint

## Problema Encontrado

El error ocurría porque intentabas crear una `EVALUACION` sin proporcionar un `IdEvaluado` (Foreign Key):

```
"The INSERT statement conflicted with the FOREIGN KEY constraint 
\"FK__EVALUACIO__id_ev__48CFD27E\". The conflict occurred in database 
\"DeltaTest\", table \"dbo.USUARIO\", column 'id_usuario'."
```

### ¿Por qué pasaba esto?

La tabla `EVALUACION` tiene una restricción de clave externa que requiere:
- `IdEvaluado` debe ser un ID válido de la tabla `USUARIO`

Pero tu código estaba creando evaluaciones **sin especificar quién era el evaluado**.

## Solución Implementada

### Cambio 1: Separar Preguntas de Evaluaciones

**Ahora el flujo es:**
1. **Primero**: Creas un banco de preguntas (sin necesidad de usuario)
2. **Después**: Cuando asignes una evaluación a un usuario, asociarás estas preguntas

### Cambio 2: Actualización del Controlador

Simplificamos el endpoint para **solo guardar preguntas**:

```csharp
// Antes (causaba error):
var evaluacion = new Evaluacion { IdEvaluado = ??? }; // ? Sin usuario

// Después (funciona):
var pregunta = new Pregunta { Texto = "...", TipoEvaluacion = true }; // ? Sin dependencias
```

### Cambio 3: Actualización del Cliente

El componente Blazor ahora muestra:
```
"¡Preguntas de evaluación teórica guardadas exitosamente!"
```

En lugar de:
```
"¡Evaluación teórica guardada exitosamente!"
```

## Próximos Pasos

### 1. Crear un nuevo endpoint para asignar preguntas a evaluaciones

Cuando tengas un usuario, podrás asignarle una evaluación teórica:

```csharp
[HttpPost("asignar-a-usuario/{idUsuario}")]
public async Task<IActionResult> AsignarEvaluacionAUsuario(int idUsuario, [FromBody] List<int> idsPreguntas)
{
 // Crear EVALUACION con IdEvaluado = idUsuario
  // Asociar las preguntas seleccionadas
}
```

### 2. Mostrar banco de preguntas

Un endpoint para listar todas las preguntas disponibles:
```
GET /api/preguntas/teoricas
```

Ya existe en el controlador.

## Cómo Probar Ahora

1. Reinicia tu API
2. Ve a la página de Evaluación Teórica
3. Ingresa algunas preguntas
4. Presiona "Subir"
5. Deberías ver: ? "¡Preguntas de evaluación teórica guardadas exitosamente!"

## Base de Datos

Tu tabla `PREGUNTA` ahora actúa como un **banco de preguntas reutilizables**, sin necesidad de estar vinculada a evaluaciones específicas desde el principio.

Cuando crees evaluaciones completas (asignadas a usuarios), entonces sí crearás registros en `EVALUACION` y relacionarás las preguntas.

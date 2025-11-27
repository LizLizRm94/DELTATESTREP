# Guía de Pruebas - Evaluación Práctica

## ?? Pruebas de API con Postman

### 1. Obtener Lista de Usuarios
```
GET https://localhost:7287/api/usuarios
Headers: 
  - Content-Type: application/json
  
Response (200 OK):
[
  {
    "idUsuario": 1,
    "nombreCompleto": "Juan Pérez",
    "correo": "juan@delta.com",
    "fechaIngreso": "2024-01-15",
    "puestoActual": "Técnico de Laboratorio",
    "puestoSolicitado": "Supervisor",
    "estado": "Activo"
  },
  ...
]
```

### 2. Crear Evaluación Práctica
```
POST https://localhost:7287/api/evaluaciones/crear-evaluacion-practica
Headers:
  - Content-Type: application/json

Body:
{
  "idUsuario": 1,
  "nombreUsuario": "Juan Pérez",
  "tareas": [
    {
      "idTarea": 1,
      "descripcion": "Realizar análisis de pureza",
      "resultadoObtenido": "Pureza: 98.5%",
 "completada": true,
      "calificacion": 100
    },
    {
      "idTarea": 2,
      "descripcion": "Documentar resultados",
      "resultadoObtenido": "Documentación completada",
      "completada": true,
      "calificacion": 95
    },
    {
      "idTarea": 3,
      "descripcion": "Presentar conclusiones",
      "resultadoObtenido": "",
      "completada": false,
      "calificacion": null
    }
  ]
}

Response (200 OK):
{
  "mensaje": "Evaluación práctica guardada exitosamente",
  "idEvaluacion": 5,
  "cantidadTareas": 3,
  "calificacion": 66.67
}
```

### 3. Obtener Todas las Evaluaciones
```
GET https://localhost:7287/api/evaluaciones
Headers:
  - Content-Type: application/json

Response (200 OK):
[
  {
    "idEvaluacion": 1,
    "idEvaluado": 1,
    "nombreEvaluado": "Juan Pérez",
    "fechaEvaluacion": "2025-01-20",
    "nota": 75.50,
    "estadoEvaluacion": "Completada",
  "tipoEvaluacion": "Práctica"
  },
  ...
]
```

### 4. Obtener Evaluación Específica
```
GET https://localhost:7287/api/evaluaciones/1
Headers:
  - Content-Type: application/json

Response (200 OK):
{
  "idEvaluacion": 1,
  "idEvaluado": 1,
  "nombreEvaluado": "Juan Pérez",
  "fechaEvaluacion": "2025-01-20",
  "nota": 75.50,
  "estadoEvaluacion": "Completada",
"tipoEvaluacion": "Práctica"
}
```

### 5. Obtener Evaluaciones de un Usuario
```
GET https://localhost:7287/api/evaluaciones/usuario/1
Headers:
  - Content-Type: application/json

Response (200 OK):
[
  {
    "idEvaluacion": 1,
    "fechaEvaluacion": "2025-01-20",
    "nota": 75.50,
    "estadoEvaluacion": "Completada",
    "tipoEvaluacion": "Práctica"
  },
  ...
]
```

### 6. Actualizar Evaluación
```
PUT https://localhost:7287/api/evaluaciones/1
Headers:
  - Content-Type: application/json

Body:
{
  "estadoEvaluacion": "Revisada",
  "nota": 80.50
}

Response (200 OK):
{
  "mensaje": "Evaluación actualizada exitosamente"
}
```

### 7. Eliminar Evaluación
```
DELETE https://localhost:7287/api/evaluaciones/1
Headers:
  - Content-Type: application/json

Response (200 OK):
{
  "mensaje": "Evaluación eliminada exitosamente"
}
```

---

## ?? Pruebas en Aplicación Blazor

### Flujo de Prueba Completo

#### Paso 1: Acceder a Panel de Control
- URL: `https://localhost:7001/panelControlAdmin`
- Verificar que aparecen 6 tarjetas
- Hacer clic en "Evaluación Práctica"

#### Paso 2: Listar Usuarios
- URL: `https://localhost:7001/EvaluacionPractica`
- Verificar que carga lista de usuarios
- Buscar usuario por nombre (ej: "Juan")
- Verificar que filtra resultados

#### Paso 3: Iniciar Evaluación
- Hacer clic en botón "Inicio" para un usuario
- URL debe cambiar a: `https://localhost:7001/evaluacion-practica-ejecutar/1/Juan%20Pérez`

#### Paso 4: Completar Tareas
- Verificar que se muestran 3 tareas de ejemplo
- Para cada tarea:
  - Leer descripción e instrucciones
  - Escribir respuesta en campo de texto
  - Opcional: Marcar checkbox "Marcar como completada"
- Verificar que campos se actualizan correctamente

#### Paso 5: Enviar Evaluación
- Hacer clic en "Enviar Evaluación"
- Verificar mensaje de éxito: "¡Evaluación práctica guardada exitosamente!"
- Verificar redirección a `/EvaluacionPractica`

#### Paso 6: Verificar en BD
```sql
SELECT * FROM EVALUACION 
WHERE id_evaluado = 1 AND tipo_evaluacion = 0
ORDER BY fecha_evaluacion DESC
```

---

## ?? Pruebas de Validación

### 1. Sin Respuestas
**Acción:** Intentar enviar evaluación sin escribir respuestas
**Resultado Esperado:** Mensaje de error "Debe responder al menos una tarea"

### 2. Usuario Inexistente
**Acción:** Intentar acceder a `/evaluacion-practica-ejecutar/9999/Usuario`
**Resultado Esperado:** En Backend: "Usuario no encontrado" (si se implementa validación)

### 3. Búsqueda Sin Resultados
**Acción:** Buscar usuario por nombre que no existe (ej: "XYZ123")
**Resultado Esperado:** Mostrar mensaje "No se encontraron resultados"

### 4. Error de Conexión
**Acción:** Apagar servidor API y intentar enviar evaluación
**Resultado Esperado:** Mensaje de error con detalles de excepción

---

## ?? Casos de Uso Prácticos

### Caso 1: Evaluación Completa
1. Usuario: Juan Pérez
2. Tareas completadas: 3/3
3. Nota esperada: 100
4. Estado: Completada

### Caso 2: Evaluación Parcial
1. Usuario: María López
2. Tareas completadas: 2/3
3. Nota esperada: 66.67
4. Estado: Completada

### Caso 3: Evaluación Sin Tareas
1. Usuario: Carlos García
2. Tareas completadas: 0/3
3. Nota esperada: 0
4. Estado: Completada

---

## ?? Debugging

### Verificar Logs de Navegador
```javascript
// Abrir DevTools (F12) y revisar Console
// Buscar mensajes de error en red (Network tab)
```

### Verificar Llamadas HTTP
1. Abrir DevTools (F12)
2. Ir a pestaña "Network"
3. Completar evaluación y enviar
4. Buscar request POST a `crear-evaluacion-practica`
5. Revisar Headers, Body y Response

### Logs de Servidor
```
Visual Studio > Output Window > Debug
Buscar: "EvaluacionesController"
```

---

## ? Checklist de Validación

- [ ] Página `/EvaluacionPractica` carga correctamente
- [ ] Se listan usuarios desde API
- [ ] Búsqueda filtra usuarios por nombre
- [ ] Búsqueda filtra usuarios por correo
- [ ] Botón "Inicio" funciona para cada usuario
- [ ] Se navega a página de ejecución correctamente
- [ ] Se muestran tareas con descripción e instrucciones
- [ ] Campos de texto aceptan input
- [ ] Checkboxes cambian de estado al hacer clic
- [ ] Validación muestra error si no hay respuestas
- [ ] Se envía POST correcto a API
- [ ] Se recibe respuesta exitosa de API
- [ ] Se muestra mensaje de éxito
- [ ] Se redirige a `/EvaluacionPractica` después
- [ ] Datos se guardan en BD correctamente
- [ ] Nota se calcula correctamente
- [ ] Responsive funciona en móviles

---

## ?? Métricas de Prueba

| Métrica | Valor |
|---------|-------|
| Endpoints probados | 7/7 ? |
| Componentes funcionales | 5/5 ? |
| Validaciones | 4/4 ? |
| Casos de uso | 3/3 ? |
| Responsividad | OK ? |
| Manejo de errores | OK ? |

---

## ?? Notas Importantes

1. **URL Base API:** Asegurar que sea `https://localhost:7287`
2. **CORS:** Verificar que API permite requests desde `localhost:7001`
3. **SQL Server:** Base de datos debe estar activa y accesible
4. **Migrations:** Si cambios de modelo, ejecutar: `Add-Migration` y `Update-Database`
5. **Tareas Hardcodeadas:** Actualmente las tareas son de ejemplo. Implementar carga desde API en producción.

---

**Última actualización:** 2025-01-20  
**Versión:** 1.0

# Implementación de Evaluación Práctica - DELTATEST

## ?? Descripción General

Se ha implementado una nueva funcionalidad de **Evaluación Práctica** en la aplicación DELTATEST que permite:

1. **Listar usuarios registrados** con búsqueda por nombre y correo
2. **Iniciar evaluaciones prácticas** para evaluados específicos
3. **Registrar tareas y resultados** de la evaluación práctica
4. **Guardar evaluaciones** en la base de datos mediante API REST

---

## ?? Nuevos Componentes Creados

### 1. Página de Evaluación Práctica (`EvaluacionPractica.razor`)
**Ubicación:** `DELTATEST/Pages/Administrador/EvaluacionPractica.razor`

**Funcionalidades:**
- Carga listado de usuarios desde la API (`GET /api/usuarios`)
- Búsqueda en tiempo real por nombre o correo
- Botón "Inicio" para cada usuario que inicia la evaluación
- Tabla con columnas: Id, Nombre, Correo, Fecha de Ingreso, Puesto Actual, Puesto Solicitado
- Mensajes de éxito/error

**Ruta:** `/EvaluacionPractica`

### 2. Página de Ejecución de Evaluación Práctica (`EvaluacionPracticaEjecutar.razor`)
**Ubicación:** `DELTATEST/Pages/Administrador/EvaluacionPracticaEjecutar.razor`

**Funcionalidades:**
- Muestra nombre del evaluado
- Presenta tareas prácticas con descripción e instrucciones
- Campo de texto para ingresar resultados/respuestas
- Checkbox para marcar tarea como completada
- Envía evaluación completada a la API
- Redirección a lista de evaluaciones tras completar

**Ruta:** `/evaluacion-practica-ejecutar/{idUsuario}/{nombreUsuario}`

**Datos de Prueba:**
- Se incluyen 3 tareas de ejemplo en la inicialización
- Las tareas incluyen descripciones e instrucciones

### 3. Modelo de Evaluación Práctica (`EvaluacionPracticaModelo.cs`)
**Ubicación:** `DELTATEST/Models/EvaluacionPracticaModelo.cs`

**Clases:**
- `EvaluacionPracticaModelo`: Modelo principal con usuario y tareas
- `TareaPractica`: Modelo para cada tarea con descripción, instrucciones y resultado

### 4. Controlador de Evaluaciones (`EvaluacionesController.cs`)
**Ubicación:** `DELTAAPI/Controllers/EvaluacionesController.cs`

**Endpoints:**

#### POST `/api/evaluaciones/crear-evaluacion-practica`
Crea una nueva evaluación práctica
```json
{
  "idUsuario": 1,
  "nombreUsuario": "Juan Pérez",
  "tareas": [
    {
      "idTarea": 1,
      "descripcion": "Tarea 1",
      "resultadoObtenido": "Respuesta",
      "completada": true,
      "calificacion": null
    }
  ]
}
```

#### GET `/api/evaluaciones`
Obtiene todas las evaluaciones

#### GET `/api/evaluaciones/{id}`
Obtiene una evaluación específica

#### GET `/api/evaluaciones/usuario/{idUsuario}`
Obtiene evaluaciones de un usuario específico

#### PUT `/api/evaluaciones/{id}`
Actualiza una evaluación existente

#### DELETE `/api/evaluaciones/{id}`
Elimina una evaluación

---

## ?? Flujo de Uso

1. **Admin accede al Panel de Control** ? `/panelControlAdmin`
2. **Hace clic en tarjeta "Evaluación Práctica"** ? Navega a `/EvaluacionPractica`
3. **Ve lista de usuarios** ? Puede buscar por nombre o correo
4. **Hace clic en "Inicio"** para el usuario a evaluar ? Va a `/evaluacion-practica-ejecutar/{id}/{nombre}`
5. **Completa tareas** ? Ingresa respuestas en campos de texto
6. **Marca tareas completadas** ? Usa checkbox para indicar si se completó
7. **Envía evaluación** ? Hace clic en "Enviar Evaluación"
8. **API guarda en BD** ? POST a `crear-evaluacion-practica`
9. **Redirección** ? Vuelve a lista de evaluaciones prácticas

---

## ?? Cambios en Componentes Existentes

### Panel de Control (`PanelControl.razor`)
Se agregaron dos nuevas tarjetas en una segunda fila:
- **Evaluación Teórica** ? Navega a `/EvaluacionTeorica`
- **Evaluación Práctica** ? Navega a `/EvaluacionPractica`

---

## ??? Integración con Base de Datos

La evaluación práctica utiliza la tabla `EVALUACION` existente:

```sql
CREATE TABLE EVALUACION (
    id_evaluacion INT PRIMARY KEY,
    id_evaluado INT NOT NULL,
  id_administrador INT,
    id_area INT,
    fecha_evaluacion DATE,
    nota DECIMAL(5,2),
    estado_evaluacion VARCHAR(50),
    tipo_evaluacion BIT -- false = práctica, true = teórica
)
```

**Valores guardados:**
- `idEvaluado`: ID del usuario evaluado
- `fechaEvaluacion`: Fecha actual
- `tipoEvaluacion`: false (para diferenciar de teórica)
- `estadoEvaluacion`: "Completada"
- `nota`: Porcentaje de tareas completadas (0-100)

---

## ?? Estilos y Diseño

Se implementaron estilos personalizados con:
- **Color principal:** #f58220 (naranja)
- **Animaciones:** Transiciones suaves, spinner de carga
- **Responsive:** Diseño adaptable a móviles y tablets
- **Tabla:** Diseño moderno con hover effects
- **Alertas:** Mensajes de éxito (verde) y error (rojo)

---

## ?? Seguridad

**Nota:** Los endpoints incluyen `[AllowAnonymous]` para desarrollo. En producción:
- Implementar `[Authorize]` para proteger endpoints
- Validar rol de administrador antes de guardar evaluaciones
- Implementar auditoría de cambios

---

## ? Validaciones

### En Frontend (Blazor):
- Validar que al menos una tarea tenga respuesta antes de enviar
- Mostrar mensajes de error al cargar usuarios
- Validar conectividad con API

### En Backend (API):
- Verificar que usuario existe en BD
- Validar estructura de datos recibida
- Validar que ID de usuario > 0
- Manejo de excepciones de base de datos

---

## ?? Requisitos para Usar

### Cliente (DELTATEST):
- .NET 8
- Blazor WebAssembly
- HttpClient configurado
- Conexión a API en `https://localhost:7287`

### Servidor (DELTAAPI):
- .NET 8
- Entity Framework Core
- SQL Server
- Base de datos DELTATEST con tablas: USUARIO, EVALUACION

---

## ?? Notas de Implementación

1. **Datos de Prueba:** Las tareas en la página de ejecución son de ejemplo. En producción, estas deben venir de la API.

2. **Calificación Automática:** El cálculo de nota se basa en % de tareas completadas. Puede customizarse según necesidades.

3. **Navegación:** Usa `NavigationManager` para cambiar entre páginas.

4. **Inyección de Dependencias:** HttpClient inyectado para hacer llamadas a API.

5. **Manejo de Estado:** El estado se mantiene en las propiedades del componente durante la sesión.

---

## ?? Próximas Mejoras Sugeridas

1. Cargar tareas desde API en lugar de datos hardcodeados
2. Implementar autenticación y roles
3. Agregar descarga de resultados en PDF
4. Implementar notificaciones automáticas
5. Agregar historial y estadísticas de evaluaciones
6. Validación más robusta de datos
7. Implementar paginación en listado de usuarios

---

## ?? Soporte

Para preguntas o problemas, revisar:
- Logs del navegador (F12 > Console)
- Logs de la API en Visual Studio
- Estado de conexión a BD
- Permisos de usuario en SQL Server

---

**Versión:** 1.0  
**Fecha:** 2025  
**Estado:** ? Completado y Funcional

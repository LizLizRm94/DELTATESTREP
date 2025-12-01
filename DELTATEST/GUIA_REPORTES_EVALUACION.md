# GUÍA: GENERADOR DE REPORTES DE EVALUACIÓN

## Descripción General
Se ha implementado un sistema completo de generación de reportes de evaluación con capacidad de impresión y descarga en PDF. El reporte incluye:
- Datos del evaluado (nombre, CI)
- Resultado de la evaluación (nota)
- Tipo de evaluación (Teórica/Práctica)
- Fecha de evaluación
- Nombre del evaluador/administrador
- Estado de la evaluación
- Diseño profesional imprimible

---

## CAMBIOS REALIZADOS

### 1. **API Backend** (DELTAAPI)

#### AuthController.cs
- Sin cambios (ya tenía los datos necesarios)

#### EvaluacionesController.cs
**Actualización - GetEvaluacionById:**
- Ahora incluye `Include(e => e.IdAdministradorNavigation)` para traer datos del evaluador
- Devuelve `NombreAdministrador` en la respuesta

**Actualización - GetEvaluacionesByUsuario:**
- Incluye información del administrador que realizó la evaluación
- Devuelve `NombreAdministrador` junto con otros datos

---

### 2. **Frontend Blazor** (DELTATEST)

#### Servicios Creados

**ReporteEvaluacionService.cs** (Nuevo)
- `ObtenerEvaluacionAsync(int idEvaluacion)` - Obtiene datos de la evaluación del API
- `GenerarHtmlReporte(DetalleEvaluacionDto evaluacion)` - Genera HTML del reporte con estilos CSS profesionales
- Incluye DTO: `DetalleEvaluacionDto` con todos los datos necesarios

#### Páginas Creadas

**ReporteEvaluacion.razor** (Nuevo)
- Ruta: `/reporte-evaluacion/{IdEvaluacion:int}`
- Carga los datos de la evaluación
- Genera y muestra el reporte en HTML
- Incluye botones para:
  - **Descargar/Imprimir**: Usa `window.print()` para imprimir o guardar como PDF
  - **Volver**: Regresa a la página anterior

**VerEvaluacionesReportes.razor** (Nuevo)
- Ruta: `/verEvaluacionesReportes`
- Lista todas las evaluaciones del sistema
- Búsqueda por nombre de evaluado o evaluador
- Cada evaluación tiene dos botones:
  - **Reporte**: Abre el reporte en una nueva vista
  - **Imprimir**: Abre el reporte con intención de impresión

**ListaEvaluacionesUsuario.razor** (Nuevo)
- Ruta: `/lista-evaluaciones-usuario/{IdUsuario:int}`
- Muestra evaluaciones de un usuario específico
- Permite ver reportes individuales

#### Panel de Control Actualizado

**PanelControl.razor**
- Agregado método `IrAVerEvaluaciones()` para navegar a reportes
- Botón "Ver Evaluación" ahora lleva a la página de reportes
- Saludo de bienvenida al administrador ya incluido en la edición anterior

#### Program.cs
- Registrado `ReporteEvaluacionService` en dependency injection

---

## CARACTERÍSTICAS DEL REPORTE

### Diseño Visual
- Encabezado profesional con logo de DELTATEST
- Sección de resultado destacada con color naranja (#f58220)
- Nota grande y visible (48px)
- Badge de estado (APROBADO/DESAPROBADO) con colores dinámicos
- Grid de información estructurado
- Pie de página con timestamp

### Contenido
- **Resultado Final**: Nota sobre 100
- **Información del Evaluado**: Nombre completo, CI
- **Detalles de la Evaluación**: 
  - Fecha
  - Estado
  - Evaluador
  - ID de Evaluación

### Funcionalidades
- ? Optimizado para impresión (CSS @media print)
- ? Responsive (se adapta a diferentes tamaños de pantalla)
- ? Descargable como PDF (mediante Imprimir > Guardar como PDF)
- ? Imprimible directamente
- ? Búsqueda de evaluaciones
- ? Filtrado por nombre

---

## CÓMO USAR

### Para Ver un Reporte Específico

1. Desde el **Panel de Control de Administrador**
2. Click en **"Ver Evaluación"**
3. Se abre la página de evaluaciones con todas las evaluaciones
4. Click en **"Reporte"** para ver la evaluación específica
5. Automáticamente abre el reporte formateado

### Para Descargar el Reporte como PDF

1. En la página del reporte, click en **"Descargar/Imprimir"**
2. Se abre el diálogo de impresión del navegador
3. Cambiar destino a **"Guardar como PDF"**
4. Click en **"Guardar"**
5. Se descarga el reporte como archivo PDF

### Para Imprimir el Reporte

1. En la página del reporte, click en **"Descargar/Imprimir"**
2. Se abre el diálogo de impresión
3. Seleccionar impresora
4. Click en **"Imprimir"**

### Para Buscar Evaluaciones

1. En la página de reportes (`/verEvaluacionesReportes`)
2. Usar la barra de búsqueda
3. Buscar por:
   - Nombre del evaluado
   - Nombre del evaluador/administrador
4. Los resultados se filtran en tiempo real

---

## URLS DISPONIBLES

| Ruta | Descripción |
|------|-------------|
| `/reporte-evaluacion/{id}` | Ver reporte de una evaluación específica |
| `/verEvaluacionesReportes` | Ver todas las evaluaciones y acceder a reportes |
| `/lista-evaluaciones-usuario/{idUsuario}` | Ver evaluaciones de un usuario específico |

---

## DATOS INCLUIDOS EN EL REPORTE

| Campo | Fuente | Ejemplo |
|-------|--------|---------|
| ID Evaluación | Evaluacion.IdEvaluacion | #001 |
| Nombre Evaluado | Usuario.NombreCompleto | Juan Pérez García |
| CI Evaluado | Usuario.Ci | 12345678 |
| Nota | Evaluacion.Nota | 85.50 |
| Tipo | Evaluacion.TipoEvaluacion | Práctica |
| Fecha | Evaluacion.FechaEvaluacion | 15/03/2025 |
| Evaluador | Usuario (IdAdministrador).NombreCompleto | María López |
| Estado | Evaluacion.EstadoEvaluacion | Completada |
| Estado Resultado | Calculado (Nota >= 80) | APROBADO |

---

## NOTAS TÉCNICAS

### Dependencias
- `ReporteEvaluacionService` - Genera HTML del reporte
- `HttpClient` - Obtiene datos del API
- `NavigationManager` - Navegación entre páginas
- CSS puro (sin librerías externas para generación de PDF)

### Alternativas de Mejora (Futuro)
- Integrar librería QR-Code para códigos de verificación
- Añadir firma digital del administrador
- Exportar directamente a Excel
- Envío de reporte por email
- Almacenamiento de reportes generados
- Gráficos de tendencia de evaluaciones

---

## TROUBLESHOOTING

### El reporte no carga
- Verificar que el ID de la evaluación existe en la BD
- Revisar que el API está corriendo en `https://localhost:7287/`
- Comprobar que el usuario evaluador tiene datos completos

### El PDF no se genera correctamente
- Usar navegadores modernos (Chrome, Edge, Firefox)
- No usar Internet Explorer
- Verificar que JavaScript está habilitado

### Datos incompletos en el reporte
- Asegurar que al crear la evaluación se asigna el `IdAdministrador`
- Verificar que el usuario administrador existe en la BD

---

## FLUJO DE DATOS

```
PanelControl.razor (Admin)
    ?
IrAVerEvaluaciones()
    ?
VerEvaluacionesReportes.razor
    ? (Selecciona una evaluación)
ReporteEvaluacion.razor
    ?
ReporteEvaluacionService.ObtenerEvaluacionAsync()
    ?
API: GET /api/evaluaciones/{id}
    ?
GenerarHtmlReporte()
    ?
Mostrar HTML con estilos
    ? (Usuario click en "Descargar/Imprimir")
window.print()
    ?
Diálogo del navegador (Imprimir o Guardar como PDF)
```

---

## Generado para DELTATEST
Sistema de Evaluación de Competencias
Fecha: 2025

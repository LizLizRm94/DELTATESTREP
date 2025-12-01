# ? INICIO RÁPIDO - GENERADOR DE REPORTES

## 1?? REQUISITOS PREVIOS
- ? API corriendo en `https://localhost:7287/`
- ? Blazor corriendo en `https://localhost:7071/` (o tu puerto)
- ? BD actualizada con evaluaciones
- ? Navegador moderno (Chrome, Edge, Firefox)

---

## 2?? FLUJO DE USUARIO

### Opción A: Desde Panel de Admin
```
Panel de Control 
    ?
Click: "Ver Evaluación"
    ?
Lista de todas las evaluaciones
    ?
Click: "Reporte" en la evaluación deseada
    ?
Ver reporte formateado
    ?
Click: "Descargar/Imprimir"
    ?
Elegir: Imprimir o Guardar como PDF
```

### Opción B: Acceso Directo
```
URL: https://localhost:7071/reporte-evaluacion/1
     (Reemplazar 1 con ID de la evaluación)
    ?
Ver reporte de evaluación #1
```

---

## 3?? CARACTERÍSTICAS

### ?? Datos en el Reporte
- **Nota**: 85.50 / 100
- **Evaluado**: Nombre y CI
- **Evaluador**: Quién realizó la evaluación
- **Fecha**: Cuándo se realizó
- **Estado**: Aprobado/Desaprobado
- **Tipo**: Teórica/Práctica

### ?? Diseño
- Profesional y moderno
- Responsive (se adapta a cualquier pantalla)
- Optimizado para impresión
- Colores corporativos (naranja #f58220)

### ?? Opciones de Descarga
- **PDF**: Guardar como PDF desde navegador
- **Imprimir**: Enviar a impresora
- **Búsqueda**: Buscar evaluaciones por nombre

---

## 4?? URLs IMPORTANTES

| URL | Descripción | Parámetro |
|-----|-------------|-----------|
| `/verEvaluacionesReportes` | Ver todas las evaluaciones | - |
| `/reporte-evaluacion/1` | Ver reporte de evaluación | ID evaluación |
| `/lista-evaluaciones-usuario/5` | Ver evaluaciones de usuario | ID usuario |

---

## 5?? BÚSQUEDA Y FILTRADO

En `/verEvaluacionesReportes`:
- **Búsqueda**: Digite nombre del evaluado o evaluador
- **Filtrado**: Automático mientras escribe
- **Resultados**: En tiempo real

Ejemplo:
```
Búsqueda: "Juan"
Resultado: Todas las evaluaciones de "Juan" (como evaluado o evaluador)
```

---

## 6?? DESCARGAR COMO PDF

### Paso a paso:
1. Abrir reporte (`/reporte-evaluacion/{id}`)
2. Click en **"Descargar/Imprimir"**
3. Se abre diálogo de impresión
4. Cambiar destino a **"Guardar como PDF"**
5. Elegir ubicación y nombre
6. Click en **"Guardar"**

### Resultado:
- Archivo PDF con nombre personalizado
- Listo para compartir por email
- Archivo de referencia

---

## 7?? IMPRIMIR DIRECTAMENTE

### Paso a paso:
1. Abrir reporte (`/reporte-evaluacion/{id}`)
2. Click en **"Descargar/Imprimir"**
3. Se abre diálogo de impresión
4. Seleccionar impresora
5. Configurar página (A4, portrait)
6. Click en **"Imprimir"**

### Recomendaciones:
- Márgenes: Normal (2.54 cm)
- Papel: A4 blanco
- Orientación: Vertical
- Encabezados: No necesarios

---

## 8?? DATOS CAPTURADOS

| Sección | Campos |
|---------|--------|
| **Resultado** | Nota, Tipo, Estado |
| **Evaluado** | Nombre Completo, CI |
| **Detalles** | Fecha, Estado, Evaluador, ID |
| **Footer** | Timestamp generación |

---

## 9?? POSIBLES ERRORES

### "Evaluación no encontrada"
- Verificar ID correcto en URL
- Verificar evaluación existe en BD

### "No hay evaluaciones"
- Crear evaluaciones primero
- Asegurar que tienen IdAdministrador

### PDF no se genera
- Usar navegador moderno
- Habilitar JavaScript
- Intentar con Chrome si es otro navegador

---

## ?? TIPS Y TRUCOS

### ?? Consejo 1: Búsqueda Rápida
- Presione `Ctrl+F` en el navegador
- Busque en la tabla de evaluaciones

### ?? Consejo 2: Múltiples Reportes
- Abra varios en pestañas
- Compare evaluaciones lado a lado

### ?? Consejo 3: Guardado Automático
- PDF se guarda en Descargas por defecto
- Renombre según necesidad

### ?? Consejo 4: Compartir
- Comparta enlace: `https://dominio.com/reporte-evaluacion/{id}`
- Requiere acceso a la aplicación

### ?? Consejo 5: Impresión en Duplex
- En diálogo de impresión
- Opción "Frente y dorso"
- Economiza papel

---

## ?? DATOS DE EJEMPLO

Para probar el sistema, use estos datos de ejemplo:

```
Evaluado:
  Nombre: Juan Carlos Pérez García
  CI: 12345678

Evaluador:
  Nombre: María Elena López Rodríguez

Evaluación:
  ID: 001
  Nota: 85.50
  Tipo: Práctica
  Fecha: 15/03/2025
  Estado: Completada
```

---

## ? CARACTERÍSTICAS ESPECIALES

### ?? Estado Automático
- Nota >= 80 ? **APROBADO** (verde)
- Nota < 80 ? **DESAPROBADO** (rojo)

### ?? Actualización Dinámica
- Datos se obtienen en tiempo real
- Cambios en BD se reflejan inmediatamente

### ?? Responsivo
- Desktop: Tabla completa
- Tablet: Tabla ajustada
- Mobile: Scroll horizontal

### ??? Optimizado para Impresión
- Sin botones en impresión
- Colores ajustados para tinta
- Tamaño de fuente legible

---

## ?? SOPORTE RÁPIDO

### ? ¿Cómo acceso al reporte?
Panel Admin ? Ver Evaluación ? Click Reporte

### ? ¿Cómo descargo como PDF?
Descargar/Imprimir ? Guardar como PDF

### ? ¿Puedo buscar evaluaciones?
Sí, barra de búsqueda en `/verEvaluacionesReportes`

### ? ¿Qué datos incluye?
Nota, evaluado, evaluador, fecha, estado

### ? ¿Es seguro imprimir?
Sí, datos de la evaluación solamente

---

## ?? MÁS INFORMACIÓN

**Guía Completa**: Consulte `GUIA_REPORTES_EVALUACION.md`

**Cambios Técnicos**: Consulte `CAMBIOS_IMPLEMENTADOS.md`

**Ejemplo Visual**: Abra `EJEMPLO_REPORTE.html` en navegador

---

## ? CHECKLIST DE VERIFICACIÓN

- [ ] API está corriendo
- [ ] Blazor está corriendo
- [ ] Hay evaluaciones en la BD
- [ ] Evaluaciones tienen IdAdministrador
- [ ] Navegador es moderno
- [ ] Certificado HTTPS válido
- [ ] JavaScript habilitado

---

**¡Ya estás listo para usar el generador de reportes! ??**

Cualquier duda, consulta la documentación completa.

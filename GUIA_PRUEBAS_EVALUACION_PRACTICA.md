# ?? Guía de Pruebas - Evaluación Práctica

## Pruebas Funcionales

### Prueba 1: Carga de Página ?
**Pasos:**
1. Navega a: `/EvaluacionPractica/1/Juan Pérez`
2. Observa que se cargan 10 tareas

**Resultado esperado:**
- ? Se muestra "Tarea 1 de 10"
- ? Barra de progreso al 10%
- ? Botón "Siguiente" está DESHABILITADO (sin calificación)

---

### Prueba 2: Calificación de Tarea ?
**Pasos:**
1. Haz clic en la estrella número 7
2. Escribe una descripción
3. Observa el estado del botón "Siguiente"

**Resultado esperado:**
- ? Las 7 primeras estrellas aparecen en naranja (#f58220)
- ? Botón "Siguiente" se HABILITA

---

### Prueba 3: Navegación Entre Tareas ?
**Pasos:**
1. Haz clic en "Siguiente" (después de calificar)
2. Verifica que avanza a tarea 2
3. Observa la barra de progreso

**Resultado esperado:**
- ? Muestra "Tarea 2 de 10"
- ? Barra de progreso al 20%
- ? Contenido anterior se guardó (puedes volver)
- ? Botón "Siguiente" está DESHABILITADO nuevamente

---

### Prueba 4: Validación de Todas las Tareas (CRÍTICO) ?
**Pasos:**
1. Califica las primeras 9 tareas con cualquier puntuación
2. Llega a la tarea 10 SIN calificarla
3. Haz clic en "Listo"

**Resultado esperado:**
- ? RECHAZA guardado
- ? Muestra alerta roja: `"Debe calificar todas las tareas. Faltan 1 por calificar."`
- ? Permanece en la página sin redirigir

---

### Prueba 5: Guardado Exitoso ?
**Pasos:**
1. Califica TODAS las 10 tareas (ejemplo: todas con 8 puntos)
2. Haz clic en "Listo"
3. Espera a que termine de guardar

**Resultado esperado:**
- ? Muestra alerta verde: `"¡Evaluación práctica guardada exitosamente! Puntuación: 80/100"`
- ? Después de 2 segundos, redirige a `/EvaluacionPracticaLista`
- ? En BD se guardó con `Nota = 80`

---

### Prueba 6: Cálculo de Puntuación ?
**Pasos:**
1. Califica así:
   - Tarea 1: 10 puntos
   - Tarea 2: 9 puntos
   - Tarea 3: 8 puntos
   - Tarea 4: 7 puntos
   - Tarea 5: 6 puntos
   - Tarea 6: 5 puntos
   - Tarea 7: 4 puntos
   - Tarea 8: 3 puntos
   - Tarea 9: 2 puntos
   - Tarea 10: 1 punto

2. Guarda

**Resultado esperado:**
- ? Suma: 10+9+8+7+6+5+4+3+2+1 = 55
- ? Muestra: `"Puntuación: 55/100"`
- ? En BD: `Nota = 55` (Reprobado según escala)

---

### Prueba 7: Escala de Calificación ?

**Caso 1 - Reprobado (0-50):**
- Califica todas con 5 puntos ? 5×10 = 50
- Resultado: `"Puntuación: 50/100"` ?

**Caso 2 - Bien (51-79):**
- Califica todas con 7 puntos ? 7×10 = 70
- Resultado: `"Puntuación: 70/100"` ??

**Caso 3 - Excelente (80-100):**
- Califica todas con 9 puntos ? 9×10 = 90
- Resultado: `"Puntuación: 90/100"` ?

---

### Prueba 8: Descripción Opcional ?
**Pasos:**
1. Califica una tarea sin escribir descripción
2. Avanza a siguiente tarea
3. Guarda la evaluación

**Resultado esperado:**
- ? Se permite guardar sin descripción
- ? El campo `ResultadoObtenido` puede ser null

---

### Prueba 9: Volver Atrás (Sin botón, pero con datos guardados) ?
**Pasos:**
1. Califica 5 tareas
2. Ve a tarea 10
3. Usa navegación del navegador o botón de volver
4. Vuelve a entrar a la evaluación

**Resultado esperado:**
- ? Los datos de las 5 tareas se mantienen en memoria de sesión
- ?? NOTA: Si recarga página, se reinicia la evaluación (es el comportamiento actual)

---

### Prueba 10: Responsive Design ??
**Pasos:**
1. Abre la página en dispositivo móvil o redimensiona a 375px
2. Intenta calificar
3. Intenta navegar

**Resultado esperado:**
- ? Estrellas se reducen de tamaño (40px × 40px)
- ? Botones se apilan verticalmente
- ? Barra de progreso sigue siendo visible
- ? Todo es usable sin scroll horizontal

---

## Pruebas de Base de Datos

### Verificar Guardado en BD ?
```sql
-- Ejecutar después de guardar una evaluación
SELECT 
    IdEvaluacion,
    IdEvaluado,
    Nota,
    TipoEvaluacion,
    FechaEvaluacion,
    EstadoEvaluacion
FROM EVALUACION
ORDER BY IdEvaluacion DESC
LIMIT 1;
```

**Resultado esperado:**
```
IdEvaluacion: 1
IdEvaluado: (el ID del usuario evaluado)
Nota: 87.00 (ejemplo)
TipoEvaluacion: 0 (false = Práctica)
FechaEvaluacion: (fecha actual)
EstadoEvaluacion: Completada
```

---

## Pruebas de Error

### Error 1: Usuario No Encontrado ?
**Pasos:**
1. Intenta guardar con un `IdUsuario` que no existe en BD

**Resultado esperado:**
- ? Backend retorna: `"Usuario no encontrado"`
- ? Frontend muestra alerta roja: `"Error al guardar: 404"`

---

### Error 2: Conexión Perdida ?
**Pasos:**
1. Apaga el servidor backend
2. Intenta calificar y guardar

**Resultado esperado:**
- ? Muestra alerta roja con el error de conexión
- ? Botón "Listo" queda sin estado de "Guardando..."

---

### Error 3: Modelo Inválido ?
**Pasos:**
1. Abre DevTools (F12)
2. Modifica el request para enviar calificaciones inválidas (>10)

**Resultado esperado:**
- ?? Backend debería validar pero actualmente no lo hace
- ?? Recomendación: Agregar validación en backend

---

## Checklist Final

- [ ] Todas las 10 tareas se cargan correctamente
- [ ] Barra de progreso avanza correctamente
- [ ] Validación de calificación actual funciona
- [ ] Validación de todas las tareas funciona
- [ ] Cálculo de suma es correcto
- [ ] Mensaje de éxito muestra puntuación correcta
- [ ] Redireccionamiento a lista funciona
- [ ] BD guarda correctamente con TipoEvaluacion = 0
- [ ] Diseño responsive funciona en móvil
- [ ] Descripción es opcional
- [ ] Botones están habilitados/deshabilitados correctamente

---

## Ambiente de Prueba Recomendado

**Frontend:**
```
URL: http://localhost:5173/EvaluacionPractica/1/Juan%20Pérez
```

**Backend API:**
```
POST http://localhost:7287/api/evaluaciones/crear-evaluacion-practica
```

**Base de Datos:**
```
Server: localhost
Database: DELTATEST (o tu BD)
```

---

## Notas de Debugging

Si encuentras problemas:

1. **Abre DevTools (F12)**
   - Network tab para ver requests
   - Console para errores JavaScript

2. **Revisa Network:**
   - POST a `/api/evaluaciones/crear-evaluacion-practica`
   - Payload debe incluir todas las tareas con calificación

3. **Revisa BD:**
   ```sql
   SELECT * FROM EVALUACION WHERE IdEvaluado = 1 ORDER BY IdEvaluacion DESC;
   ```

4. **Resetear Datos:**
   ```sql
   DELETE FROM EVALUACION WHERE IdEvaluado = 1;
   ```

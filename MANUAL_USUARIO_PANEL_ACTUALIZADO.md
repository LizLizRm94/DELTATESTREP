# ?? Manual de Usuario - Panel Actualizado

## ¿Qué Cambió?

El panel de usuario ahora muestra tu **Última Nota Entregada** en un cuadro grande y llamativo, en lugar de solo mostrar un promedio general.

---

## ?? Información que Ves

### El Cuadro Principal (Naranja)

```
??????????????????????????????????
?  Última Nota Entregada         ?
?                                ?
?           100                  ?
?          / 100                 ?
?                                ?
?  Fecha          ?   Estado     ?
?  01/12/2025     ? Calificado   ?
??????????????????????????????????
```

### Detalles de Cada Elemento

| Elemento | Significado | Ejemplo |
|----------|-------------|---------|
| **Título** | Indica que es tu evaluación más reciente | "Última Nota Entregada" |
| **Número Grande** | Tu puntaje en la última evaluación | 100 |
| **/ 100** | La nota está sobre 100 puntos | Siempre es "/ 100" |
| **Fecha** | Cuándo fue esa evaluación | 01/12/2025 |
| **Estado** | Si ya fue calificada o está pendiente | "Calificado" o "Pendiente" |

---

## ?? ¿Qué Evaluación Se Muestra?

El sistema prioriza así:

### 1?? **Evaluación Práctica Más Reciente**
Si tienes evaluaciones prácticas completadas y calificadas, muestra la más reciente.

```
Tienes:
• Práctica del 01/10/2025 ? 75 puntos
• Práctica del 15/11/2025 ? 85 puntos ? SE MUESTRA ESTA
• Teórica del 01/12/2025 ? Pendiente
```

### 2?? **Evaluación Teórica Más Reciente (si no hay práctica)**
Si no hay prácticas, busca la teórica más reciente que ya fue calificada.

```
Tienes:
• Teórica del 01/10/2025 ? 80 puntos
• Teórica del 15/11/2025 ? 90 puntos ? SE MUESTRA ESTA
```

### 3?? **Sin Cuadro (si no hay evaluaciones calificadas)**
Si no tienes ninguna evaluación aún calificada, el cuadro no aparece.

```
Tienes:
• Teórica del 01/10/2025 ? Respondida (Pendiente de calificación)
• Teórica del 15/11/2025 ? Respondida (Pendiente de calificación)

Resultado: NO SE MUESTRA EL CUADRO
```

---

## ?? Estados Posibles

### Estado: "Calificado"
? Tu evaluación ya fue revisada y tiene una nota
- Significa que el evaluador ya terminó de revisar tu evaluación
- Puedes ver tu desempeño
- La nota es definitiva

### Estado: "Pendiente"
? Tu evaluación fue entregada pero aún no tiene nota
- Significa que el evaluador aún no revisa tu evaluación
- Tu evaluación está en cola para calificar
- Espera a que sea calificada para ver tu nota

---

## ?? Botón "Ver Evaluaciones"

Debajo del cuadro naranja hay un botón azul que dice **"Ver Evaluaciones"**.

### ¿Qué Hace?
Te lleva a una página completa donde puedes:
- ? Ver todas tus evaluaciones (prácticas y teóricas)
- ?? Ver la nota de cada una
- ?? Ver la fecha de cada evaluación
- ?? Comparar tu progreso

### Cómo Usarlo
1. Haz clic en el botón azul "Ver Evaluaciones"
2. Se abre una nueva página
3. Scrollea hacia abajo para ver todas tus notas
4. Haz clic en el ícono del ojo para ver detalles de cada una

---

## ?? ¿Cuándo Se Actualiza?

El cuadro se actualiza automáticamente cuando:

1. ?? **El evaluador te califica una nueva evaluación**
   - Habría que actualizar la página (F5)
   
2. ?? **Completas una nueva evaluación**
   - Puede tardar en aparecer (hasta 24 horas)

3. ?? **Recargas la página**
   - Presiona F5 o Ctrl+Shift+R

---

## ?? Consejos

### Consejos para Interpretar tu Nota

```
Nota        ? Interpretación
??????????????????????????????????????????????????
80-100      ? ? Excelente - ¡Sigue así!
60-79       ? ?? Bueno - Hay espacio para mejorar
40-59       ? ?? Regular - Necesitas mejorar
Menor a 40  ? ? Bajo - Habla con tu supervisor
```

### Tips de Estudio

- ?? Revisa las evaluaciones anteriores para ver en qué fallaste
- ?? Enfócate en mejorar para la próxima evaluación
- ?? Habla con tu evaluador sobre áreas de mejora
- ?? Establece metas realistas para la siguiente evaluación

---

## ? Preguntas Frecuentes

### P: ¿Por qué no veo el cuadro naranja?

**Respuesta**: Hay varias razones:
1. No has completado ninguna evaluación aún
2. Completaste evaluaciones pero aún están pendientes de calificación
3. Es tu primer día en el sistema

**Solución**: Ve al botón "Ver Evaluaciones" para confirmarlo

### P: ¿Puedo ver mis evaluaciones antiguas?

**Respuesta**: ? Sí, haciendo clic en "Ver Evaluaciones" ves todas las que ya completaste.

### P: ¿Cómo sé si pasé?

**Respuesta**: La nota de aprobación mínima es **80 puntos**
- Si tu nota es 80 o más: ? Pasaste
- Si tu nota es menor a 80: ? Necesitas mejorar

### P: ¿Mi nota puede cambiar?

**Respuesta**: No, una vez que el evaluador la asigna es definitiva. Pero puedes:
- Intentar una nueva evaluación (diferente)
- Mejorar en futuras evaluaciones

### P: ¿Por qué veo una nota de Teórica y no de Práctica?

**Respuesta**: Porque:
1. Aún no has completado una evaluación práctica, O
2. Tu evaluación práctica más reciente aún no está calificada

En ese caso, el sistema te muestra tu teórica más reciente

### P: ¿Es normal que la fecha sea de hace varios meses?

**Respuesta**: ? Sí, es normal. Significa que esa fue tu última evaluación calificada. Las nuevas evaluaciones estarán pendientes hasta que el evaluador las calificaciones

---

## ?? Colores del Cuadro

El cuadro tiene colores especiales para llamar tu atención:

```
Naranja Intenso ? Color motivacional
?? Indica importancia
?? Resalta tu última nota
?? Fácil de encontrar
```

---

## ?? En Dispositivos Móviles

Si usas tu teléfono o tablet, el cuadro se adapta automáticamente:

```
Desktop                ? Mobile
??????????????????????? ???????????????
?  Nota   ? Notif    ?? ?  Nota       ?
?         ?          ?? ?             ?
? 100/100 ?          ?? ? 100 / 100   ?
?         ?          ?? ? Fecha|Est.  ?
??????????????????????? ???????????????
                      ? ???????????????
                      ? ? Notif       ?
                      ? ?             ?
                      ? ? • Notif 1   ?
                      ? ? • Notif 2   ?
                      ? ???????????????
```

---

## ?? Próximas Mejoras Esperadas

Pronto el panel podría mostrar:
- ?? Gráfico de progreso
- ?? Meta de mejora
- ?? Comparativa con promedio del equipo (anónimo)
- ?? Notificaciones cuando haya nuevas evaluaciones

---

## ?? Ayuda Adicional

### ¿Necesitas Ayuda?
- ?? Contacta a tu supervisor
- ?? Pregunta en el área de evaluaciones
- ?? Envía un email al departamento de recursos humanos

### ¿Encontraste un Error?
- ?? Reporta el problema
- ?? Toma una captura de pantalla
- ?? Describe lo que pasó

---

## ? Resumen Rápido

| Pregunta | Respuesta |
|----------|-----------|
| ¿Qué veo? | Tu última nota con fecha y estado |
| ¿Qué significa? | Tu evaluación más reciente |
| ¿Cómo puedo ver más? | Haz clic en "Ver Evaluaciones" |
| ¿Cuándo se actualiza? | Cuando hay una nueva evaluación calificada |
| ¿Qué nota aprueba? | 80 puntos o más |
| ¿Puedo ver otras notas? | Sí, en "Ver Evaluaciones" |


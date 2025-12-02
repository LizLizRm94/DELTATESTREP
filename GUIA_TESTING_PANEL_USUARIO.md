# ?? Guía de Testing - Panel Actualizado

## ?? Testing Manual

### Preparación

1. **Iniciar la aplicación**
   ```bash
   # Terminal 1: Backend
   cd DELTAAPI
   dotnet run
   
   # Terminal 2: Frontend
   cd DELTATEST
   dotnet run
   ```

2. **Abrir en navegador**
   ```
   https://localhost:7105
   ```

3. **Iniciar sesión** con un usuario existente
   ```
   Usuario: test@test.com (o CI)
   Contraseña: (tu contraseña)
   ```

---

## ? Test Cases

### Test 1: Ver Panel con Evaluación Práctica Reciente
**Precondiciones**:
- Usuario con al menos 1 evaluación práctica calificada
- ID del usuario: `{IdUsuario}`

**Pasos**:
1. Navegar a `/estado/{IdUsuario}`
2. Esperar a que cargue la página (máximo 3 segundos)
3. Observar el cuadro naranja

**Resultado Esperado** ?:
- [ ] Aparece cuadro naranja con gradiente
- [ ] Muestra "Última Nota Entregada"
- [ ] Muestra la nota numérica (ej: 85)
- [ ] Muestra "/ 100"
- [ ] Muestra la fecha correcta (formato dd/MM/yyyy)
- [ ] Muestra "Calificado" en estado
- [ ] Hay sombra (shadow-lg)

**Resultado Fallido** ?:
- [ ] Cuadro no aparece
- [ ] Números no son legibles
- [ ] Fecha en formato incorrecto
- [ ] Estado muestra otra cosa

---

### Test 2: Ver Panel sin Evaluación Práctica pero con Teórica
**Precondiciones**:
- Usuario SIN evaluación práctica
- Usuario CON evaluación teórica calificada

**Pasos**:
1. Navegar a `/estado/{IdUsuario}`
2. Esperar a que cargue
3. Observar el cuadro

**Resultado Esperado** ?:
- [ ] Aparece cuadro naranja (debería ser teórica más reciente)
- [ ] Muestra la nota de la teórica
- [ ] Muestra la fecha de la teórica
- [ ] Estado muestra "Calificado"

---

### Test 3: Ver Panel sin Evaluaciones Calificadas
**Precondiciones**:
- Usuario con evaluaciones "Respondidas" pero no calificadas
- O usuario sin evaluaciones

**Pasos**:
1. Navegar a `/estado/{IdUsuario}`
2. Esperar a que cargue
3. Observar si aparece el cuadro

**Resultado Esperado** ?:
- [ ] NO aparece el cuadro naranja
- [ ] Aparece solo la sección de notificaciones
- [ ] El botón "Ver Evaluaciones" sigue visible (aunque sin cuadro)

---

### Test 4: Botón "Ver Evaluaciones"
**Precondiciones**:
- Usuario debe tener evaluaciones

**Pasos**:
1. En el panel de usuario (`/estado/{IdUsuario}`)
2. Hacer clic en el botón azul "Ver Evaluaciones"
3. Esperar a que cargue la nueva página

**Resultado Esperado** ?:
- [ ] Se abre nueva página
- [ ] URL cambia a `/usuarios/evaluaciones/{IdUsuario}`
- [ ] Aparece lista de evaluaciones
- [ ] Se ve tabla de prácticas y teóricas

---

### Test 5: Responsividad Desktop (1920px)
**Dispositivo**: Computadora de escritorio

**Pasos**:
1. Abrir en navegador desktop
2. Ir a `/estado/{IdUsuario}`
3. Observar el layout

**Resultado Esperado** ?:
- [ ] Cuadro naranja ocupa máximo 420px (50% de pantalla)
- [ ] Notificaciones aparecen al lado (50% de pantalla)
- [ ] Ambas columnas tienen igual altura
- [ ] Espaciado es proporcional

```
???????????????????????????????????????????????
?   Última Nota (420px)    ? Notificaciones   ?
?                          ?                  ?
?  ??????????????????????  ? • Notif 1        ?
?  ?     100 / 100      ?  ? • Notif 2        ?
?  ?  Fecha | Estado    ?  ?                  ?
?  ??????????????????????  ?                  ?
?                          ?                  ?
???????????????????????????????????????????????
```

---

### Test 6: Responsividad Tablet (768px)
**Dispositivo**: iPad o similar

**Pasos**:
1. Abrir DevTools (F12)
2. Activar dispositivo tablet (768x1024)
3. Ir a `/estado/{IdUsuario}`

**Resultado Esperado** ?:
- [ ] Cuadro naranja se ajusta a 50% de ancho
- [ ] Notificaciones se mantienen al lado
- [ ] Texto es legible
- [ ] Botón está accesible

---

### Test 7: Responsividad Mobile (375px)
**Dispositivo**: iPhone o similar

**Pasos**:
1. Abrir DevTools (F12)
2. Activar dispositivo mobile (375x667)
3. Ir a `/estado/{IdUsuario}`

**Resultado Esperado** ?:
- [ ] Cuadro naranja ocupa 100% de ancho
- [ ] Notificaciones aparecen debajo
- [ ] Nota es legible en pantalla pequeña
- [ ] Botón es tocable (min 44x44px)
- [ ] No hay scroll horizontal

```
????????????????????????
? Última Nota (100%)   ?
?                      ?
?  ??????????????????  ?
?  ?   100 / 100    ?  ?
?  ? Fecha | Estado ?  ?
?  ??????????????????  ?
?                      ?
????????????????????????
? Notificaciones(100%) ?
?                      ?
? • Notif 1            ?
? • Notif 2            ?
?                      ?
????????????????????????
```

---

### Test 8: Rendimiento - Tiempo de Carga
**Objetivo**: Verificar que la página carga rápido

**Pasos**:
1. Abrir DevTools (F12 ? Network)
2. Ir a `/estado/{IdUsuario}`
3. Observar waterfall de requests

**Resultado Esperado** ?:
- [ ] Página visible en < 2 segundos
- [ ] Requests al API completan en < 3 segundos
- [ ] Máximo 4 requests simultáneos (usuarios, evals práctica, evals teórica, notificaciones)

**Tiempos Esperados**:
```
GET /api/usuarios/{id}                      ? <200ms
GET /api/evaluaciones/usuario/{id}          ? <300ms
GET /api/evaluacionesteoricass/usuario/{id} ? <300ms (si aplica)
GET /api/notificaciones/usuario/{id}        ? <200ms
?????????????????????????????????????????????
TOTAL                                       ? <1000ms
```

---

### Test 9: Actualización de Datos
**Objetivo**: Verificar que los datos se cargan correctamente

**Pasos**:
1. Abrir `/estado/{IdUsuario}`
2. Observar la nota mostrada
3. Verificar contra el API directamente
4. Hacer F5 (refresh)
5. Verificar que se actualiza

**Resultado Esperado** ?:
- [ ] Nota coincide con API
- [ ] Fecha coincide con API
- [ ] Después de F5, datos se actualizan
- [ ] Estado es correcto

**Verificar API**:
```bash
# Terminal
curl "https://localhost:7287/api/evaluaciones/usuario/1"

# Buscar el más reciente por FechaEvaluacion y comparar con nota mostrada
```

---

### Test 10: Manejo de Errores
**Objetivo**: Verificar que la app no crashea si falla un API

**Precondiciones**:
- Simular error del API

**Pasos**:
1. Abrir DevTools (F12 ? Network)
2. Ir a `/estado/{IdUsuario}`
3. En Network, throttle a "Offline"
4. F5 para recargar
5. Observar comportamiento

**Resultado Esperado** ?:
- [ ] Página no muestra errores rojo
- [ ] Console no tiene exceptions no manejadas
- [ ] Muestra mensaje amigable o se oculta el cuadro
- [ ] Notificaciones aún se intenta cargar
- [ ] UI no se congela

---

## ?? Testing Visual

### Test 11: Colores Correctos
**Pasos**:
1. Inspeccionar elemento del cuadro (F12)
2. Verificar estilos

**Resultado Esperado** ?:
```css
background: linear-gradient(135deg, #f58220 0%, #ff9c42 100%);
color: white;
box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
```

- [ ] Gradiente naranja visible
- [ ] Texto blanco legible
- [ ] Sombra visible

---

### Test 12: Tipografía
**Pasos**:
1. Inspeccionar elementos de texto
2. Verificar tamaños de fuente

**Resultado Esperado** ?:
- [ ] Título: 18px, font-weight: 600
- [ ] Nota grande: 72px, font-weight: bold
- [ ] "/ 100": 36px
- [ ] Labels: 12px
- [ ] Valores: 16px

---

## ?? Testing en Navegadores

### Test 13: Chrome
**Pasos**:
1. Abrir Chrome
2. Navegar a `/estado/{IdUsuario}`
3. Inspeccionar layout

**Resultado Esperado** ?:
- [ ] Carga correctamente
- [ ] Gradiente se ve bien
- [ ] Responsive funciona

### Test 14: Firefox
**Pasos**:
1. Abrir Firefox
2. Navegar a `/estado/{IdUsuario}`
3. Inspeccionar layout

**Resultado Esperado** ?:
- [ ] Carga correctamente
- [ ] Colores correctos
- [ ] Sin problemas de rendering

### Test 15: Safari
**Pasos**:
1. Abrir Safari (macOS)
2. Navegar a `/estado/{IdUsuario}`

**Resultado Esperado** ?:
- [ ] Carga correctamente
- [ ] Gradiente funciona
- [ ] Responsive OK

### Test 16: Edge
**Pasos**:
1. Abrir Edge
2. Navegar a `/estado/{IdUsuario}`

**Resultado Esperado** ?:
- [ ] Compatible
- [ ] Bootstrap funciona
- [ ] Sin warnings de seguridad

---

## ?? Testing de Casos Edge

### Test 17: Usuario con Nota 0
**Precondiciones**:
- Usuario con evaluación calificada en 0 puntos

**Resultado Esperado** ?:
- [ ] Muestra "0 / 100"
- [ ] No oculta el cuadro
- [ ] Estado: "Calificado"

### Test 18: Usuario con Nota NULL
**Precondiciones**:
- Evaluación existe pero Nota es NULL en DB

**Resultado Esperado** ?:
- [ ] Muestra "N/A"
- [ ] Estado: "Pendiente"

### Test 19: Usuario con Fecha Futura
**Precondiciones**:
- Por alguna razón hay evaluación con fecha futura

**Resultado Esperado** ?:
- [ ] Muestra la fecha correctamente
- [ ] No hay error

### Test 20: Usuario con Muchas Evaluaciones
**Precondiciones**:
- Usuario con 50+ evaluaciones

**Resultado Esperado** ?:
- [ ] Solo muestra la más reciente
- [ ] Carga rápido
- [ ] No hay lag

---

## ?? Testing de Seguridad

### Test 21: Acceso no Autenticado
**Pasos**:
1. Sin hacer login, ir directamente a `/estado/1`

**Resultado Esperado** ?:
- [ ] Redirige a login
- [ ] No muestra datos del usuario

### Test 22: Acceso a Otro Usuario
**Pasos**:
1. Logearse como usuario A
2. Ir a `/estado/{IdUsuarioB}`

**Resultado Esperado** ?:
- [ ] Puede ver la página (verificar requisitos de seguridad)
- [ ] O redirige a su propio estado
- [ ] Documento especifica el comportamiento

---

## ?? Testing de Base de Datos

### Test 23: Datos Correctos del DB
**Pasos**:
1. Consultar DB directamente
2. Comparar con UI

```sql
SELECT TOP 1 
    IdEvaluacion, 
    FechaEvaluacion, 
    NotaPractica
FROM Evaluacion
WHERE IdEvaluado = @IdUsuario
ORDER BY FechaEvaluacion DESC
```

**Resultado Esperado** ?:
- [ ] Nota coincide
- [ ] Fecha coincide
- [ ] Es el registro más reciente

---

## ?? Reporte de Testing

Use esta plantilla para reportar resultados:

```markdown
# Reporte de Testing - Panel Usuario

## Información
- Fecha: [fecha]
- Tester: [nombre]
- Navegador: [Chrome/Firefox/Safari/Edge]
- Versión: [versión]
- Dispositivo: [Desktop/Tablet/Mobile]

## Resultados

### Tests Exitosos ?
- [x] Test 1: Ver Panel con Evaluación
- [x] Test 2: Responsividad Desktop

### Tests Fallidos ?
- [ ] Test X: [Descripción del fallo]
  - Pasos para reproducir: [...]
  - Resultado esperado: [...]
  - Resultado actual: [...]
  - Captura: [URL de imagen]

### Bugs Encontrados
1. [Descripción del bug]
   - Severidad: Alta/Media/Baja
   - Reproducible: Siempre/A veces/Raro

## Conclusión
Status General: EXITOSO / FALLIDO
Listo para producción: SÍ / NO
```

---

## ? Checklist Final

Antes de considerar el cambio como completado:

- [ ] Todos los tests pasan
- [ ] Responsive design funciona
- [ ] Rendimiento aceptable (<2s)
- [ ] Sin errores en console
- [ ] Manejo de errores funciona
- [ ] Documentación completa
- [ ] Código comentado
- [ ] Compilación sin warnings
- [ ] Testing en 4+ navegadores
- [ ] Aprobado por QA
- [ ] Listo para deploy

---

## ?? Deployment

Una vez que todos los tests pasen:

```bash
# 1. Commit de cambios
git add .
git commit -m "feat: Panel usuario muestra última nota entregada"

# 2. Push a rama feature
git push origin feature/ultima-nota-panel

# 3. Crear Pull Request
# En GitHub ? Create Pull Request

# 4. Code Review
# Esperar aprobación de revisor

# 5. Merge a main
# Una vez aprobado

# 6. Deploy a producción
# Seguir procedimiento de deployment
```


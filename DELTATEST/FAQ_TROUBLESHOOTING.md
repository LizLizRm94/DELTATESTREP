# ? FAQ - PREGUNTAS FRECUENTES Y TROUBLESHOOTING

## ?? PREGUNTAS FRECUENTES

### P1: ¿Dónde accedo a los reportes?
**R:** Panel de Control ? "Ver Evaluación" ? Selecciona una ? "Reporte"

O directamente: `https://localhost:7071/verEvaluacionesReportes`

---

### P2: ¿Qué datos incluye el reporte?
**R:** 
- ? Nota de la evaluación (ej: 85.50/100)
- ? Nombre del evaluado
- ? CI del evaluado
- ? Nombre del evaluador/administrador
- ? Fecha de evaluación
- ? Tipo de evaluación (Teórica/Práctica)
- ? Estado de la evaluación
- ? Estado del resultado (Aprobado/Desaprobado)

---

### P3: ¿Cómo descargo el reporte en PDF?
**R:**
1. Abre el reporte
2. Click en "Descargar/Imprimir"
3. En el diálogo ? "Guardar como PDF"
4. Elige ubicación
5. Click "Guardar"

---

### P4: ¿Se puede imprimir directamente?
**R:** Sí, en el mismo botón "Descargar/Imprimir":
1. Click en el botón
2. Selecciona impresora
3. Configura páginas
4. Click "Imprimir"

---

### P5: ¿Cómo busco una evaluación específica?
**R:** En `/verEvaluacionesReportes`:
- Barra de búsqueda arriba de la tabla
- Busca por nombre del evaluado o evaluador
- Resultados filtran en tiempo real

---

### P6: ¿El reporte es editable?
**R:** No, es de solo lectura. Los datos se generan del sistema.

---

### P7: ¿Cuánto tiempo tarda en generar?
**R:** Instantáneo (menos de 1 segundo)

---

### P8: ¿Se pueden generar múltiples reportes?
**R:** Sí, sin límite. Abre en pestañas diferentes.

---

### P9: ¿El reporte incluye firma?
**R:** No en esta versión. Se sugiere agregar después.

---

### P10: ¿Puedo compartir el enlace del reporte?
**R:** Sí, pero quien acceda necesita tener acceso a la aplicación.

---

## ?? TROUBLESHOOTING

### ? ERROR: "No hay evaluaciones"

**Causa posible:** No existen evaluaciones en la base de datos

**Solución:**
1. Crear una evaluación primero
2. Ir a "Crear Evaluación" en panel admin
3. Completar todos los campos
4. Guardar
5. Volver a /verEvaluacionesReportes

---

### ? ERROR: "Evaluación no encontrada"

**Causa posible:** ID de evaluación no existe o está mal escrito

**Solución:**
1. Verificar ID en URL: `/reporte-evaluacion/123`
2. Asegurar que evaluación existe en BD
3. Usar tabla de evaluaciones para obtener ID correcto

---

### ? ERROR: "No se carga el reporte"

**Causas posibles:**
- API no está corriendo
- URL del API es incorrecta
- Problema de conexión

**Solución:**
1. Verificar API está ejecutándose (puerto 7287)
2. Verificar URL en Program.cs: `https://localhost:7287/`
3. Revisar consola del navegador (F12)
4. Reiniciar API y navegador

---

### ? ERROR: "Spinner infinito"

**Causa posible:** La solicitud al API se congela

**Solución:**
1. Abrir DevTools (F12)
2. Pestaña Network
3. Verificar requests al API
4. Si están en rojo, problema de conexión
5. Reiniciar API

**Verificación:**
```
Consola ? Buscar errores CORS
Si hay CORS: Verificar configuración del API
```

---

### ? ERROR: "PDF no se descarga"

**Causa posible:** Navegador no soporta o está bloqueado

**Solución:**
1. Usar navegador moderno: Chrome, Edge, Firefox
2. NO usar Internet Explorer
3. Verificar que JavaScript esté habilitado
4. Permitir descargas en configuración del navegador
5. Probar con navegador privado/incógnito

---

### ? ERROR: "Datos incompletos en reporte"

**Causa posible:** Campos nulos en base de datos

**Solución:**
1. Verificar que evaluación tiene `IdAdministrador`
2. Verificar que usuario administrador existe
3. Re-crear evaluación con datos completos

**Verificación en BD:**
```sql
SELECT e.IdEvaluacion, e.IdAdministrador, u.NombreCompleto
FROM Evaluacion e
LEFT JOIN Usuario u ON e.IdAdministrador = u.IdUsuario
WHERE e.IdEvaluacion = 1
```

---

### ? ERROR: "Tabla vacía en VerEvaluacionesReportes"

**Causa posible:** Evaluaciones no tienen relación correcta

**Solución:**
1. Revisar que `IdEvaluado` existe en tabla Usuario
2. Revisar que `IdAdministrador` existe (puede ser NULL)
3. Ejecutar:
```sql
SELECT * FROM Evaluacion
WHERE IdEvaluado IN (SELECT IdUsuario FROM Usuario)
```

---

### ? ERROR: "Caracteres raros en reporte"

**Causa posible:** Problema de encoding

**Solución:**
1. Verificar encoding en HTML: `<meta charset='UTF-8'>`
2. Verificar BD está en UTF-8
3. Verificar API devuelve UTF-8

---

### ? ERROR: "No aparece evaluador"

**Causa posible:** `IdAdministrador` es NULL

**Solución:**
1. Asignar administrador al crear evaluación
2. Actualizar evaluaciones existentes:
```sql
UPDATE Evaluacion SET IdAdministrador = 1 WHERE IdAdministrador IS NULL
```

---

### ? ERROR: "Se abre en blanco"

**Causa posible:** Error en el servicio

**Solución:**
1. Abrir DevTools (F12)
2. Consola del navegador
3. Ver error específico
4. Verificar:
   - ID de evaluación válido
   - API respondiendo
   - DTO mapeado correctamente

---

### ? ERROR: "Botón Imprimir no funciona"

**Causa posible:** Script de impresión deshabilitado

**Solución:**
1. Verificar navegador permite `window.print()`
2. Habilitar JavaScript
3. Probar con otro navegador
4. Usar atajo: `Ctrl+P` o `Cmd+P`

---

### ? ERROR: "CORS error"

**Respuesta típica:**
```
Access to XMLHttpRequest at 'https://localhost:7287/api/...'
has been blocked by CORS policy
```

**Solución en API (Startup.cs o Program.cs):**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.WithOrigins("https://localhost:7071")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

app.UseCors("AllowBlazor");
```

---

## ?? VERIFICACIONES DE CONFIGURACIÓN

### ? Verificar API está corriendo
```powershell
# En terminal
curl https://localhost:7287/api/evaluaciones
# Debe devolver JSON, no error
```

### ? Verificar Blazor está corriendo
```powershell
# En navegador
https://localhost:7071/
# Debe cargar la aplicación
```

### ? Verificar conectividad
```powershell
# En PowerShell
Test-NetConnection localhost -Port 7287
# Status: Success
```

### ? Verificar BD
```sql
SELECT COUNT(*) as EvaluacionesTotal FROM Evaluacion
-- Debe devolver > 0 si hay evaluaciones
```

---

## ?? SOLUCIONES AVANZADAS

### Limpiar Caché del Navegador
```
1. Press F12 (DevTools)
2. Consola
3. Copiar: localStorage.clear();
4. Enter
5. Recargar página (Ctrl+R)
```

### Forzar Recarga Completa
```
Ctrl+Shift+R (Windows/Linux)
Cmd+Shift+R (Mac)
```

### Ver Logs del API
```csharp
// En Program.cs
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// En consola del API verás:
[Debug] Request to api/evaluaciones/1
[Debug] Response: 200 OK
```

### Verificar Certificado SSL
```powershell
# Si da error de certificado
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

---

## ?? CHECKLIST DE DIAGNÓSTICO

Cuando algo no funciona, verifica:

- [ ] ¿API está corriendo en puerto 7287?
- [ ] ¿Blazor está corriendo en puerto 7071?
- [ ] ¿Hay evaluaciones en la BD?
- [ ] ¿Las evaluaciones tienen IdAdministrador?
- [ ] ¿El navegador es moderno?
- [ ] ¿JavaScript está habilitado?
- [ ] ¿HTTPS está funcionando?
- [ ] ¿LocalStorage está habilitado?
- [ ] ¿No hay filtros/adblockers?
- [ ] ¿La consola del navegador no tiene errores?

---

## ?? INFORMACIÓN DE CONTACTO

Si el problema persiste:

1. **Verificar Logs:**
   - Consola del navegador (F12 ? Console)
   - Consola del API (output window)

2. **Enviar Información:**
   - Mensaje de error exacto
   - ID de evaluación problemática
   - Navegador y versión
   - Pasos para reproducir

3. **Archivos de Referencia:**
   - `GUIA_REPORTES_EVALUACION.md` - Documentación completa
   - `CAMBIOS_IMPLEMENTADOS.md` - Cambios técnicos
   - `INICIO_RAPIDO.md` - Guía rápida

---

## ?? RECURSOS ÚTILES

### Documentación
- [Blazor Oficial](https://docs.microsoft.com/en-us/aspnet/core/blazor)
- [ASP.NET Core API](https://docs.microsoft.com/en-us/aspnet/core)
- [MDN Web Docs](https://developer.mozilla.org)

### Herramientas Recomendadas
- **DevTools**: F12 en cualquier navegador
- **Postman**: Prueba API endpoints
- **SQL Server Management Studio**: Revisa BD

### Accesos Directos
- Panel Admin: `/panelControlAdmin`
- Evaluaciones: `/verEvaluacionesReportes`
- Reporte: `/reporte-evaluacion/1`

---

## ? VALIDACIÓN FINAL

Después de resolver el problema:

1. Recargar página (Ctrl+R)
2. Limpiar caché (Ctrl+Shift+Delete)
3. Cerrar y abrir navegador
4. Probar en navegador privado/incógnito
5. Probar en otro navegador

---

**¡Esperamos haberte ayudado! Si tienes más preguntas, consulta la documentación.** ??


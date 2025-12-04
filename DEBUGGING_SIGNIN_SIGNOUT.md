# Debugging: Sign In/Sign Out Issue

## Problema
El botón sigue mostrando "Sign In" aunque el usuario esté autenticado y se vea "Bienvenido admin".

## Cambios Realizados

### 1. **Mejorado AuthService.GetCurrentUserAsync()**
- Mejor manejo de valores booleanos en localStorage
- Logging más detallado con prefijo `?` y `?`
- Fallback más robusto en caso de excepciones

### 2. **Agregado delay en MainLayout**
- Espera 100ms en `OnInitializedAsync` para que localStorage se inicialice
- Asegura que Blazored.LocalStorage esté listo antes de leer datos

### 3. **Agregado delay en IniciarSesion.razor**
- Espera 500ms después del login antes de hacer forceLoad
- Permite que localStorage se sincronice completamente

### 4. **Agregado script de debug: auth-debug.js**
- Disponible en la consola del navegador
- Comandos útiles:
  - `authDebug.checkAuth()` - Ver estado actual
  - `authDebug.simulateLogin('Nombre', 'Inspector', '1')` - Simular login
  - `authDebug.clearAuth()` - Limpiar datos
  - `authDebug.monitorStorage()` - Monitorear cambios en localStorage

## Cómo Debuggear

### En Google Chrome/Edge DevTools:

1. **Abrir la consola**: F12 ? Pestaña "Console"

2. **Verificar estado actual**:
   ```javascript
   authDebug.checkAuth()
   ```
   Debería mostrar:
   ```
   === AUTH DEBUG ===
   isAuthenticated: true
   userName: [Tu nombre]
   userRole: Inspector
   userId: [Tu ID]
   ==================
   ```

3. **Hacer login normalmente** y verificar que localStorage se actualiza:
   - Abre DevTools
   - Pestaña "Application" ? "Local Storage"
   - Verifica que aparecen las claves: `isAuthenticated`, `userName`, `userRole`, `userId`

4. **Verificar logs de consola**:
   Después del login, busca mensajes como:
   ```
   ? Returning authenticated user from localStorage: [Tu nombre]
   [MainLayout] Auth Status: true, User: [Tu nombre]
   ```

### Escenarios de Test:

**Escenario 1: Login exitoso**
- [ ] Loguearse con credenciales válidas
- [ ] Verificar localStorage en DevTools
- [ ] Debería redirigir a `/panelControlAdmin` o `/estado/{id}`
- [ ] Debería mostrar "Sign Out" en la navbar

**Escenario 2: Reload de página**
- [ ] Loguearse
- [ ] Presionar F5 para recargar
- [ ] Debería seguir mostrando "Sign Out"
- [ ] Verificar que localStorage persiste

**Escenario 3: Logout**
- [ ] Click en "Sign Out"
- [ ] Verificar que localStorage se borra
- [ ] Debería redirigir a home
- [ ] Debería mostrar "Sign In"

## Logs Importantes a Buscar

```
GetCurrentUserAsync - isAuthenticated flag from localStorage: true
Auth found in localStorage: [Tu nombre], Role: [Tu rol]
? Returning authenticated user from localStorage: [Tu nombre]
[MainLayout] Auth Status: true, User: [Tu nombre], Role: [Tu rol]
```

## Si Aún No Funciona

### Opción 1: Forzar un Reload Manual
```javascript
// En la consola:
location.reload(true);
```

### Opción 2: Simular el Login
```javascript
// En la consola (reemplaza con valores reales):
authDebug.simulateLogin('Tu Nombre', 'Inspector', '1');
location.reload(true);
```

### Opción 3: Verificar Base de Datos
- Asegurate que el usuario existe en la tabla `Usuarios`
- Verifica que el `Rol` está correcto (exactamente "Inspector" o "Usuario")
- Verifica que el usuario está `Activo`

## Información de Configuración

**Endpoints:**
- API Base: `https://localhost:7287/`
- Login: `POST /api/auth/login`
- Current User: `GET /api/auth/current-user`
- Logout: `POST /api/auth/logout`

**LocalStorage Keys:**
- `isAuthenticated` - Boolean (true/false)
- `userName` - String (nombre completo)
- `userRole` - String ("Inspector" o "Usuario")
- `userId` - Number (ID del usuario)

## Stack de Autenticación

```
IniciarSesion.razor
    ?
AuthService.LoginAsync()
    ? (Guarda en localStorage)
localStorage: {isAuthenticated, userName, userRole, userId}
    ?
forceLoad: true (Reload de página)
    ?
MainLayout.OnInitializedAsync()
    ? (Espera 100ms)
AuthService.GetCurrentUserAsync()
    ? (Lee de localStorage)
isAuthenticated = true
    ?
MainLayout muestra "Sign Out"
```

## Próximas Acciones

Si después de estos cambios sigue sin funcionar, reporta:
1. Mensaje exacto en la consola del navegador
2. Contenido de localStorage (DevTools ? Application)
3. Estado esperado vs actual

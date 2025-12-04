# Fix: Login/Logout Cookie Handling in Blazor WebAssembly

## Problema Original
En los paneles de administrador y usuario, el botón de "Sign In" seguía apareciendo aunque el usuario estuviera autenticado. Debería mostrar "Sign Out" después de iniciar sesión.

## Causa Raíz
En Blazor WebAssembly, las **cookies HTTP no se envían automáticamente en peticiones cross-origin** debido a restricciones de seguridad del navegador. Esto causaba:

1. ? El login creaba correctamente la cookie en el servidor
2. ? Las peticiones subsecuentes NO incluían la cookie
3. ? El endpoint `/api/auth/current-user` no podía verificar la sesión
4. ? El `MainLayout` siempre recibía `isAuthenticated = false`

## Solución Implementada

### 1. **Jerarquía de Verificación de Autenticación**
Se modificó `AuthService.GetCurrentUserAsync()` para:
- **Primero**: Verificar el `localStorage` (más confiable en WebAssembly)
- **Luego**: Intentar validar con el servidor vía `/api/auth/current-user`
- **Fallback**: Usar datos cacheados si falla la conexión

```csharp
// Primero intenta localStorage (datos guardados después del login)
var isAuth = await _localStorage.GetItemAsync<bool>("isAuthenticated");
if (isAuth && !string.IsNullOrEmpty(userName))
{
    return (true, userName, userRole, userId);
}

// Si no está en localStorage, intenta validar con el servidor
var response = await _http.GetAsync("api/auth/current-user");
```

### 2. **Configuración de Credenciales en HttpClient**
El `AuthorizationMessageHandler` ya estaba configurado correctamente:
```csharp
request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
```

Esto asegura que las cookies se incluyan en las peticiones (cuando sea posible).

### 3. **CORS Correctamente Configurado**
La API ya tenía CORS configurado con:
```csharp
policy.AllowCredentials(); // Permite envío de cookies
```

## Cambios Realizados

### DELTATEST/Services/AuthService.cs
- Mejorado `GetCurrentUserAsync()` para verificar localStorage primero
- Mejor manejo de excepciones y fallback

### DELTATEST/Layout/MainLayout.razor
- Clarificada la lógica de redirección después del logout
- Mejorado el logging para debugging

### DELTAAPI/Controllers/AuthController.cs
- Mejorado `GetCurrentUser()` con mejor logging y manejo de errores
- Añadidos mensajes de debug más descriptivos

## Cómo Funciona Ahora

1. **Usuario inicia sesión**
   ```
   LoginAsync() ? Guarda datos en localStorage ? Redirige según rol
   ```

2. **Usuario navega o recarga página**
   ```
   MainLayout ? RefreshAuthStatus() 
   ? AuthService.GetCurrentUserAsync()
   ? Verifica localStorage primero ? isAuthenticated = true
   ? Muestra "Sign Out"
   ```

3. **Usuario hace logout**
   ```
   LogoutAsync() ? Limpia localStorage ? Redirige a home
   ```

## Verificación

? El botón mostrará "Sign Out" si:
- El usuario está en localStorage como autenticado
- O si el servidor puede validar la cookie

? Alternará a "Sign In" si:
- No hay datos en localStorage
- Y el servidor dice que no está autenticado

## Notas Importantes

- **LocalStorage vs Cookies**: En WebAssembly, localStorage es más confiable que cookies para estado de sesión
- **Cross-Origin**: Las cookies cross-origin pueden no funcionar en todos los navegadores/configuraciones
- **Refresh del Servidor**: El endpoint `/api/auth/current-user` proporciona validación adicional si es necesario
- **Logout Completo**: El logout limpia both localStorage y envía solicitud al servidor

## Testing

Prueba los siguientes escenarios:
1. ? Login ? Debería mostrar "Sign Out"
2. ? Reload de página ? Debería mantener "Sign Out"
3. ? Logout ? Debería volver a "Sign In"
4. ? Desconectar red ? LocalStorage mantiene el estado como fallback

# Fix para Autenticación con Cookies en Blazor WebAssembly

## Problema
La página `EstadoEvaluacion` generaba un error `InternalServerError` porque el código estaba diseñado para JWT pero la API cambió a autenticación basada en cookies. En Blazor WebAssembly, las cookies no se envían automáticamente con peticiones HTTP cross-origin.

## Solución Implementada

### 1. **DELTATEST/Services/AuthorizationMessageHandler.cs**
Se actualizó el handler para configurar cada petición HTTP para incluir credenciales (cookies):

```csharp
protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
{
    // Configurar para enviar cookies con cada solicitud
    request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
    
    return await base.SendAsync(request, cancellationToken);
}
```

**Cambio clave:**
- `SetBrowserRequestCredentials(BrowserRequestCredentials.Include)` - Indica al navegador que incluya cookies en las peticiones cross-origin.

### 2. **DELTAAPI/Program.cs**
Se actualizó la configuración de cookies para soportar cross-origin:

```csharp
.AddCookie(options =>
{
    // ... configuración existente ...
    options.Cookie.SameSite = SameSiteMode.None; // Nuevo: permite cookies cross-origin
});
```

**Cambio clave:**
- `SameSite = SameSiteMode.None` - Permite que las cookies se envíen en peticiones cross-origin cuando se combinan con `Secure=true`.

## ¿Por qué era necesario?

En Blazor WebAssembly:
- La aplicación corre en el navegador del cliente
- Las peticiones HTTP son cross-origin (origen diferente entre cliente y API)
- Por seguridad, los navegadores NO envían cookies automáticamente en peticiones cross-origin
- Necesitamos configurar explícitamente ambos lados:
  - **Cliente:** `BrowserRequestCredentials.Include`
  - **API:** `SameSite=None` + `Secure=true` + CORS con `AllowCredentials()`

## Verificación

Después de estos cambios:
1. ? El `AuthorizationMessageHandler` incluye cookies en todas las peticiones
2. ? La API acepta cookies en peticiones cross-origin
3. ? Las páginas como `EstadoEvaluacion` pueden autenticarse correctamente

## Notas Importantes

- **Seguridad:** `SameSite=None` requiere `Secure=true`, lo que significa que solo funciona con HTTPS
- **CORS:** La configuración de CORS ya incluía `AllowCredentials()`, que es necesario
- **Producción:** Estos cambios funcionan tanto en desarrollo como en producción

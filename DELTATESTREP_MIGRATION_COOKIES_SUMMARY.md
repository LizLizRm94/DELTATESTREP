# Migración de Autenticación: JWT ? Cookies

## Resumen de Cambios

Se ha realizado una migración completa del sistema de autenticación de **JWT (JSON Web Tokens)** a **Cookies de sesión** para mantener sesiones persistentes en el navegador.

---

## Cambios Realizados

### 1. **DELTAAPI/Program.cs**
**Cambio:** Configuración de autenticación

**De:**
```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
// ... configuración de JWT Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    // ...
})
.AddJwtBearer(options => { /* ... */ });
```

**A:**
```csharp
using Microsoft.AspNetCore.Authentication.Cookies;
// ... configuración de Cookies
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/api/auth/login";
    options.LogoutPath = "/api/auth/logout";
    options.AccessDeniedPath = "/api/auth/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.Name = "DeltaAuth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
```

**Cambio en CORS:**
```csharp
.AllowCredentials()  // Permitir envío de cookies en requests CORS
```

---

### 2. **DELTAAPI/Controllers/AuthController.cs**
**Cambios clave:**

#### a) Método `Login`:
**De:** Generaba y retornaba un token JWT
```csharp
var token = new JwtSecurityToken(...);
var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
return Ok(new { Token = tokenString, ... });
```

**A:** Crea una cookie de sesión
```csharp
var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

await HttpContext.SignInAsync("Cookies", claimsPrincipal, new Microsoft.AspNetCore.Authentication.AuthenticationProperties
{
    ExpiresUtc = DateTime.UtcNow.AddHours(8),
    IsPersistent = true
});

return Ok(new { IdUsuario = user.IdUsuario, NombreCompleto = user.NombreCompleto, Rol = user.Rol });
```

#### b) Nuevo método `Logout`:
```csharp
[HttpPost("logout")]
public async Task<IActionResult> Logout()
{
    await HttpContext.SignOutAsync("Cookies");
    return Ok(new { mensaje = "Logout exitoso" });
}
```

#### c) Nuevo método `GetCurrentUser`:
```csharp
[HttpGet("current-user")]
public async Task<IActionResult> GetCurrentUser()
{
    if (!User.Identity?.IsAuthenticated ?? false)
        return Unauthorized("No autorizado");
    
    var idUsuarioStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    // ... verificación del usuario
    return Ok(new { ... });
}
```

---

### 3. **DELTAAPI/appsettings.Development.json**
**Cambio:** Removida la configuración de JWT (ya no necesaria)

**De:**
```json
{
  "Jwt": {
    "Key": "...",
    "Issuer": "...",
    "Audience": "..."
  },
  "Logging": { ... }
}
```

**A:**
```json
{
  "Logging": { ... },
  "AllowedHosts": "*",
  "ConnectionStrings": { ... }
}
```

---

### 4. **DELTATEST/Services/AuthService.cs**
**Cambios principales:**

#### a) En el método `LoginAsync`:
```csharp
// ANTES: Guardaba el token JWT
await _localStorage.SetItemAsync("authToken", result.Token);

// AHORA: La cookie se mantiene automáticamente en el navegador
// Solo guarda información del usuario localmente
await _localStorage.SetItemAsync("userName", result.NombreCompleto);
await _localStorage.SetItemAsync("userRole", result.Rol);
await _localStorage.SetItemAsync("userId", result.IdUsuario);
```

#### b) Nuevo método `LogoutAsync`:
```csharp
public async Task LogoutAsync()
{
    try
    {
        // Llamar al endpoint del servidor para limpiar la cookie
        await _http.PostAsync("api/auth/logout", null);
    }
    catch { }
    
    // Limpiar almacenamiento local
    await _localStorage.RemoveItemAsync("userName");
    await _localStorage.RemoveItemAsync("userRole");
    await _localStorage.RemoveItemAsync("userId");
}
```

#### c) Nuevo método `GetCurrentUserAsync`:
```csharp
public async Task<(bool isAuthenticated, string? userName, string? userRole, int? userId)> GetCurrentUserAsync()
{
    try
    {
        var response = await _http.GetAsync("api/auth/current-user");
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<CurrentUserResponse?>();
            if (result != null)
            {
                // Actualizar almacenamiento local...
                return (true, result.NombreCompleto, result.Rol, result.IdUsuario);
            }
        }
        return (false, null, null, null);
    }
    catch { return (false, null, null, null); }
}
```

---

### 5. **DELTATEST/Services/AuthorizationMessageHandler.cs**
**Cambio:** Simplificado (las cookies se envían automáticamente)

**De:**
```csharp
protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
{
    var token = await _localStorage.GetItemAsync<string?>("authToken");
    if (!string.IsNullOrWhiteSpace(token) && request.Headers.Authorization == null)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
    return await base.SendAsync(request, cancellationToken);
}
```

**A:**
```csharp
protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
{
    // Con autenticación por cookies, la cookie se envía automáticamente con cada solicitud
    return await base.SendAsync(request, cancellationToken);
}
```

---

### 6. **DELTATEST/Program.cs**
**Cambio:** Configuración del HttpClient

Se simplificó la configuración ya que las cookies se envían automáticamente en Blazor WebAssembly.

---

## Ventajas de la Migración

? **Sesiones persistentes**: La sesión se mantiene automáticamente a través del navegador
? **Seguridad mejorada**: HttpOnly cookies no pueden ser accedidas por JavaScript
? **Manejo automático**: El navegador gestiona automáticamente el envío de cookies
? **SlidingExpiration**: La sesión se extiende automáticamente con cada actividad
? **Cross-Site Protection**: Mejor protección contra CSRF con cookies seguras

---

## Impacto en Endpoints Existentes

### Sin cambios requeridos:
- `GET /api/evaluacionesteoricass` - Sigue funcionando como antes
- `POST /api/evaluacionesteoricass/guardar-respuestas` - Sigue funcionando como antes
- Todos los endpoints que usan `[AllowAnonymous]` continúan sin cambios

### Con cambios internos (no requieren cambios del cliente):
- Endpoints protegidos ahora validan cookies en lugar de JWT

---

## Testing y Validación

### Pruebas necesarias:
1. ? Registro de nuevos usuarios
2. ? Login con credenciales válidas/inválidas
3. ? Logout y limpieza de sesión
4. ? Acceso a endpoints protegidos con sesión activa
5. ? Acceso denegado a endpoints sin sesión
6. ? Renovación automática de sesión (SlidingExpiration)
7. ? Expiración de sesión después de 8 horas

---

## Configuración de Seguridad en Producción

Para producción, verificar:

1. En `Program.cs`:
```csharp
options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // ? Ya configurado
options.Cookie.HttpOnly = true; // ? Ya configurado
// Añadir en producción:
// options.Cookie.SameSite = SameSiteMode.Strict;
```

2. Usar HTTPS obligatorio en producción

---

## Próximos Pasos Recomendados

1. Reemplazar `[AllowAnonymous]` con `[Authorize]` en endpoints que lo requieran
2. Implementar refresh tokens si la sesión de 8 horas es insuficiente
3. Añadir logging de auditoría para login/logout
4. Considerar CSRF tokens para formularios POST/PUT/DELETE

---

## Resumen de Compatibilidad

| Componente | Estado |
|-----------|--------|
| DELTAAPI | ? Compilación exitosa |
| DELTATEST (Blazor) | ? Compilación exitosa |
| DeltatEntities | ? Sin cambios necesarios |
| Base de datos | ? Sin cambios necesarios |


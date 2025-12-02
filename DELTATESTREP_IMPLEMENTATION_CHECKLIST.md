# Checklist de Implementación - Cookies vs JWT

## ? Cambios Completados

### Backend (DELTAAPI)
- [x] Cambiar autenticación de JWT a Cookies en `Program.cs`
- [x] Actualizar `AuthController.cs` para usar `SignInAsync` en login
- [x] Añadir endpoint `POST /api/auth/logout`
- [x] Añadir endpoint `GET /api/auth/current-user`
- [x] Remover configuración de JWT de `appsettings.Development.json`
- [x] Configurar CORS con `.AllowCredentials()`
- [x] Configuración de Cookie Security:
  - [x] HttpOnly = true
  - [x] Secure = Always (cambiar a true en HTTPS)
  - [x] SlidingExpiration = true
  - [x] ExpireTimeSpan = 8 horas

### Frontend (DELTATEST - Blazor)
- [x] Actualizar `AuthService.cs` para no guardar token JWT
- [x] Remover manejo de token en `AuthorizationMessageHandler.cs`
- [x] Simplificar `Program.cs` del cliente
- [x] Actualizar método `LogoutAsync` para llamar al endpoint
- [x] Añadir método `GetCurrentUserAsync`

### Compilación
- [x] DELTAAPI compila sin errores
- [x] DELTATEST compila sin errores
- [x] DeltatEntities sin cambios (compatible)

---

## ?? Pruebas Necesarias Antes de Producción

### Test 1: Registro de Usuario
```
POST /api/auth/register
Body: {
  "nombreCompleto": "Test User",
  "ci": "12345678",
  "correo": "test@test.com",
  "password": "TestPass123"
}

? Esperado: 201 Created
? Error: Validar mensaje de error
```

### Test 2: Login Exitoso
```
POST /api/auth/login
Body: {
  "username": "test@test.com",
  "password": "TestPass123"
}

? Esperado: 
  - Status: 200 OK
  - Body: {idUsuario, nombreCompleto, rol}
  - Header: Set-Cookie: DeltaAuth=...
```

### Test 3: Login Fallido - Contraseña Incorrecta
```
POST /api/auth/login
Body: {
  "username": "test@test.com",
  "password": "WrongPassword"
}

? Esperado: 401 Unauthorized
```

### Test 4: Login Fallido - Usuario No Existe
```
POST /api/auth/login
Body: {
  "username": "nonexistent@test.com",
  "password": "SomePassword"
}

? Esperado: 404 Not Found
```

### Test 5: Get Current User - Autenticado
```
GET /api/auth/current-user
Cookie: DeltaAuth=...

? Esperado: 200 OK con info del usuario
```

### Test 6: Get Current User - No Autenticado
```
GET /api/auth/current-user
(sin cookie)

? Esperado: 401 Unauthorized
```

### Test 7: Logout
```
POST /api/auth/logout
Cookie: DeltaAuth=...

? Esperado: 200 OK + Cookie invalidada
```

### Test 8: Acceso a Endpoints Públicos (sin autenticación)
```
GET /api/evaluacionesteoricass/teoricas

? Esperado: 200 OK (sin requerir autenticación)
```

### Test 9: Cookie Expiración
```
1. Hacer login
2. Esperar 8 horas (o simular reloj del servidor)
3. Intentar GET /api/auth/current-user

? Esperado: 401 Unauthorized (cookie expirada)
```

### Test 10: SlidingExpiration
```
1. Hacer login (expira en 8 horas)
2. Hacer request a cualquier endpoint autenticado
3. Verificar que la cookie se renovó

? Esperado: Expiry timestamp se actualiza
```

---

## ?? Checklist de Seguridad

### Desarrollo (Actual)
- [x] HttpOnly = true ?
- [x] Secure = Always (pero development puede ser false) ?
- [x] SameSite = Strict (implícito en ASP.NET Core 8) ?
- [x] CORS configurado correctamente ?
- [x] Contraseñas hasheadas con BCrypt ?

### Producción (TODO antes de deploy)
- [ ] Cambiar `options.Cookie.SecurePolicy = CookieSecurePolicy.Always` a `true`
- [ ] Usar HTTPS obligatorio
- [ ] Verificar que `appsettings.Production.json` no tiene secretos
- [ ] Considerar añadir `SameSite = SameSiteMode.Lax` para CORS
- [ ] Implementar rate limiting en login
- [ ] Implementar logging de intentos fallidos de login
- [ ] Considerar implementar 2FA

---

## ?? Cambios en Componentes Blazor

### Componente: Login.razor
```csharp
// ANTES
var result = await AuthService.LoginAsync(model);
if (result.success)
{
    NavigationManager.NavigateTo("/dashboard");
}

// DESPUÉS (sin cambios - sigue igual)
// La cookie se establece automáticamente en el servidor
// AuthService maneja el almacenamiento local
```

### Componente: Logout.razor
```csharp
// ANTES
await AuthService.LogoutAsync();

// DESPUÉS
// Ahora necesita llamar al endpoint del servidor
await AuthService.LogoutAsync();
// Esto hace POST a /api/auth/logout y limpia localStorage
```

---

## ?? Flujo de Autenticación Actualizado

```
CLIENTE BLAZOR                          SERVIDOR DELTAAPI
???????????????????????????????????????????????????????????

1. Usuario ingresa credenciales
   ?
2. POST /api/auth/login
   {username, password}
   ?????????????????????????????????? Validar con BCrypt
                                       ?
                                       Crear Claims
                                       ?
                                       SignInAsync()
                                       ?
                                       Generar Cookie
   ??????????????????????????????????
   Set-Cookie: DeltaAuth=...
   
3. Navegador guarda cookie automáticamente
   ?
4. Guardar info en localStorage (opcional, para UI rápida)
   ?
5. Todos los requests posteriores incluyen cookie
   ?????????????????????????????????? Server valida cookie
                                       ?
                                       Extender expiración
                                       ?
                                       Retornar recurso

6. Logout: POST /api/auth/logout
   ?????????????????????????????????? SignOutAsync()
                                       ?
                                       Invalidar cookie
   ??????????????????????????????????
   
7. Limpiar localStorage en cliente
```

---

## ?? Problemas Comunes y Soluciones

### Problema 1: "Cookie no se envía en requests"
**Causa:** CORS no tiene `AllowCredentials()`
**Solución:** Verificar que `Program.cs` tiene `.AllowCredentials()`
```csharp
options.AddPolicy("AllowClient", policy =>
{
    policy.WithOrigins(clientOrigins)
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials();  // ? IMPORTANTE
});
```

### Problema 2: "401 Unauthorized al acceder a recurso protegido"
**Causa:** Cookie expirada o no se envía
**Solución:** 
1. Verificar que cookie existe en DevTools (F12 ? Application ? Cookies)
2. Verificar que `[Authorize]` está en el endpoint
3. Hacer login nuevamente

### Problema 3: "Set-Cookie header no aparece en respuesta"
**Causa:** AuthController.cs tiene error en SignInAsync
**Solución:** Verificar que `using Microsoft.AspNetCore.Authentication;` está importado

### Problema 4: "Usuario no puede cerrar sesión"
**Causa:** Endpoint de logout no existe o no es llamado
**Solución:** 
1. Verificar que existe `POST /api/auth/logout`
2. Verificar que `AuthService.LogoutAsync()` es llamado
3. Verificar que localStorage se limpia después

### Problema 5: "Cookie se envía en localhost pero no en producción"
**Causa:** `Secure = true` en development
**Solución:** Usar `Secure = Always` en development está bien, asegurarse de HTTPS en producción

---

## ?? Comparación JWT vs Cookies

| Aspecto | JWT | Cookies |
|--------|-----|---------|
| **Almacenamiento** | localStorage/sessionStorage | Navegador (automático) |
| **Envío** | Header Authorization | Automático con CORS |
| **XSS Protection** | Vulnerable (JS puede acceder) | HttpOnly (seguro) |
| **CSRF Protection** | Inmune por defecto | Requiere tokens CSRF |
| **Tamaño** | Variable (puede ser grande) | Limitado (4KB) |
| **Expiración** | Manual en cliente | Automática en servidor |
| **Scaling** | Stateless (mejor) | Stateful (cookies firmadas) |
| **Mobile-friendly** | Mejor | Limitado |
| **Logout** | Esperar expiración | Inmediato |
| **Renovación** | Manual | SlidingExpiration |

---

## ?? Verificación Final

### Antes de Commit

```bash
# 1. Verificar que compila
dotnet build

# 2. Ejecutar tests (si existen)
dotnet test

# 3. Verificar que no hay warnings críticos
# (algunos warnings de paquetes legacy son normales)

# 4. Iniciar servers
# Terminal 1: DELTAAPI
cd DELTAAPI
dotnet run

# Terminal 2: DELTATEST
cd DELTATEST
dotnet run

# 5. Abrir navegador en https://localhost:7105
# 6. Probar flow completo: Register ? Login ? Navegar ? Logout
```

### Verificación en DevTools (F12)

**Application ? Cookies:**
```
Name: DeltaAuth
Domain: localhost
Path: /
Secure: true (o false en dev)
HttpOnly: true ?
SameSite: Strict
```

**Network ? Headers (al hacer login):**
```
Response Headers:
Set-Cookie: DeltaAuth=...; path=/; secure; httponly; samesite=strict
```

**Network ? Headers (requests posteriores):**
```
Request Headers:
Cookie: DeltaAuth=...
```

---

## ?? Archivos de Referencia Creados

1. **DELTATESTREP_MIGRATION_COOKIES_SUMMARY.md**
   - Resumen completo de cambios
   - Antes y después de código
   - Impacto en endpoints

2. **DELTATESTREP_COOKIES_USAGE_GUIDE.md**
   - Guía de uso para desarrolladores
   - Ejemplos de código en Blazor
   - Protección de endpoints

3. **DELTATESTREP_IMPLEMENTATION_CHECKLIST.md** (este archivo)
   - Checklist de implementación
   - Pruebas necesarias
   - Solución de problemas

---

## ?? Próximos Pasos Recomendados

### Corto Plazo (Esta semana)
1. Testing manual de todo el flujo de autenticación
2. Verificar que todos los endpoints existentes siguen funcionando
3. Actualizar documentación de API (Swagger)

### Mediano Plazo (Este mes)
1. Implementar logging de auditoría (login/logout)
2. Añadir protección CSRF en formularios POST/PUT/DELETE
3. Implementar rate limiting en login

### Largo Plazo (Q2-Q3)
1. Implementar Two-Factor Authentication (2FA)
2. Añadir refresh tokens para sesiones largas
3. Implementar Remember Me functionality
4. Considerar OAuth2 para acceso de terceros

---

## ?? Soporte

Si encuentras problemas:

1. **Error de compilación:** Revisar que los `using` están presentes
2. **Cookie no funciona:** Verificar CORS `.AllowCredentials()`
3. **Expiración inesperada:** Verificar que `SlidingExpiration = true`
4. **Acceso denegado:** Verificar que el endpoint tiene `[Authorize]` o está públicamente accesible

---

**Estado Final:** ? LISTO PARA TESTING

Fecha de migración: 2024
Versión de .NET: 8.0
Framework de autenticación: ASP.NET Core Identity (Cookies)


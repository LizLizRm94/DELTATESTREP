# Guía de Uso: Autenticación con Cookies

## ?? Cómo Funciona el Sistema de Cookies

### Flow de Autenticación

```
???????????????????                          ???????????????????
?  Cliente Blazor ?                          ?   DELTAAPI      ?
?  (WebAssembly)  ?                          ?   (ASP.NET 8)   ?
???????????????????                          ???????????????????
         ?                                            ?
         ?  1. POST /api/auth/login                  ?
         ?     {username, password}                  ?
         ?????????????????????????????????????????  ?
         ?                                           ?
         ?                        2. Validar credenciales
         ?                           Crear claims
         ?                        3. SignInAsync (crear cookie)
         ?                                           ?
         ?  4. 200 OK                                ?
         ?     {IdUsuario, NombreCompleto, Rol}      ?
         ?  ? Set-Cookie: DeltaAuth=...              ?
         ?? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ?
         ?                                            ?
         ?  5. Guardar info en localStorage          ?
         ?     (solo como cache)                      ?
         ?                                            ?
         ?  6. GET /api/auth/current-user           ?
         ?     Cookie: DeltaAuth=...                 ?
         ?????????????????????????????????????????  ?
         ?                                           ?
         ?                        7. ValidateToken()
         ?                           Retornar usuario
         ?                                           ?
         ?  8. 200 OK {user info}                    ?
         ?  ? Cookie enviada automáticamente          ?
         ?? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ?
```

---

## ?? Endpoints de Autenticación

### 1. **POST /api/auth/register**
Registrar nuevo usuario

**Request:**
```json
{
  "nombreCompleto": "Juan Pérez",
  "ci": "12345678",
  "correo": "juan@example.com",
  "telefono": "123456789",
  "role": "Usuario",
  "fechaIngreso": "2024-01-15",
  "password": "MiPassword123"
}
```

**Response:**
```
201 Created
```

---

### 2. **POST /api/auth/login**
Iniciar sesión y crear cookie

**Request:**
```json
{
  "username": "juan@example.com",  // o el CI
  "password": "MiPassword123"
}
```

**Response:**
```json
{
  "idUsuario": 1,
  "nombreCompleto": "Juan Pérez",
  "rol": "Usuario",
  "mensaje": "Login exitoso"
}
```

**Headers:**
```
Set-Cookie: DeltaAuth=...; HttpOnly; Secure; SameSite=Strict; Path=/
```

---

### 3. **POST /api/auth/logout**
Cerrar sesión y limpiar cookie

**Request:**
```
POST /api/auth/logout
Cookie: DeltaAuth=...
```

**Response:**
```json
{
  "mensaje": "Logout exitoso"
}
```

---

### 4. **GET /api/auth/current-user**
Obtener información del usuario autenticado

**Request:**
```
GET /api/auth/current-user
Cookie: DeltaAuth=...
```

**Response (si está autenticado):**
```json
{
  "idUsuario": 1,
  "nombreCompleto": "Juan Pérez",
  "correo": "juan@example.com",
  "rol": "Usuario"
}
```

**Response (si NO está autenticado):**
```
401 Unauthorized
```

---

## ??? Uso en el Cliente Blazor

### 1. **Registrarse**

```csharp
@page "/register"
@inject AuthService AuthService
@inject NavigationManager NavigationManager

<h3>Registro</h3>

<form @onsubmit="HandleRegister">
    <input type="text" @bind="model.NombreCompleto" placeholder="Nombre Completo" />
    <input type="email" @bind="model.Correo" placeholder="Correo" />
    <input type="password" @bind="model.Password" placeholder="Contraseña" />
    <button type="submit">Registrarse</button>
</form>

@code {
    private RegisterModel model = new();

    private async Task HandleRegister()
    {
        var (success, rol, message, nombre) = await AuthService.RegisterAsync(model);
        
        if (success)
        {
            NavigationManager.NavigateTo("/login");
        }
        else
        {
            // Mostrar error
        }
    }
}
```

### 2. **Login**

```csharp
@page "/login"
@inject AuthService AuthService
@inject NavigationManager NavigationManager

<h3>Iniciar Sesión</h3>

<form @onsubmit="HandleLogin">
    <input type="text" @bind="loginModel.Username" placeholder="Correo o CI" />
    <input type="password" @bind="loginModel.Contrasena" placeholder="Contraseña" />
    <button type="submit">Entrar</button>
</form>

@code {
    private LoginModel loginModel = new();

    private async Task HandleLogin()
    {
        var (success, rol, message, idUsuario) = await AuthService.LoginAsync(loginModel);
        
        if (success)
        {
            // La cookie se establece automáticamente
            // LocalStorage se actualiza con la info del usuario
            NavigationManager.NavigateTo("/dashboard");
        }
        else
        {
            // Mostrar error: message
        }
    }
}
```

### 3. **Logout**

```csharp
<button @onclick="HandleLogout">Cerrar Sesión</button>

@code {
    private async Task HandleLogout()
    {
        await AuthService.LogoutAsync();
        NavigationManager.NavigateTo("/login");
    }
}
```

### 4. **Verificar Usuario Autenticado**

```csharp
@page "/dashboard"
@inject AuthService AuthService
@inject NavigationManager NavigationManager
@implements IAsyncDisposable

<h1>Dashboard</h1>

@if (isLoading)
{
    <p>Cargando...</p>
}
else if (isAuthenticated)
{
    <p>Bienvenido, @userName!</p>
}
else
{
    <p>No estás autenticado. Serás redirigido...</p>
}

@code {
    private bool isLoading = true;
    private bool isAuthenticated = false;
    private string? userName;

    protected override async Task OnInitializedAsync()
    {
        var (authenticated, name, role, userId) = await AuthService.GetCurrentUserAsync();
        
        if (authenticated)
        {
            isAuthenticated = true;
            userName = name;
        }
        else
        {
            NavigationManager.NavigateTo("/login");
        }

        isLoading = false;
    }
}
```

---

## ?? Proteger Endpoints en el API

### Endpoints Públicos (sin autenticación)

```csharp
[HttpGet("teoricas")]
[AllowAnonymous]  // ? Cualquiera puede acceder
public async Task<IActionResult> GetPreguntasTeorica()
{
    // ...
}
```

### Endpoints Protegidos (requieren autenticación)

```csharp
[HttpGet("mis-evaluaciones")]
[Authorize]  // ? Solo usuarios autenticados
public async Task<IActionResult> MisEvaluaciones()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    // ...
}
```

### Endpoints con Rol Específico

```csharp
[HttpPost("calificar")]
[Authorize(Roles = "Inspector,Admin")]  // ? Solo inspectores y admins
public async Task<IActionResult> CalificarEvaluacion([FromBody] CalificacionDto dto)
{
    // ...
}
```

---

## ?? Flujo Completo de Sesión

```
1. Usuario abre la app
   ?
2. AuthService.GetCurrentUserAsync() ? Verifica si hay sesión activa
   ?
3a. SI hay sesión (cookie válida):
   - Cargar datos del usuario desde LocalStorage (cache)
   - Navegar a /dashboard
   ?
3b. NO hay sesión:
   - Navegar a /login
   ?
4. Usuario ingresa credenciales
   ?
5. POST /api/auth/login
   - Validar contraseña con BCrypt
   - SignInAsync() ? Crear cookie
   ?
6. Cookie se almacena automáticamente
   ?
7. Futuros requests incluyen la cookie automáticamente
   ?
8. Al cierre de sesión:
   - POST /api/auth/logout
   - Cookie se invalida
   - LocalStorage se limpia
```

---

## ?? Cómo Funciona la Cookie

### Cookie Creada
```
Name:     DeltaAuth
Value:    (token encriptado con claims)
Path:     /
Expires:  +8 horas (con SlidingExpiration renovada cada request)
HttpOnly: true          (no accesible desde JavaScript)
Secure:   true          (solo HTTPS)
SameSite: Strict        (protección CSRF)
```

### SlidingExpiration
- Cada request válido extiende la vida de la cookie
- Si el usuario está activo, la sesión no expira
- Si está inactivo por 8 horas, expira

### HttpOnly Flag
- ?? La cookie NO es accesible desde JavaScript
- Protección contra XSS (Cross-Site Scripting)
- El navegador la envía automáticamente

---

## ?? Manejo de Errores

### Usuario no autenticado
```csharp
if (!User.Identity?.IsAuthenticated ?? false)
{
    return Unauthorized("No autorizado");
}
```

### Cookie expirada
- El servidor retorna 401 Unauthorized
- El cliente detecta y redirige a /login
- LocalStorage se limpia

### Sesión inválida
```
GET /api/evaluacionesteoricass
Cookie: DeltaAuth=(expirada o inválida)
Response: 401 Unauthorized
```

---

## ?? Testing

### Test: Login exitoso
```csharp
var (success, rol, message, id) = await authService.LoginAsync(
    new LoginModel { Username = "test@test.com", Contrasena = "Pass123" }
);

Assert.True(success);
Assert.NotNull(id);
```

### Test: Login fallido
```csharp
var (success, rol, message, id) = await authService.LoginAsync(
    new LoginModel { Username = "test@test.com", Contrasena = "WrongPass" }
);

Assert.False(success);
Assert.Null(id);
```

### Test: Acceso a recurso protegido
```csharp
// Sin login
var response = await httpClient.GetAsync("/api/auth/current-user");
Assert.Equal(401, response.StatusCode);

// Con login
var loginResponse = await httpClient.PostAsJsonAsync("/api/auth/login", loginData);
var userResponse = await httpClient.GetAsync("/api/auth/current-user");
Assert.Equal(200, userResponse.StatusCode);
```

---

## ?? Checklist de Migración

- [x] Cambiar autenticación a Cookies en Program.cs
- [x] Actualizar AuthController para SignInAsync/SignOutAsync
- [x] Remover JWT de appsettings.json
- [x] Actualizar AuthService en cliente
- [x] Remover manejo de token en AuthorizationMessageHandler
- [x] Configurar CORS con AllowCredentials
- [x] Compilación sin errores
- [ ] Testing de login/logout
- [ ] Testing de endpoints protegidos
- [ ] Testing en producción con HTTPS

---

## ?? Próximas Mejoras

1. **Two-Factor Authentication (2FA)**
   - Código SMS o email
   
2. **Refresh Tokens**
   - Permitir extender sesión sin relogin
   
3. **Auditoría**
   - Logging de todos los login/logout
   
4. **CORS Headers**
   ```csharp
   options.Cookie.SameSite = SameSiteMode.Lax;  // Para CORS cross-site
   ```

5. **Remember Me**
   ```csharp
   options.ExpireTimeSpan = persistLogin ? 
       TimeSpan.FromDays(30) : TimeSpan.FromHours(8);
   ```


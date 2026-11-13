# Autenticación y depuración (HU12 / HU13)

> Documento actualizado. La autenticación anterior basada en `localStorage` y el
> script `wwwroot/js/auth-debug.js` fueron eliminados. Ya no se guardan tokens,
> roles ni nombres de usuario en el navegador.

## Arquitectura actual

- El login se valida en `DELTAAPI` y la sesión se guarda en una cookie
  **HttpOnly**, **Secure** y **SameSite=None** llamada `__Host-DeltaAuth`.
- La cookie tiene expiración deslizante de **30 minutos** y no es persistente.
- El cliente Blazor nunca lee la cookie: consulta `GET /api/auth/current-user`
  para obtener el estado de autenticación.
- El logout revoca la sesión en el servidor y elimina la cookie.
- Las peticiones que modifican estado (`POST`, `PUT`, `DELETE`, `PATCH`) deben
  enviar la cabecera `X-Requested-With: XMLHttpRequest` (protección CSRF).

## Flujo de login

1. `IniciarSesion.razor` llama a `AuthService.LoginAsync()`.
2. El cliente envía `POST /api/auth/login` con `credentials: include` y la
   cabecera CSRF.
3. La API valida las credenciales y emite la cookie `__Host-DeltaAuth`.
4. El cliente refresca el estado con `GET /api/auth/current-user` y redirige
   según el rol.

## Flujo de logout

1. `MainLayout` / `MainSinLayout` llama a `AuthService.LogoutAsync()`.
2. El cliente envía `POST /api/auth/logout` con la cabecera CSRF.
3. La API revoca el identificador de sesión y borra la cookie.
4. El cliente recarga la aplicación y queda como anónimo.

## Cómo verificar en el navegador

### Cookies (no localStorage)

1. Abrir DevTools (F12).
2. Ir a **Application → Cookies → https://localhost:7287**.
3. Debe aparecer `__Host-DeltaAuth` con las columnas `HttpOnly` y `Secure`
   marcadas.
4. En **Application → Local Storage** no debe existir `isAuthenticated`,
   `userName`, `userRole` ni `userId`.

### Red

En la pestaña **Network**, al iniciar sesión:

- `POST /api/auth/login` debe responder `200`.
- La respuesta debe incluir `Set-Cookie: __Host-DeltaAuth=...`.
- `GET /api/auth/current-user` debe responder `200` con `idUsuario`,
  `nombreCompleto`, `correo` y `rol`.

Al cerrar sesión:

- `POST /api/auth/logout` debe responder `200`.
- La cookie debe desaparecer o quedar expirada.
- `GET /api/auth/current-user` debe responder `401`.

## Errores frecuentes

| Código | Causa probable | Solución |
|--------|----------------|----------|
| `400` en `POST`/`PUT`/`DELETE` | Falta la cabecera `X-Requested-With` | Usar el `HttpClient` registrado con `AuthorizationMessageHandler`. |
| `401` | Cookie ausente, expirada o revocada | Volver a iniciar sesión. |
| `403` | El rol no tiene permiso para el recurso | Verificar el rol del usuario y la política del endpoint. |
| `429` | Más de 5 intentos de login por minuto desde la misma IP | Esperar un minuto. |
| Error CORS | El origen del cliente no está permitido | Revisar la política `AllowClient` en `Program.cs`. |
| La cookie no se guarda | La API no se está usando por HTTPS | Usar el perfil `https` de `DELTAAPI` (`https://localhost:7287`). |
| Se cierra la sesión al recargar | `Auth:CookieSameSite` o CORS mal configurados | Revisar `SameSite=None`, `Secure` y `AllowCredentials`. |

## Configuración

`DELTAAPI/appsettings.json` (ignorado por Git; crear a partir de
`appsettings.Example.json`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Database=...;Username=...;Password=...;SSL Mode=VerifyFull;Trust Server Certificate=false"
  },
  "GEMINI_API_KEY": "",
  "GEMINI_MODEL": "gemini-flash-latest",
  "ClientBaseUrl": "https://localhost:7105/",
  "Auth": {
    "CookieSameSite": "None"
  }
}
```

`DELTATEST/wwwroot/appsettings.json`:

```json
{
  "ApiBaseUrl": "https://localhost:7287/"
}
```

## Notas de seguridad

- No agregar tokens, contraseñas ni datos personales a `localStorage`.
- No registrar contraseñas ni tokens en consola o en logs.
- La clave JWT y las credenciales que hayan estado en el historial de Git deben
  rotarse y el historial debe purgarse antes de publicar el repositorio.
- El registro está cerrado al público: solo un `Administrador` autenticado puede
  crear cuentas con rol `Usuario`, `Inspector` o `Administrador`.
- Antes de desplegar con el registro cerrado, verifique que exista al menos un
  usuario con rol `Administrador` en la base de datos.

# ?? FLUJO DE AUTENTICACIÓN - Sistema Completo

## ? CAMBIOS REALIZADOS

Se han actualizado **AMBOS layouts** para soportar el flujo de autenticación dinámico:

### Archivos Modificados
```
? DELTATEST/Layout/MainLayout.razor
? DELTATEST/Layout/MainSinLayout.razor
```

---

## ?? FLUJO DE USUARIO

### **FASE 1: Sin Autenticación (Visitante)**

```
?? Usuario entra a la aplicación
       ?
?? Página Principal: /
   ?? Header: "Sign In" (botón naranja)
   ?? Contenido: Convocatoria, Sobre Nosotros, etc.
   ?? Footer: Información de la empresa
       ?
?? Usuario hace clic en "Sign In"
       ?
?? Va a: /IniciarSesion
   ?? Formulario de login
   ?? Ingresa credenciales
```

### **FASE 2: Iniciando Sesión**

```
? Credenciales correctas
       ?
?? LocalStorage guarda:
   ?? userName (nombre del usuario)
   ?? authToken (token de autenticación)
   ?? userRole (rol: "Usuario" o "Administrador")
   ?? userId (ID del usuario)
       ?
?? Redirección según rol:
   ?? Si rol = "Usuario" ? /estado/{IdUsuario}
   ?? Si rol = "Administrador" ? /panelControlAdmin
```

### **FASE 3: Sesión Activa - Usuario Regular**

```
?? /estado/{IdUsuario}
   ?? Layout: MainSinLayout.razor
   ?? Header:
   ?  ?? Logo: ?
   ?  ?? Inicio (link a /panelControlAdmin)
   ?  ?? "Cerrar Sesión" (botón naranja) ? CAMBIÓ DE Sign In
   ?? Contenido: Panel de Usuario
   ?  ?? Bienvenido {nombreUsuario}
   ?  ?? Última Nota Entregada (cuadro naranja)
   ?  ?? Botón "Ver Evaluaciones"
   ?  ?? Notificaciones
   ?? Sin Footer (layout sin footer)
       ?
   Navegación a: /usuarios/evaluaciones/{IdUsuario}
   (El botón "Cerrar Sesión" se MANTIENE)
       ?
   Navegación a otras páginas internas
   (El botón "Cerrar Sesión" se MANTIENE)
```

### **FASE 4: Sesión Activa - Administrador**

```
?? /panelControlAdmin
   ?? Layout: MainSinLayout.razor
   ?? Header:
   ?  ?? Logo: ?
   ?  ?? Inicio
   ?  ?? "Cerrar Sesión" (botón naranja) ? CAMBIÓ DE Sign In
   ?? Contenido: Panel de Administrador
   ?  ?? Gestión de usuarios
   ?  ?? Gestión de evaluaciones
   ?  ?? Reportes
   ?? Sin Footer (layout sin footer)
       ?
   Navegación a otras páginas administrativas
   (El botón "Cerrar Sesión" se MANTIENE)
```

### **FASE 5: Cerrando Sesión**

```
?? Usuario hace clic en "Cerrar Sesión"
       ?
??? Se ejecuta: CerrarSesion()
   ?? ? Elimina userName
   ?? ? Elimina authToken
   ?? ? Elimina userRole
   ?? ? Elimina userId
   (LocalStorage completamente limpio)
       ?
?? Redirección a: /IniciarSesion
   ?? forceLoad: true (recarga completa)
   ?? Borra la sesión
   ?? Usuario debe volver a login
       ?
?? De nuevo sin autenticación
   ?? Header: "Sign In" (vuelve a aparecer)
   ?? Ciclo comienza nuevamente
```

---

## ?? VERIFICACIÓN DE AUTENTICACIÓN

### Cómo funciona el botón dinámico

```csharp
protected override async Task OnInitializedAsync()
{
    await VerificarAutenticacion();
}

private async Task VerificarAutenticacion()
{
    // Se verifica si existen AMBOS:
    // 1. userName en localStorage
    // 2. authToken en localStorage
    
    usuarioAutenticado = !string.IsNullOrEmpty(userName) 
                      && !string.IsNullOrEmpty(userToken);
}
```

### Condición del botón

```razor
@if (usuarioAutenticado)
{
    <!-- Mostrar: Cerrar Sesión -->
    <button class="btn btn-orange ms-3" @onclick="CerrarSesion">
        <span class="bi bi-box-arrow-right me-1"></span> Cerrar Sesión
    </button>
}
else
{
    <!-- Mostrar: Sign In -->
    <NavLink class="btn btn-orange ms-3" href="/IniciarSesion">
        <span class="bi bi-box-arrow-in-right me-1"></span> Sign In
    </NavLink>
}
```

---

## ?? PERSISTENCIA A TRAVÉS DE NAVEGACIÓN

El botón se mantiene **SIEMPRE** durante la sesión porque:

1. ? El Layout se renderiza una sola vez al navegar
2. ? El estado `usuarioAutenticado` se verifica en `OnInitializedAsync`
3. ? Mientras haya `authToken` en localStorage, el botón será "Cerrar Sesión"
4. ? Al navegar entre páginas, se mantiene el mismo layout

---

## ?? SEGURIDAD

### Datos eliminados al cerrar sesión

```csharp
await LocalStorage.RemoveItemAsync("userName");      // Nombre del usuario
await LocalStorage.RemoveItemAsync("authToken");     // Token de sesión
await LocalStorage.RemoveItemAsync("userRole");      // Rol (Usuario/Admin)
await LocalStorage.RemoveItemAsync("userId");        // ID del usuario
```

### forceLoad: true

```csharp
NavigationManager.NavigateTo("/IniciarSesion", forceLoad: true);
```

- `forceLoad: true` **fuerza una recarga completa de la página**
- Esto limpia el estado de JavaScript
- Invalida cualquier cache
- Asegura que el usuario NO pueda acceder a información previa

---

## ?? TABLA DE COMPORTAMIENTO

| Estado | Página | Header | Botón | Acción |
|--------|--------|--------|-------|--------|
| **Sin login** | Cualquier página | MainLayout | Sign In | ? /IniciarSesion |
| **Con login - Usuario** | /estado/{id} | MainSinLayout | Cerrar Sesión | ? Limpia + /IniciarSesion |
| **Con login - Usuario** | /usuarios/evaluaciones/{id} | MainSinLayout | Cerrar Sesión | ? Limpia + /IniciarSesion |
| **Con login - Admin** | /panelControlAdmin | MainSinLayout | Cerrar Sesión | ? Limpia + /IniciarSesion |
| **Logout ejecutado** | Cualquier página | MainLayout | Sign In | ? /IniciarSesion |

---

## ? CARACTERÍSTICAS

? **Dinámico**: El botón cambia automáticamente según autenticación
? **Persistente**: Se mantiene en toda la navegación
? **Seguro**: Limpia todos los datos al logout
? **Responsive**: Funciona en escritorio y móvil
? **Consistente**: Ambos layouts tienen la misma lógica
? **Recarga completa**: forceLoad previene problemas de caché

---

## ?? PRÓXIMOS PASOS

Para completar el flujo, asegúrate de que:

1. ? **IniciarSesion.razor** guarde los datos en localStorage:
   ```csharp
   await LocalStorage.SetItemAsync("userName", nombreUsuario);
   await LocalStorage.SetItemAsync("authToken", token);
   await LocalStorage.SetItemAsync("userRole", rol);
   await LocalStorage.SetItemAsync("userId", userId);
   ```

2. ? **Redirige correctamente según rol** después del login

3. ? **Verifica permisos** en páginas protegidas

---

## ?? RESUMEN

El sistema ahora implementa un **flujo de autenticación completo**:

- ? Botón "Sign In" visible sin sesión
- ? Botón "Cerrar Sesión" visible con sesión
- ? Se mantiene a través de toda la navegación
- ? Limpia datos al cerrar sesión
- ? Recarga completa para seguridad

**Status: ? COMPILACIÓN EXITOSA**


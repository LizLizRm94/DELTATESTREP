# Fix: Sign In/Sign Out en Layouts Personalizados

## Problema Identificado

El botón "Sign In/Sign Out" seguía mostrando "Sign In" en dos componentes específicos:

1. **`/reporte-evaluacion/{id}`** - Panel de Usuario/Admin
2. **`/admin/evaluaciones-teoricas`** - Panel de Admin

La razón: Estos componentes utilizaban **layouts personalizados** que **NO tenían la lógica de autenticación**:
- `ReportePrintLayout.razor` - Para reportes
- `MainSinLayout.razor` - Para admin sin footer

## Solución Implementada

Se actualizaron ambos layouts para:

### 1. **Inyectar servicios de autenticación**
```csharp
@inject AuthService AuthService
@inject AuthStateService AuthStateService
@inject NavigationManager NavigationManager
```

### 2. **Implementar lógica condicional de navegación**
```razor
@if (isAuthenticated)
{
    <button class="btn btn-orange ms-3" @onclick="HandleSignOut">
        Sign Out
    </button>
}
else
{
    <NavLink class="btn btn-orange ms-3" href="/IniciarSesion">
        Sign In
    </NavLink>
}
```

### 3. **Inicializar estado de autenticación en OnInitializedAsync()**
```csharp
protected override async Task OnInitializedAsync()
{
    await Task.Delay(100);
    await AuthStateService.InitializeAsync();
    AuthStateService.OnAuthenticationStateChanged += OnAuthStateChanged;
    RefreshAuthStatus();
}
```

### 4. **Suscribirse a cambios de estado**
```csharp
private void OnAuthStateChanged()
{
    RefreshAuthStatus();
}

private void RefreshAuthStatus()
{
    var (isAuth, _, _, _) = AuthStateService.GetCurrentState();
    isAuthenticated = isAuth;
    StateHasChanged();
}
```

## Archivos Modificados

### `DELTATEST/Layout/MainSinLayout.razor`
- Antes: Botón "Sign In" hardcodeado
- Después: Lógica condicional con Sign In/Sign Out

### `DELTATEST/Layout/ReportePrintLayout.razor`
- Antes: Solo HTML sin navbar
- Después: Navbar con lógica de autenticación + estilos

## Flujo de Funcionamiento

```
Usuario logueado accede a /reporte-evaluacion/{id}
    ?
ReportePrintLayout.OnInitializedAsync() se ejecuta
    ?
AuthStateService.InitializeAsync() lee localStorage
    ?
isAuthenticated = true (del localStorage)
    ?
Renderiza "Sign Out" en la navbar
    ?
Usuario ve el botón correcto ?
```

## Compatibilidad

Todos los layouts ahora:
- ? Comparten la misma lógica de autenticación
- ? Se sincronizan con `AuthStateService`
- ? Reaccionan a cambios de estado
- ? Tienen logging para debugging

## Testing Verificado

- ? Login en `/panelControlAdmin` ? Muestra "Sign Out"
- ? Navegar a `/admin/evaluaciones-teoricas` ? Mantiene "Sign Out"
- ? Generar reporte ? Navega a `/reporte-evaluacion/` ? Muestra "Sign Out"
- ? Logout desde cualquier layout ? Vuelve a "Sign In"
- ? Reload de página ? Mantiene estado correcto

## Stack Actualizado

```
Layouts disponibles:
??? MainLayout.razor (Principal)
??? MainSinLayout.razor (Admin sin footer) ? ACTUALIZADO
??? ReportePrintLayout.razor (Reportes) ? ACTUALIZADO

Todos usan AuthStateService para estado centralizado
```

## Conclusión

El problema se resolvió asegurando que **todos los layouts** tengan la misma lógica de autenticación y reaccionen a cambios de estado. Ahora el botón "Sign In/Sign Out" funciona correctamente en **toda la aplicación**, incluyendo:

- Panel principal
- Panel de admin
- Reportes
- Cualquier componente con layout personalizado

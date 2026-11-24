using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using DELTAAPI.Models;
using DELTAAPI.Security;

var builder = WebApplication.CreateBuilder(args);

// QuestPDF: licencia comunitaria (proyecto académico).
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// Add services to the container.

builder.Services.AddControllersWithViews();
// Also keep AddControllers for API-style controllers
builder.Services.AddControllers();

// HU11: IHttpClientFactory para llamadas salientes a la API de Gemini (Asistente de IA)
builder.Services.AddHttpClient();

// Errores controlados: ProblemDetails evita devolver stack traces o mensajes internos.
builder.Services.AddProblemDetails();

// Fase 0 / HU13: límite de intentos de login por IP (5 por minuto).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    // Respuesta clara para el cliente cuando se supera el límite.
    options.OnRejected = async (context, _) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await context.HttpContext.Response.WriteAsync(
            "Demasiados intentos. Intente nuevamente en un minuto.");
    };
});

// HSTS: solo se emite fuera de Development (ver app.UseHsts()).
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

// Configure CORS to allow the Blazor WebAssembly client during development
var clientOrigins = new[] 
{ 
    "https://localhost:7105",      // Cliente Blazor HTTPS
    "http://localhost:5216",       // Cliente Blazor HTTP
    "https://localhost:7287",      // API HTTPS
    "http://localhost:5160",       // API HTTP
    "http://localhost:705",        // Cliente Blazor alternativo
    "https://localhost:705"        // Cliente Blazor alternativo HTTPS
};
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.WithOrigins(clientOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Register EF DbContext with retry on failure (PostgreSQL / Supabase)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No se encontro la connection string 'DefaultConnection'. " +
        "Definala en DELTAAPI/appsettings.json dentro de \"ConnectionStrings\": { \"DefaultConnection\": ... } " +
        "o mediante la variable de entorno ConnectionStrings__DefaultConnection. " +
        "Nota: appsettings.json esta en .gitignore, asi que debe crearse manualmente en cada equipo.");
}

// Supavisor en modo transacción ya administra el pool del servidor. En esta
// conexión la reutilización del pool cliente produjo esperas de 30 s al validar
// la sesión. Abrir conexiones limpias evita ese problema sin omitir validaciones.
var databaseConnection = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
var supabaseTransactionPooler = databaseConnection.Port == 6543
    && databaseConnection.Host?.EndsWith(".pooler.supabase.com", StringComparison.OrdinalIgnoreCase) == true;
databaseConnection.Pooling = builder.Configuration.GetValue<bool?>("Database:ClientPooling")
    ?? (supabaseTransactionPooler ? false : databaseConnection.Pooling);

builder.Services.AddDbContext<DeltaTestContext>(options =>
    options.UseNpgsql(
        databaseConnection.ConnectionString,
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(5))
);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HU12: registro en memoria de sesiones revocadas al cerrar sesión.
builder.Services.AddSingleton<SessionRevocationStore>();

// HU12: cookie de autenticación endurecida.
// El cliente y la API corren en orígenes distintos (puertos diferentes), por lo
// que SameSite=None + Secure es necesario. Puede cambiarse con Auth:CookieSameSite.
var sameSiteValue = builder.Configuration["Auth:CookieSameSite"] ?? "None";
var cookieSameSite = Enum.TryParse<SameSiteMode>(sameSiteValue, ignoreCase: true, out var parsedSameSite)
    ? parsedSameSite
    : SameSiteMode.None;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    // Para peticiones /api no se redirige a una página HTML: se responde 401/403.
    options.LoginPath = "/api/auth/login";
    options.LogoutPath = "/api/auth/logout";
    options.AccessDeniedPath = "/api/auth/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.Cookie.Name = "__Host-DeltaAuth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = cookieSameSite;
    options.Cookie.Domain = null;
    options.Cookie.Path = "/";
    options.Cookie.IsEssential = true;

    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        },
        OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        },
        OnValidatePrincipal = async context =>
        {
            var sessionId = context.Principal?.FindFirst("sid")?.Value;
            var revocationStore = context.HttpContext.RequestServices
                .GetRequiredService<SessionRevocationStore>();

            if (revocationStore.IsRevoked(sessionId))
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            // HU12: si la cuenta fue desactivada o eliminada, invalidar la cookie.
            var userIdValue = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdValue, out var userId) || userId <= 0)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            var db = context.HttpContext.RequestServices.GetRequiredService<DeltaTestContext>();
            var usuario = await db.Usuarios
                .AsNoTracking()
                .Where(u => u.IdUsuario == userId)
                .Select(u => new SessionUserSnapshot { IdUsuario = u.IdUsuario, NombreCompleto = u.NombreCompleto, Correo = u.Correo, Estado = u.Estado, Rol = u.Rol })
                .FirstOrDefaultAsync();

            var activo = usuario?.Estado != null &&
                         usuario.Estado.Equals("Activo", StringComparison.OrdinalIgnoreCase);

            // Si el rol cambió, la cookie queda obsoleta: forzar un nuevo inicio de sesión.
            var rolCookie = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var rolActualizado = string.Equals(rolCookie, usuario?.Rol, StringComparison.OrdinalIgnoreCase);

            if (!activo || !rolActualizado)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }
            context.HttpContext.Items[typeof(SessionUserSnapshot)] = usuario;
        }
    };
});

// Fase 1: por defecto todo requiere autenticación. Cada endpoint público debe
// declarar [AllowAnonymous] de forma explícita.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

// Respuestas de error uniformes (ProblemDetails) sin stack traces ni mensajes internos.
app.UseExceptionHandler();

// Cabeceras de seguridad para las respuestas de la API.
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "no-referrer";
    headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
    headers["Content-Security-Policy"] =
        "default-src 'self'; script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; font-src 'self' https://cdn.jsdelivr.net; " +
        "img-src 'self' data:; connect-src 'self'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

// Enable CORS for requests from the client - Debe ir ANTES de Authentication y Authorization
app.UseCors("AllowClient");

// CSRF: las peticiones que modifican estado deben incluir una cabecera que un
// formulario HTML de otro origen no puede establecer sin pasar por CORS.
app.Use(async (context, next) =>
{
    var method = context.Request.Method;
    var isSafeMethod = HttpMethods.IsGet(method)
        || HttpMethods.IsHead(method)
        || HttpMethods.IsOptions(method)
        || HttpMethods.IsTrace(method);

    if (context.Request.Path.StartsWithSegments("/api") && !isSafeMethod)
    {
        if (!context.Request.Headers.TryGetValue("X-Requested-With", out var requestedWith)
            || !string.Equals(requestedWith.ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsJsonAsync(new { mensaje = "Solicitud no válida." });
            return;
        }
    }

    await next();
});

// Fase 0: limita los intentos de login por IP.
app.UseRateLimiter();

app.UseAuthentication(); // Muy importante: primero autenticación
app.UseAuthorization();

// Map MVC default route so views can be served
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.Run();

using DELTATEST;
using DELTATEST.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// La URL de la API es configurable en wwwroot/appsettings.json.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7287/";
if (!Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var apiBase))
    throw new InvalidOperationException("ApiBaseUrl no es una URL absoluta válida.");

// Register message handler for requests - DEBE IR ANTES de AddHttpClient
builder.Services.AddTransient<AuthorizationMessageHandler>();

// Configure a named HttpClient for API calls with credentials support for cookies
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = apiBase;
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(120);
})
.AddHttpMessageHandler<AuthorizationMessageHandler>();

// HU11: Cliente dedicado para el Asistente de IA (la generación con Gemini puede tardar más de 30 s)
builder.Services.AddHttpClient("AI", client =>
{
    client.BaseAddress = apiBase;
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromMinutes(3);
})
.AddHttpMessageHandler<AuthorizationMessageHandler>();

// Provide an injectable HttpClient that uses the named client
builder.Services.AddScoped(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return factory.CreateClient("API");
});

// HU12: el estado de autenticación vive en una cookie HttpOnly validada por el servidor.
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<CookieAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CookieAuthenticationStateProvider>());

// Register application services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ReporteEvaluacionService>();
builder.Services.AddScoped<EvaluadoService>();

await builder.Build().RunAsync();

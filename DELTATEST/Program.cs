using DELTATEST;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using DELTATEST.Services;
using System.Net.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Register message handler for requests - DEBE IR ANTES de AddHttpClient
builder.Services.AddTransient<AuthorizationMessageHandler>();

// Configure a named HttpClient for API calls with credentials support for cookies
// Use the local API URL for development. Change if your API runs on a different port.
var apiBase = new Uri("https://localhost:7287/");
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = apiBase;
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddHttpMessageHandler<AuthorizationMessageHandler>();

// Provide an injectable HttpClient that uses the named client
builder.Services.AddScoped(sp => 
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = factory.CreateClient("API");
    // Asegurar que las cookies se incluyan
    httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
    return httpClient;
});

// Register application services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AuthStateService>();
builder.Services.AddScoped<ReporteEvaluacionService>();
builder.Services.AddScoped<EvaluadoService>();

Console.WriteLine("Blazor app initializing...");

await builder.Build().RunAsync();

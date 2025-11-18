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

// Register message handler that attaches the JWT from localStorage
builder.Services.AddTransient<AuthorizationMessageHandler>();

// Configure a named HttpClient for API calls and add the authorization handler
// Use the local API URL for development. Change if your API runs on a different port.
var apiBase = new Uri("https://localhost:7287/");
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = apiBase;
}).AddHttpMessageHandler<AuthorizationMessageHandler>();

// Provide an injectable HttpClient that uses the named client
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

// Register application services
builder.Services.AddScoped<AuthService>();

await builder.Build().RunAsync();

using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Authorization;

namespace DELTATEST.Services
{
    /// <summary>
    /// HU12/HU13: el estado de autenticación se obtiene del servidor a través de
    /// la cookie HttpOnly. El navegador nunca lee ni guarda roles en localStorage.
    /// </summary>
    public class CookieAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _http;
        private static readonly AuthenticationState Anonymous =
            new(new ClaimsPrincipal(new ClaimsIdentity()));

        public CookieAuthenticationStateProvider(HttpClient http)
        {
            _http = http;
        }

        private Task<AuthenticationState>? consultaPendiente;
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => consultaPendiente ??= ConsultarAsync();

        private async Task<AuthenticationState> ConsultarAsync()
        {
            await Task.Yield();
            try
            {
                using var response = await _http.GetAsync("api/auth/current-user");
                if (!response.IsSuccessStatusCode)
                    return Anonymous;

                var user = await response.Content.ReadFromJsonAsync<CurrentUserResponse?>();
                if (user == null || user.IdUsuario <= 0)
                    return Anonymous;

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.IdUsuario.ToString()),
                    new(ClaimTypes.Name, user.NombreCompleto ?? string.Empty),
                    new(ClaimTypes.Role, string.IsNullOrWhiteSpace(user.Rol) ? "Usuario" : user.Rol)
                };

                if (!string.IsNullOrWhiteSpace(user.Correo))
                    claims.Add(new Claim(ClaimTypes.Email, user.Correo));

                var identity = new ClaimsIdentity(claims, "ServerCookie");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return Anonymous;
            }
            catch
            {
                return Anonymous;
            }
            finally { consultaPendiente = null; }
        }

        public async Task RefreshAsync()
        {
            var state = await GetAuthenticationStateAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(state));
        }

        public sealed class CurrentUserResponse
        {
            [JsonPropertyName("idUsuario")]
            public int IdUsuario { get; set; }

            [JsonPropertyName("nombreCompleto")]
            public string? NombreCompleto { get; set; }

            [JsonPropertyName("correo")]
            public string? Correo { get; set; }

            [JsonPropertyName("rol")]
            public string? Rol { get; set; }
        }
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DELTATEST.Models;

namespace DELTATEST.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly CookieAuthenticationStateProvider _authStateProvider;

        public AuthService(HttpClient http, CookieAuthenticationStateProvider authStateProvider)
        {
            _http = http;
            _authStateProvider = authStateProvider;
        }

        public async Task<(bool success, string? rol, string? message, int? idUsuario)> LoginAsync(LoginModel model)
        {
            try
            {
                // Enviar Username (CI o correo) y Password
                var payload = new { Username = model.Username, Password = model.Contrasena };
                var response = await _http.PostAsJsonAsync("api/auth/login", payload);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse?>();
                    // La cookie ya fue emitida por el servidor; refrescar el estado.
                    await _authStateProvider.RefreshAsync();

                    if (result != null)
                        return (true, result.Rol, null, result.IdUsuario);

                    return (false, null, "Respuesta inválida del servidor.", null);
                }

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    var message = await ReadErrorMessage(
                        response,
                        "Demasiados intentos. Intente nuevamente en un minuto.");
                    return (false, null, message, null);
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized ||
                    response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var message = await ReadErrorMessage(
                        response,
                        "Usuario o contraseña incorrectos");
                    return (false, null, message, null);
                }

                return (false, null, "No se pudo iniciar sesión. Intente nuevamente.", null);
            }
            catch (Exception)
            {
                return (false, null, "No se pudo conectar con el servidor. Intente nuevamente.", null);
            }
        }

        public async Task<(bool success, string? rol, string? message, string? nombre)> RegisterAsync(RegisterModel model)
        {
            try
            {
                // Aseguramos que FechaIngreso se serialice en formato yyyy-MM-dd si existe
                string? fechaStr = null;
                if (model.FechaIngreso.HasValue)
                {
                    fechaStr = model.FechaIngreso.Value.ToString("yyyy-MM-dd");
                }

                // El servidor decide el rol y toma el creador desde la cookie.
                var payload = new
                {
                    NombreCompleto = model.NombreCompleto,
                    Ci = model.Ci,
                    Expedicion = model.Expedicion,
                    Correo = model.Correo,
                    Telefono = model.Telefono,
                    Role = model.Rol,
                    FechaIngreso = fechaStr,
                    Estado = model.Estado,
                    TipoEvaluado = model.TipoEvaluado,
                    PuestoActual = model.PuestoActual,
                    PuestoSolicitado = model.PuestoSolicitado,
                    PuestoARotar = model.PuestoARotar,
                    Password = model.Password
                };

                var response = await _http.PostAsJsonAsync("api/auth/register", payload);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<RegisterResponse?>();
                    return (true, result?.Rol ?? model.Rol ?? "Usuario", null, model.NombreCompleto);
                }

                var error = await ReadErrorMessage(response, "No se pudo crear la cuenta.");
                return (false, null, error, null);
            }
            catch (Exception)
            {
                return (false, null, "No se pudo conectar con el servidor. Intente nuevamente.", null);
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                using var response = await _http.PostAsync("api/auth/logout", null);
                if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.Unauthorized)
                    return false;
                await _authStateProvider.RefreshAsync();
                return true;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return false;
            }
        }

        public async Task<(bool isAuthenticated, string? userName, string? userRole, int? userId)> GetCurrentUserAsync()
        {
            var state = await _authStateProvider.GetAuthenticationStateAsync();
            var user = state.User;

            if (user.Identity?.IsAuthenticated != true)
                return (false, null, null, null);

            var idValue = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int? id = int.TryParse(idValue, out var parsed) ? parsed : null;

            return (
                true,
                user.Identity.Name,
                user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value,
                id);
        }

        public async Task<bool> UpdateUsuario(int id, UserViewModel model)
        {
            var response = await _http.PutAsJsonAsync($"api/usuarios/{id}", model);
            return response.IsSuccessStatusCode;
        }

        private static async Task<string> ReadErrorMessage(HttpResponseMessage response, string fallback)
        {
            var text = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(text))
                return fallback;

            try
            {
                using var document = JsonDocument.Parse(text);
                if (document.RootElement.ValueKind == JsonValueKind.String)
                    return document.RootElement.GetString() ?? fallback;

                if (document.RootElement.TryGetProperty("mensaje", out var message) &&
                    message.ValueKind == JsonValueKind.String)
                    return message.GetString() ?? fallback;
            }
            catch (JsonException)
            {
                // Respuesta de texto plano.
            }

            return text.Trim();
        }

        public sealed class LoginResponse
        {
            [JsonPropertyName("idUsuario")]
            public int IdUsuario { get; set; }

            [JsonPropertyName("nombreCompleto")]
            public string NombreCompleto { get; set; } = string.Empty;

            [JsonPropertyName("rol")]
            public string Rol { get; set; } = string.Empty;

            [JsonPropertyName("mensaje")]
            public string? Mensaje { get; set; }
        }

        public sealed class RegisterResponse
        {
            [JsonPropertyName("idUsuario")]
            public int IdUsuario { get; set; }

            [JsonPropertyName("rol")]
            public string? Rol { get; set; }
        }
    }
}

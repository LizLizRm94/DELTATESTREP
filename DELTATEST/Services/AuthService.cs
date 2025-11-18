using System.Net.Http.Json;
using DELTATEST.Models;
using Blazored.LocalStorage;
using System.Text.Json.Serialization;

namespace DELTATEST.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;

        public AuthService(HttpClient http, ILocalStorageService localStorage)
        {
            _http = http;
            _localStorage = localStorage;
        }

        public async Task<(bool success, string? message)> LoginAsync(LoginModel model)
        {
            try
            {
                // Ahora enviamos Username (CI o correo) y Password
                var payload = new { Username = model.Username, Password = model.Contrasena };
                var response = await _http.PostAsJsonAsync("api/auth/login", payload);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse?>();
                    if (result != null)
                    {
                        await _localStorage.SetItemAsync("authToken", result.Token);
                        await _localStorage.SetItemAsync("userName", result.NombreCompleto);
                        await _localStorage.SetItemAsync("userRole", result.Rol);

                        return (true, null);
                    }

                    return (false, "Respuesta inválida del servidor.");
                }

                var error = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(error) ? response.ReasonPhrase : error);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool success, string? message)> RegisterAsync(RegisterModel model)
        {
            try
            {
                var payload = new
                {
                    NombreCompleto = model.NombreCompleto,
                    Ci = model.Ci,
                    Correo = model.Correo,
                    Telefono = model.Telefono,
                    Rol = model.Rol,
                    FechaIngreso = model.FechaIngreso,
                    Estado = model.Estado,
                    PuestoActual = model.PuestoActual,
                    PuestoSolicitado = model.PuestoSolicitado,
                    PuestoARotar = model.PuestoARotar,
                    Password = model.Password
                };

                var response = await _http.PostAsJsonAsync("api/auth/register", payload);
                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    return (true, null);
                }

                var error = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(error) ? response.ReasonPhrase : error);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<string?> GetTokenAsync() => await _localStorage.GetItemAsync<string?>("authToken");

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("userName");
            await _localStorage.RemoveItemAsync("userRole");
        }

        private class LoginResponse
        {
            [JsonPropertyName("token")]
            public string Token { get; set; } = string.Empty;

            [JsonPropertyName("IdUsuario")]
            public int IdUsuario { get; set; }

            [JsonPropertyName("NombreCompleto")]
            public string NombreCompleto { get; set; } = string.Empty;

            [JsonPropertyName("Rol")]
            public string Rol { get; set; } = string.Empty;
        }
    }
}
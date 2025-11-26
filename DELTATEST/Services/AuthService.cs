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

    public async Task<(bool success, string? rol, string? message)> LoginAsync(LoginModel model)
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

                    return (true, result.Rol, null);
                }

                return (false, null, "Respuesta inválida del servidor.");
            }

            var error = await response.Content.ReadAsStringAsync();
            return (false, null, string.IsNullOrWhiteSpace(error) ? response.ReasonPhrase : error);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
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

            var payload = new
            {
                NombreCompleto = model.NombreCompleto,
                Ci = model.Ci,
                Correo = model.Correo,
                Telefono = model.Telefono,
                Role = model.Rol, // coincide con el DTO del API
                FechaIngreso = fechaStr,
                Estado = model.Estado,
                PuestoActual = model.PuestoActual,
                PuestoSolicitado = model.PuestoSolicitado,
                PuestoARotar = model.PuestoARotar,
                Password = model.Password
            };

            var response = await _http.PostAsJsonAsync("api/auth/register", payload);
            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                // Devuelve éxito, rol y nombre registrado
                return (true, model.Rol, null, model.NombreCompleto);
            }

            var error = await response.Content.ReadAsStringAsync();
            return (false, null, string.IsNullOrWhiteSpace(error) ? response.ReasonPhrase : error, null);
        }
        catch (Exception ex)
        {
            // En caso de excepción, no se puede devolver null en bool
            return (false, null, ex.Message, null);
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

        public async Task<bool> UpdateUsuario(int id, UserViewModel model)
        {
            var response = await _http.PutAsJsonAsync($"api/usuarios/{id}", model);
            return response.IsSuccessStatusCode;
        }


    }

}
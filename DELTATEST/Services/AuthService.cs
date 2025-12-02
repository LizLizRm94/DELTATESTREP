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

    public async Task<(bool success, string? rol, string? message, int? idUsuario)> LoginAsync(LoginModel model)
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
                    // La cookie se mantiene automáticamente en el navegador
                    // Guardamos información del usuario localmente para acceso rápido
                    await _localStorage.SetItemAsync("userName", result.NombreCompleto);
                    await _localStorage.SetItemAsync("userRole", result.Rol);
                    await _localStorage.SetItemAsync("userId", result.IdUsuario);

                    return (true, result.Rol, null, result.IdUsuario);
                }

                return (false, null, "Respuesta inválida del servidor.", null);
            }

            var error = await response.Content.ReadAsStringAsync();
            return (false, null, string.IsNullOrWhiteSpace(error) ? response.ReasonPhrase : error, null);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message, null);
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

    public async Task LogoutAsync()
    {
        try
        {
            // Enviar solicitud de logout al servidor para limpiar la cookie
            await _http.PostAsync("api/auth/logout", null);
        }
        catch
        {
            // Continuar incluso si falla la solicitud de logout remota
        }

        // Limpiar almacenamiento local
        await _localStorage.RemoveItemAsync("userName");
        await _localStorage.RemoveItemAsync("userRole");
        await _localStorage.RemoveItemAsync("userId");
    }

    public async Task<(bool isAuthenticated, string? userName, string? userRole, int? userId)> GetCurrentUserAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/auth/current-user");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CurrentUserResponse?>();
                if (result != null)
                {
                    await _localStorage.SetItemAsync("userName", result.NombreCompleto);
                    await _localStorage.SetItemAsync("userRole", result.Rol);
                    await _localStorage.SetItemAsync("userId", result.IdUsuario);
                    
                    return (true, result.NombreCompleto, result.Rol, result.IdUsuario);
                }
            }

            return (false, null, null, null);
        }
        catch
        {
            return (false, null, null, null);
        }
    }

    private class LoginResponse
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

    private class CurrentUserResponse
    {
        [JsonPropertyName("idUsuario")]
        public int IdUsuario { get; set; }

        [JsonPropertyName("nombreCompleto")]
        public string NombreCompleto { get; set; } = string.Empty;

        [JsonPropertyName("correo")]
        public string? Correo { get; set; }

        [JsonPropertyName("rol")]
        public string Rol { get; set; } = string.Empty;
    }

    public async Task<bool> UpdateUsuario(int id, UserViewModel model)
    {
        var response = await _http.PutAsJsonAsync($"api/usuarios/{id}", model);
        return response.IsSuccessStatusCode;
    }
    }

}
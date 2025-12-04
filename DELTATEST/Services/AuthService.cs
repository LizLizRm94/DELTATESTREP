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
            // Enviar Username (CI o correo) y Password
            var payload = new { Username = model.Username, Password = model.Contrasena };
            var response = await _http.PostAsJsonAsync("api/auth/login", payload);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse?>();
                if (result != null)
                {
                    // Guardar información del usuario en localStorage para acceso rápido
                    // Esto es especialmente importante en WebAssembly donde las cookies pueden no funcionar directamente
                    await _localStorage.SetItemAsync("userName", result.NombreCompleto);
                    await _localStorage.SetItemAsync("userRole", result.Rol);
                    await _localStorage.SetItemAsync("userId", result.IdUsuario);
                    await _localStorage.SetItemAsync("isAuthenticated", true);

                    Console.WriteLine($"Login exitoso: {result.NombreCompleto}, Rol: {result.Rol}");
                    return (true, result.Rol, null, result.IdUsuario);
                }

                return (false, null, "Respuesta inválida del servidor.", null);
            }

            var error = await response.Content.ReadAsStringAsync();
            return (false, null, string.IsNullOrWhiteSpace(error) ? response.ReasonPhrase : error, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login error: {ex.Message}");
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
                Role = model.Rol,
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
                return (true, model.Rol, null, model.NombreCompleto);
            }

            var error = await response.Content.ReadAsStringAsync();
            return (false, null, string.IsNullOrWhiteSpace(error) ? response.ReasonPhrase : error, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Register error: {ex.Message}");
            return (false, null, ex.Message, null);
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            // Enviar solicitud de logout al servidor para limpiar la cookie
            await _http.PostAsync("api/auth/logout", null);
            Console.WriteLine("Logout request sent to server");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Logout error: {ex.Message}");
        }

        // Limpiar almacenamiento local
        await _localStorage.RemoveItemAsync("userName");
        await _localStorage.RemoveItemAsync("userRole");
        await _localStorage.RemoveItemAsync("userId");
        await _localStorage.RemoveItemAsync("isAuthenticated");
        Console.WriteLine("Local storage cleared");
    }

    public async Task<(bool isAuthenticated, string? userName, string? userRole, int? userId)> GetCurrentUserAsync()
    {
        try
        {
            // Primero, intentar obtener del localStorage (es más confiable en WebAssembly)
            bool isAuth = false;
            try
            {
                isAuth = await _localStorage.GetItemAsync<bool>("isAuthenticated");
            }
            catch
            {
                // Si hay error al obtener, isAuth permanece como false
                isAuth = false;
            }
            
            Console.WriteLine($"GetCurrentUserAsync - isAuthenticated flag from localStorage: {isAuth}");
            
            if (isAuth)
            {
                var userName = await _localStorage.GetItemAsync<string>("userName");
                var userRole = await _localStorage.GetItemAsync<string>("userRole");
                var userId = await _localStorage.GetItemAsync<int?>("userId");
                
                Console.WriteLine($"Auth found in localStorage: {userName}, Role: {userRole}");
                
                // Si tenemos datos en localStorage, consideramos al usuario como autenticado
                if (!string.IsNullOrEmpty(userName))
                {
                    Console.WriteLine($"? Returning authenticated user from localStorage: {userName}");
                    return (true, userName, userRole, userId);
                }
            }
            
            Console.WriteLine($"No auth data in localStorage, attempting server validation...");
            
            // Si no hay en localStorage, intentar obtener del servidor
            var response = await _http.GetAsync("api/auth/current-user");
            
            Console.WriteLine($"GetCurrentUser response status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CurrentUserResponse?>();
                if (result != null)
                {
                    await _localStorage.SetItemAsync("userName", result.NombreCompleto);
                    await _localStorage.SetItemAsync("userRole", result.Rol);
                    await _localStorage.SetItemAsync("userId", result.IdUsuario);
                    await _localStorage.SetItemAsync("isAuthenticated", true);
                    
                    Console.WriteLine($"? Auth confirmed from server: {result.NombreCompleto}");
                    return (true, result.NombreCompleto, result.Rol, result.IdUsuario);
                }
            }
            else
            {
                Console.WriteLine($"GetCurrentUser failed with status: {response.StatusCode}");
            }

            Console.WriteLine($"? No authentication found");
            return (false, null, null, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetCurrentUserAsync error: {ex.Message}");
            
            // Fallback: intentar obtener del local storage en caso de error
            try
            {
                bool isAuth = false;
                try
                {
                    isAuth = await _localStorage.GetItemAsync<bool>("isAuthenticated");
                }
                catch
                {
                    isAuth = false;
                }
                
                if (isAuth)
                {
                    var userName = await _localStorage.GetItemAsync<string>("userName");
                    var userRole = await _localStorage.GetItemAsync<string>("userRole");
                    var userId = await _localStorage.GetItemAsync<int?>("userId");
                    
                    Console.WriteLine($"? Using cached auth data (from fallback): {userName}");
                    return (true, userName, userRole, userId);
                }
            }
            catch (Exception fallbackEx) 
            { 
                Console.WriteLine($"Fallback error: {fallbackEx.Message}");
            }
            
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
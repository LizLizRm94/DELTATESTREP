using System;
using System.Threading.Tasks;
using Blazored.LocalStorage;

namespace DELTATEST.Services
{
    /// <summary>
    /// Servicio para mantener el estado de autenticación sincronizado en toda la aplicación
    /// </summary>
    public class AuthStateService
    {
        private readonly AuthService _authService;
        private readonly ILocalStorageService _localStorage;
        
        private bool _isAuthenticated = false;
        private string? _userName;
        private string? _userRole;
        private int? _userId;

        public event Action? OnAuthenticationStateChanged;

        public bool IsAuthenticated => _isAuthenticated;
        public string? UserName => _userName;
        public string? UserRole => _userRole;
        public int? UserId => _userId;

        public AuthStateService(AuthService authService, ILocalStorageService localStorage)
        {
            _authService = authService;
            _localStorage = localStorage;
        }

        /// <summary>
        /// Inicializa el estado de autenticación desde localStorage
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                Console.WriteLine("[AuthStateService] Initializing...");
                
                // Intentar obtener del localStorage primero
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
                    _userName = await _localStorage.GetItemAsync<string>("userName");
                    _userRole = await _localStorage.GetItemAsync<string>("userRole");
                    _userId = await _localStorage.GetItemAsync<int?>("userId");
                    _isAuthenticated = !string.IsNullOrEmpty(_userName);
                    
                    Console.WriteLine($"[AuthStateService] Initialized from localStorage: {_userName} ({_userRole})");
                }
                else
                {
                    // Si no hay en localStorage, intentar validar con servidor
                    var (isAuth2, userName2, userRole2, userId2) = await _authService.GetCurrentUserAsync();
                    _isAuthenticated = isAuth2;
                    _userName = userName2;
                    _userRole = userRole2;
                    _userId = userId2;
                    
                    if (isAuth2)
                    {
                        Console.WriteLine($"[AuthStateService] Initialized from server: {_userName} ({_userRole})");
                    }
                    else
                    {
                        Console.WriteLine($"[AuthStateService] No authentication found");
                    }
                }

                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthStateService] Initialization error: {ex.Message}");
                _isAuthenticated = false;
                NotifyStateChanged();
            }
        }

        /// <summary>
        /// Marca al usuario como autenticado (generalmente después del login)
        /// </summary>
        public async Task SetAuthenticatedAsync(string userName, string userRole, int userId)
        {
            _isAuthenticated = true;
            _userName = userName;
            _userRole = userRole;
            _userId = userId;

            await _localStorage.SetItemAsync("isAuthenticated", true);
            await _localStorage.SetItemAsync("userName", userName);
            await _localStorage.SetItemAsync("userRole", userRole);
            await _localStorage.SetItemAsync("userId", userId);

            Console.WriteLine($"[AuthStateService] User authenticated: {userName} ({userRole})");
            NotifyStateChanged();
        }

        /// <summary>
        /// Limpia el estado de autenticación (logout)
        /// </summary>
        public async Task ClearAuthenticationAsync()
        {
            _isAuthenticated = false;
            _userName = null;
            _userRole = null;
            _userId = null;

            await _localStorage.RemoveItemAsync("isAuthenticated");
            await _localStorage.RemoveItemAsync("userName");
            await _localStorage.RemoveItemAsync("userRole");
            await _localStorage.RemoveItemAsync("userId");

            Console.WriteLine($"[AuthStateService] User authentication cleared");
            NotifyStateChanged();
        }

        /// <summary>
        /// Notifica a los suscriptores que el estado cambió
        /// </summary>
        private void NotifyStateChanged()
        {
            OnAuthenticationStateChanged?.Invoke();
        }

        /// <summary>
        /// Obtiene el estado actual de autenticación
        /// </summary>
        public (bool isAuthenticated, string? userName, string? userRole, int? userId) GetCurrentState()
        {
            return (_isAuthenticated, _userName, _userRole, _userId);
        }
    }
}

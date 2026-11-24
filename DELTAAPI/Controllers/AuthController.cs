using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using DELTAAPI.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using DELTAAPI.Security;


namespace DELTAAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DeltaTestContext _context;
        private readonly ILogger<AuthController> _logger;
        private readonly SessionRevocationStore _sessionRevocationStore;

        private static readonly HashSet<string> ExtensionesValidas = new(StringComparer.Ordinal)
        {
            "LP", "SC", "CB", "OR", "PT", "TJ", "BN", "CH", "PD"
        };

        // HU13: hash de relleno para que el tiempo de respuesta no revele si el usuario existe.
        private static readonly string DummyPasswordHash =
            BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N"), workFactor: 11);

        private const string MensajeCredencialesInvalidas = "Usuario o contraseña incorrectos";

        public AuthController(
            DeltaTestContext context,
            ILogger<AuthController> logger,
            SessionRevocationStore sessionRevocationStore)
        {
            _context = context;
            _logger = logger;
            _sessionRevocationStore = sessionRevocationStore;
        }

        public class RegisterDto
        {
            public string? NombreCompleto { get; set; }
            public string? Ci { get; set; }
            public string? Expedicion { get; set; }
            public string? Correo { get; set; }
            public string? Telefono { get; set; }
            public string? FechaIngreso { get; set; } // formato ISO yyyy-MM-dd esperado desde <input type="date">
            public string? Password { get; set; }
            public string? Role { get; set; }
            public string? TipoEvaluado { get; set; }
            public string? PuestoActual { get; set; }
            public string? PuestoARotar { get; set; }
            public string? PuestoSolicitado { get; set; }
        }

        [HttpPost("register")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.NombreCompleto) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Nombre completo y contraseña son obligatorios.");

            if (string.IsNullOrWhiteSpace(dto.Correo) || !System.Net.Mail.MailAddress.TryCreate(dto.Correo.Trim(), out _))
                return BadRequest("Correo no válido.");

            var errorLaboral = Delta.Contracts.VinculacionLaboral.Validar(dto.TipoEvaluado,dto.PuestoActual,dto.PuestoARotar,dto.PuestoSolicitado);
            if (errorLaboral != null) return BadRequest(errorLaboral);

            var passwordError = PasswordPolicy.Validate(dto.Password);
            if (passwordError != null)
                return BadRequest(passwordError);

            var correo = dto.Correo.Trim();

            // Normalizar y validar la extensión (sigla del departamento)
            string? expedicion = null;
            if (!string.IsNullOrWhiteSpace(dto.Expedicion))
            {
                expedicion = dto.Expedicion.Trim().ToUpperInvariant();
                if (!ExtensionesValidas.Contains(expedicion))
                    return BadRequest("Extensión no válida.");
            }

            // Validar correo/ci únicos si se proporcionan (sin distinguir mayúsculas)
            try
            {
                var existsCorreo = await _context.Usuarios
                    .AnyAsync(u => u.Correo != null && u.Correo.ToLower() == correo.ToLower());
                if (existsCorreo) return BadRequest("Correo ya registrado.");

                if (!string.IsNullOrWhiteSpace(dto.Ci))
                {
                    var ci = dto.Ci.Trim();
                    var existsCi = await _context.Usuarios
                        .AnyAsync(u => u.Ci != null && u.Ci.ToLower() == ci.ToLower());
                    if (existsCi) return BadRequest("CI ya registrado.");
                }
            }
            catch (System.Net.Sockets.SocketException sockEx)
            {
                _logger.LogError(sockEx, "Register: error de red al consultar usuarios (posible host desconocido)");
                return StatusCode(503, "Servicio de base de datos no disponible (host desconocido o inaccesible).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register: error al consultar usuarios");
                return StatusCode(500, "Error interno del servidor.");
            }

            // Política de roles: solo un Administrador autenticado puede crear cuentas.
            // Si no se indica rol, se crea un "Usuario".
            var requestedRole = string.IsNullOrWhiteSpace(dto.Role) ? "Usuario" : dto.Role.Trim();
            var roleNormalized = NormalizeRole(requestedRole);

            if (roleNormalized == null)
                return BadRequest("Rol no válido.");

            // El creador SIEMPRE se toma de la cookie autenticada. Nunca del cuerpo.
            var idCreadoPor = User.GetUserId();

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var usuario = new Usuario
            {
                NombreCompleto = dto.NombreCompleto!.Trim(),
                Ci = string.IsNullOrWhiteSpace(dto.Ci) ? string.Empty : dto.Ci.Trim(),
                Expedicion = expedicion,
                Correo = correo,
                Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim(),
                Contraseña = hashedPassword, 
                Rol = roleNormalized,
                Estado = "Activo",
                TipoEvaluado = dto.TipoEvaluado,
                PuestoActual = dto.TipoEvaluado == "Empleado" ? dto.PuestoActual?.Trim() : null,
                PuestoARotar = dto.TipoEvaluado == "Empleado" ? dto.PuestoARotar?.Trim() : null,
                PuestoSolicitado = dto.TipoEvaluado == "Pasante" ? dto.PuestoSolicitado?.Trim() : null,
                IdCreadoPor = idCreadoPor
            };

            // Parse FechaIngreso si viene
            if (!string.IsNullOrWhiteSpace(dto.FechaIngreso))
            {
                if (DateOnly.TryParse(dto.FechaIngreso, out var d))
                {
                    usuario.FechaIngreso = d;
                }
            }

            try
            {
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Register: no se pudo guardar el usuario.");
                return StatusCode(500, "No se pudo completar el registro.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register: error inesperado al guardar el usuario.");
                return StatusCode(500, "Error interno del servidor.");
            }

            return StatusCode(StatusCodes.Status201Created, new
            {
                idUsuario = usuario.IdUsuario,
                rol = usuario.Rol
            });
        }

        /// <summary>
        /// Verifica la contraseña sin lanzar excepciones si el hash almacenado es
        /// inválido o está vacío. Siempre consume tiempo de BCrypt para no revelar
        /// si la cuenta existe o si su hash es válido.
        /// </summary>
        private static bool VerifyPassword(string password, string? hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
            {
                BCrypt.Net.BCrypt.Verify(password, DummyPasswordHash);
                return false;
            }

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                BCrypt.Net.BCrypt.Verify(password, DummyPasswordHash);
                return false;
            }
        }

        private static string? NormalizeRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return "Usuario";

            var normalized = role.Trim();
            if (normalized.Equals("Usuario", StringComparison.OrdinalIgnoreCase)) return "Usuario";
            if (normalized.Equals("Inspector", StringComparison.OrdinalIgnoreCase)) return "Inspector";
            if (normalized.Equals("Administrador", StringComparison.OrdinalIgnoreCase)) return "Administrador";

            return null;
        }

        // Login DTO esperado por el cliente
        public class LoginDto
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Usuario y contraseña son obligatorios.");

            var usernameNormalized = dto.Username.Trim();
            var usernameLower = usernameNormalized.ToLowerInvariant();

            try
            {
                // Buscar usuario por correo o CI, comparando en minusculas y sin espacios
                var user = await _context.Usuarios
                    .FirstOrDefaultAsync(u =>
                        (!string.IsNullOrEmpty(u.Correo) && u.Correo.ToLower() == usernameLower) ||
                        (!string.IsNullOrEmpty(u.Ci) && u.Ci.ToLower() == usernameLower)
                    );

                // HU13: misma respuesta y mismo tiempo cuando el usuario no existe.
                if (user == null)
                {
                    VerifyPassword(dto.Password, null);
                    _logger.LogWarning("Login failed.");
                    return Unauthorized(MensajeCredencialesInvalidas);
                }

                // Un usuario deshabilitado no debe poder iniciar sesión.
                if (string.IsNullOrWhiteSpace(user.Estado) ||
                    !user.Estado.Equals("Activo", StringComparison.OrdinalIgnoreCase))
                {
                    VerifyPassword(dto.Password, user.Contraseña);
                    _logger.LogWarning("Login blocked: inactive user {UserId}.", user.IdUsuario);
                    return Unauthorized(MensajeCredencialesInvalidas);
                }

                // No recortar la contraseña: debe coincidir exactamente con la registrada.
                if (!VerifyPassword(dto.Password, user.Contraseña))
                {
                    _logger.LogWarning("Login failed.");
                    return Unauthorized(MensajeCredencialesInvalidas);
                }

                // HU12: identificador de sesión para poder revocarla en el servidor.
                var sessionId = Guid.NewGuid().ToString("N");
                _sessionRevocationStore.RemoveOlderThan(TimeSpan.FromHours(24));

                // Crear claims para la cookie
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.IdUsuario.ToString()),
                    new Claim(ClaimTypes.Name, user.NombreCompleto ?? string.Empty),
                    new Claim(ClaimTypes.Role, user.Rol ?? "Usuario"),
                    new Claim("sid", sessionId)
                };

                if (!string.IsNullOrWhiteSpace(user.Correo))
                    claims.Add(new Claim(ClaimTypes.Email, user.Correo));

                // Crear la identidad y principal para la cookie
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // HU12: sesión no persistente y expiración por inactividad (30 min deslizantes).
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    new AuthenticationProperties
                    {
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                        IsPersistent = false,
                        AllowRefresh = true
                    });

                var result = new
                {
                    IdUsuario = user.IdUsuario,
                    NombreCompleto = user.NombreCompleto,
                    Rol = user.Rol,
                    Mensaje = "Login exitoso"
                };

                _logger.LogInformation("Login successful for user {UserId}.", user.IdUsuario);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error.");
                return StatusCode(500, "Error interno del servidor.");
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // HU12: revocar la sesión en el servidor antes de borrar la cookie.
            _sessionRevocationStore.Revoke(User.FindFirst("sid")?.Value);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { mensaje = "Logout exitoso" });
        }

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [HttpGet("current-user")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                if (!User.Identity?.IsAuthenticated ?? false)
                {
                    _logger.LogWarning("Current-user request without authentication");
                    return Unauthorized("No autorizado");
                }

                var idUsuarioStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(idUsuarioStr, out var idUsuario))
                {
                    _logger.LogWarning("Invalid user ID in claims");
                    return Unauthorized("Token inválido");
                }

                if (HttpContext.Items[typeof(SessionUserSnapshot)] is SessionUserSnapshot validado)
                    return Ok(new { idUsuario = validado.IdUsuario, nombreCompleto = validado.NombreCompleto, correo = validado.Correo, rol = validado.Rol });

                var user = await _context.Usuarios.FindAsync(idUsuario);
                if (user == null)
                {
                    _logger.LogWarning($"User {idUsuario} not found in database");
                    return NotFound("Usuario no encontrado");
                }

                _logger.LogInformation("Current-user verified for id {UserId}.", user.IdUsuario);

                return Ok(new
                {
                    idUsuario = user.IdUsuario,
                    nombreCompleto = user.NombreCompleto,
                    correo = user.Correo,
                    rol = user.Rol
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetCurrentUser: {ex.Message}");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
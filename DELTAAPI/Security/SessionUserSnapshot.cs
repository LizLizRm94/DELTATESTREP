namespace DELTAAPI.Security;

// Datos mínimos leídos al validar la cookie; reutilizados solo durante esa petición.
public sealed class SessionUserSnapshot
{
    public int IdUsuario { get; set; }
    public string NombreCompleto { get; set; } = "";
    public string? Correo { get; set; }
    public string? Estado { get; set; }
    public string Rol { get; set; } = "";
}

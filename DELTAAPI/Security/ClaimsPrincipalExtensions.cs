using System.Security.Claims;

namespace DELTAAPI.Security;

public static class ClaimsPrincipalExtensions
{
    public static int? GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }

    public static bool IsAdministrador(this ClaimsPrincipal user)
        => user.IsInRole("Administrador");

    public static bool IsInspector(this ClaimsPrincipal user)
        => user.IsInRole("Inspector");

    public static bool IsStaff(this ClaimsPrincipal user)
        => user.IsAdministrador() || user.IsInspector();

    /// <summary>
    /// Devuelve true si el usuario autenticado es el dueño del recurso o
    /// pertenece a un rol administrativo.
    /// </summary>
    public static bool CanAccessUser(this ClaimsPrincipal user, int idUsuario)
        => user.IsStaff() || user.GetUserId() == idUsuario;
}

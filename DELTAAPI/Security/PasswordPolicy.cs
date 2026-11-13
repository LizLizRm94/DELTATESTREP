namespace DELTAAPI.Security;

/// <summary>
/// Política mínima de contraseñas para cuentas nuevas.
/// No se aplica al inicio de sesión de cuentas existentes.
/// </summary>
public static class PasswordPolicy
{
    public const int MinimumLength = 10;

    public static string? Validate(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return "La contraseña es obligatoria.";

        if (password.Length < MinimumLength)
            return $"La contraseña debe tener al menos {MinimumLength} caracteres.";

        if (password.Length > 128)
            return "La contraseña no puede superar los 128 caracteres.";

        if (password.Any(char.IsWhiteSpace))
            return "La contraseña no puede contener espacios.";

        if (!password.Any(char.IsUpper))
            return "La contraseña debe incluir al menos una letra mayúscula.";

        if (!password.Any(char.IsLower))
            return "La contraseña debe incluir al menos una letra minúscula.";

        if (!password.Any(char.IsDigit))
            return "La contraseña debe incluir al menos un número.";

        if (!password.Any(c => !char.IsLetterOrDigit(c)))
            return "La contraseña debe incluir al menos un símbolo.";

        return null;
    }
}

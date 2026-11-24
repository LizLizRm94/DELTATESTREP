namespace Delta.Contracts;

public static class VinculacionLaboral
{
    public static string? Validar(string? tipo, string? actual, string? rotar, string? solicitado)
    {
        if (tipo is not ("Empleado" or "Pasante")) return "Seleccione Empleado o Pasante.";
        if (new[] {actual,rotar,solicitado}.Any(p => p?.Length > 100)) return "Los cargos admiten un máximo de 100 caracteres.";
        if (tipo == "Empleado")
        {
            if (string.IsNullOrWhiteSpace(actual) || string.IsNullOrWhiteSpace(rotar)) return "Seleccione el puesto actual y el puesto a rotar.";
            if (!string.IsNullOrWhiteSpace(solicitado)) return "Un empleado no debe tener puesto solicitado.";
        }
        else
        {
            if (string.IsNullOrWhiteSpace(solicitado)) return "Seleccione el puesto solicitado por el pasante.";
            if (!string.IsNullOrWhiteSpace(actual) || !string.IsNullOrWhiteSpace(rotar)) return "Un pasante no debe tener puesto actual ni puesto a rotar.";
        }
        return null;
    }
}

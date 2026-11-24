namespace Delta.Contracts;

/// <summary>Cargos proporcionados por el usuario para los formularios de personal.</summary>
public static class CatalogoCargos
{
    public static readonly IReadOnlyList<string> Predeterminados = Array.AsReadOnly(new[]
    {
        "Jefe de control de calidad",
        "Encargado de documentación",
        "Encargado de estudios de estabilidad",
        "Encargado de validaciones de metodologías analíticas",
        "Analista de control de calidad",
        "Encargado de recursos de laboratorio",
        "Auxiliar de laboratorio",
        "Inspector"
    });
}

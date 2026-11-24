namespace Delta.Contracts;

/// <summary>Áreas provisionales; la sección Cargos permite sustituirlas por el catálogo de la empresa.</summary>
public static class AreasFarmaceuticas
{
    public static readonly IReadOnlyList<string> Provisionales = Array.AsReadOnly(new[]
    {
        "Producción farmacéutica",
        "Aseguramiento de la calidad",
        "Control de calidad fisicoquímico",
        "Microbiología y control ambiental",
        "Envasado y acondicionamiento",
        "Almacén y logística",
        "Ingeniería y mantenimiento",
        "Calificación y validación",
        "Asuntos regulatorios",
        "Farmacovigilancia"
    });
}

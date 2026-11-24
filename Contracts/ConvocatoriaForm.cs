using System.ComponentModel.DataAnnotations;

namespace Delta.Contracts;

public class ConvocatoriaForm : IValidatableObject
{
    public bool Publicada { get; set; } = true;
    [Required(ErrorMessage = "Ingrese el código."), StringLength(60, ErrorMessage = "Máximo 60 caracteres.")]
    public string Codigo { get; set; } = "";
    [Required(ErrorMessage = "Ingrese el título."), StringLength(255, ErrorMessage = "Máximo 255 caracteres.")]
    public string NombreConvocatoria { get; set; } = "";
    [Required(ErrorMessage = "Complete este campo obligatorio."), StringLength(200, ErrorMessage = "Máximo 200 caracteres.")]
    public string EntidadEmisora { get; set; } = "Laboratorios Delta S.A.";
    [Required(ErrorMessage = "Complete este campo obligatorio."), StringLength(500, ErrorMessage = "Máximo 500 caracteres.")]
    public string Resumen { get; set; } = "";
    [Required(ErrorMessage = "Complete este campo obligatorio."), StringLength(10000, ErrorMessage = "Máximo 10000 caracteres.")]
    public string Descripcion { get; set; } = "";
    [Required(ErrorMessage = "Complete este campo obligatorio."), StringLength(10000, ErrorMessage = "Máximo 10000 caracteres.")]
    public string Objetivo { get; set; } = "";
    [Required(ErrorMessage = "Complete este campo obligatorio."), StringLength(10000, ErrorMessage = "Máximo 10000 caracteres.")]
    public string Participantes { get; set; } = "";
    [Required(ErrorMessage = "Complete este campo obligatorio."), StringLength(10000, ErrorMessage = "Máximo 10000 caracteres.")]
    public string Requisitos { get; set; } = "";
    [Required(ErrorMessage = "Complete este campo obligatorio."), StringLength(10000, ErrorMessage = "Máximo 10000 caracteres.")]
    public string Modalidad { get; set; } = "";
    [Required(ErrorMessage = "Complete este campo obligatorio."), StringLength(10000, ErrorMessage = "Máximo 10000 caracteres.")]
    public string Cronograma { get; set; } = "";
    [Required(ErrorMessage = "Complete este campo obligatorio."), StringLength(10000, ErrorMessage = "Máximo 10000 caracteres.")]
    public string Instrucciones { get; set; } = "";
    [Required(ErrorMessage = "Complete este campo obligatorio."), StringLength(500, ErrorMessage = "Máximo 500 caracteres.")]
    public string Lugar { get; set; } = "";
    [Required(ErrorMessage = "Complete este campo obligatorio."), StringLength(500, ErrorMessage = "Máximo 500 caracteres.")]
    public string Contacto { get; set; } = "";
    [Required(ErrorMessage = "Complete este campo obligatorio."), RegularExpression("^(Activa|Cerrada)$", ErrorMessage = "Seleccione Activa o Cerrada.")]
    public string EstadoConvocatoria { get; set; } = "Activa";
    [Required(ErrorMessage = "Ingrese la fecha de publicación.")]
    public DateOnly? FechaConvocatoria { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [Required(ErrorMessage = "Ingrese la fecha límite.")]
    public DateOnly? FechaCierre { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FechaConvocatoria == DateOnly.MinValue || FechaCierre == DateOnly.MinValue)
            yield return new ValidationResult("Ingrese fechas válidas.", new[] { nameof(FechaConvocatoria), nameof(FechaCierre) });
        if (FechaCierre < FechaConvocatoria)
            yield return new ValidationResult("La fecha límite debe ser igual o posterior a la publicación.", new[] { nameof(FechaCierre) });
    }
}

public sealed class ConvocatoriaResumen
{
    public int IdConvocatoria { get; set; }
    public string Codigo { get; set; } = "";
    public string NombreConvocatoria { get; set; } = "";
    public string Resumen { get; set; } = "";
    public string EstadoConvocatoria { get; set; } = "";
    public DateOnly? FechaConvocatoria { get; set; }
    public DateOnly? FechaCierre { get; set; }
}

public sealed class ConvocatoriaGestion
{
    public int IdConvocatoria { get; set; }
    public string Codigo { get; set; } = "";
    public string NombreConvocatoria { get; set; } = "";
    public string EstadoConvocatoria { get; set; } = "";
    public DateOnly? FechaConvocatoria { get; set; }
    public DateOnly? FechaCierre { get; set; }
    public bool PdfPublicoDisponible { get; set; }
    public string Publicacion { get; set; } = "";
}

using System.ComponentModel.DataAnnotations;
namespace DELTATEST.Models;
public class EditUsuarioModel : IValidatableObject
{
    public int IdUsuario { get; set; }
    [Required(ErrorMessage="Ingrese el nombre completo."), StringLength(255)]
    public string? NombreCompleto { get; set; }
    [Required(ErrorMessage="Ingrese el correo."), EmailAddress(ErrorMessage="Ingrese un correo válido."), StringLength(255)]
    public string? Correo { get; set; }
    [StringLength(50)] public string? Ci { get; set; }
    [StringLength(50)] public string? Telefono { get; set; }
    public DateTime? FechaIngreso { get; set; }
    [StringLength(100)] public string? PuestoActual { get; set; }
    [StringLength(100)] public string? PuestoSolicitado { get; set; }
    [StringLength(100)] public string? PuestoARotar { get; set; }
    public string? Estado { get; set; }
    public bool Activo
    {
        get => string.Equals(Estado?.Trim(), "Activo", StringComparison.OrdinalIgnoreCase);
        set => Estado = value ? "Activo" : "Inactivo";
    }
    [Required(ErrorMessage="Seleccione un rol."), RegularExpression("^(Usuario|Inspector|Administrador)$", ErrorMessage="Seleccione un rol válido.")]
    public string? Rol { get; set; }
    public string? TipoEvaluado { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var error=Delta.Contracts.VinculacionLaboral.Validar(TipoEvaluado,PuestoActual,PuestoARotar,PuestoSolicitado);
        if(error!=null) yield return new ValidationResult(error);
    }
}

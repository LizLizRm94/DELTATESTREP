using System.ComponentModel.DataAnnotations;

namespace DELTATEST.Models
{
    public class RegisterModel : IValidatableObject
    {
        [Required(ErrorMessage = "Nombre completo es obligatorio")]
        public string NombreCompleto { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "CI demasiado largo")]
        public string? Ci { get; set; }

        [StringLength(2, ErrorMessage = "Extensión no válida")]
        public string? Expedicion { get; set; }

        [Required(ErrorMessage = "Correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo no válido")]
        public string Correo { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Telefono { get; set; }

        [Required(ErrorMessage="Seleccione un rol."), RegularExpression("^(Usuario|Inspector|Administrador)$", ErrorMessage="Rol no válido.")]
        public string? Rol { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FechaIngreso { get; set; }

        public string? Estado { get; set; }
        public string? PuestoActual { get; set; }
        public string? PuestoSolicitado { get; set; }
        public string? PuestoARotar { get; set; }

        [Required(ErrorMessage = "Contraseña es obligatoria")]
        [StringLength(128, MinimumLength = 10, ErrorMessage = "La contraseña debe tener entre 10 y 128 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9])\S+$",
            ErrorMessage = "La contraseña debe incluir mayúscula, minúscula, número y símbolo, sin espacios")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }
        public string? TipoEvaluado { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var error=Delta.Contracts.VinculacionLaboral.Validar(TipoEvaluado,PuestoActual,PuestoARotar,PuestoSolicitado);
            if(error!=null) yield return new ValidationResult(error);
        }
    }
}

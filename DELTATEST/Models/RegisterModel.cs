using System.ComponentModel.DataAnnotations;

namespace DELTATEST.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Nombre completo es obligatorio")]
        public string NombreCompleto { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "CI demasiado largo")]
        public string? Ci { get; set; }

        [Required(ErrorMessage = "Correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo no válido")]
        public string Correo { get; set; } = string.Empty;

        public string? Telefono { get; set; }

        public string? Rol { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FechaIngreso { get; set; }

        public string? Estado { get; set; }
        public string? PuestoActual { get; set; }
        public string? PuestoSolicitado { get; set; }
        public string? PuestoARotar { get; set; }

        [Required(ErrorMessage = "Contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }
    }
}

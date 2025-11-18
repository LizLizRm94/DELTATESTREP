using System.ComponentModel.DataAnnotations;

namespace DELTATEST.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Usuario (CI o correo) es obligatorio")]
        public string Username { get; set; } = string.Empty; // CI o correo

        [Required(ErrorMessage = "Contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Contrasena { get; set; } = string.Empty;
    }
}
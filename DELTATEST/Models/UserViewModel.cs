namespace DELTATEST.Models
{
    public class UserViewModel
    {
        public int IdUsuario { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Correo { get; set; }
        public DateOnly? FechaIngreso { get; set; }
        public string? PuestoActual { get; set; }
        public string? PuestoSolicitado { get; set; }
        public string? Estado { get; set; } // uso para "Ingreso con"
    }
}
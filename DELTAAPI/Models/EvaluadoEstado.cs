using System.Collections.Generic;

namespace DELTAAPI.Models
{
    public class EvaluadoEstado
    {
        public int IdUsuario { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public decimal PromedioGeneral { get; set; }
        public string ProximaRotacion { get; set; } = string.Empty;

        public List<Evaluacion> Evaluaciones { get; set; } = new();
        public List<Notificacion> Notificaciones { get; set; } = new();
    }
}

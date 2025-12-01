using System;
using System.Collections.Generic;

namespace DELTATEST.Models
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

    public class Evaluacion
    {
        public int IdEvaluacion { get; set; }
        public string NombreEvaluacion { get; set; } = string.Empty;
        public DateTime? FechaEvaluacion { get; set; }
        public decimal? Nota { get; set; }

        // If you need EstadoEvaluacion from the API, add it here as well
        // public string EstadoEvaluacion { get; set; } = string.Empty;
    }

    public class Notificacion
    {
        public string TipoNotificacion { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public DateTime? FechaEnvio { get; set; }
    }
}

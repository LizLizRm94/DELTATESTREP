using System;
using System.Collections.Generic;

namespace DELTAAPI.Models;

public partial class Evaluacion
{
    public int IdEvaluacion { get; set; }

    public int IdEvaluado { get; set; }

    public int? IdAdministrador { get; set; }

    public int? IdArea { get; set; }

    public DateOnly? FechaEvaluacion { get; set; }

    public decimal? Nota { get; set; }

    public string? EstadoEvaluacion { get; set; }

    public bool? TipoEvaluacion { get; set; }

    public string? Recomendaciones { get; set; }

    public virtual Usuario? IdAdministradorNavigation { get; set; }

    public virtual Area? IdAreaNavigation { get; set; }

    public virtual Usuario IdEvaluadoNavigation { get; set; } = null!;

    public virtual ICollection<Respuesta> Respuestas { get; set; } = new List<Respuesta>();

    public virtual ICollection<Pregunta> Preguntas { get; set; } = new List<Pregunta>();

    public virtual ICollection<Notificacion> Notificacions { get; set; } = new List<Notificacion>();
}

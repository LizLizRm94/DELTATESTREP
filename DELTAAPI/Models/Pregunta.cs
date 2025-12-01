using System;
using System.Collections.Generic;

namespace DELTAAPI.Models;

public partial class Pregunta
{
    public int IdPregunta { get; set; }
    public string Texto { get; set; } = string.Empty;
    public bool TipoEvaluacion { get; set; }

    public virtual ICollection<Respuesta> Respuestas { get; set; } = new List<Respuesta>();
}

using System;
using System.Collections.Generic;

namespace DELTATEST.Models;

public partial class Pregunta
{
    public int IdPregunta { get; set; }
    public string Texto { get; set; } = string.Empty;
    public bool TipoEvaluacion { get; set; }
    public int? IdEvaluacion { get; set; }
}

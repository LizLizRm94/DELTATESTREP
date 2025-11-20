using System;
using System.Collections.Generic;

namespace DELTAAPI.Models;

public partial class Pregunta
{
    public int IdPregunta { get; set; }

    public string PreguntaTexto { get; set; } = null!;

    public bool TipoEvaluacion { get; set; }
}

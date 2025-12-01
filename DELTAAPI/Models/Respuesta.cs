using System;
using System.Collections.Generic;

namespace DELTAAPI.Models;

public partial class Respuesta
{
    public int IdRespuesta { get; set; }

    public int IdUsuario { get; set; }

  public int IdPregunta { get; set; }

    public int IdEvaluacion { get; set; }

    public string TextoRespuesta { get; set; } = null!;

    public virtual Evaluacion IdEvaluacionNavigation { get; set; } = null!;

    public virtual Pregunta IdPreguntaNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}

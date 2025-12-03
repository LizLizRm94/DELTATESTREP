using System;
using System.Collections.Generic;

namespace DELTAAPI.Models;

public partial class Pregunta
{
    public int IdPregunta { get; set; }
    public string Texto { get; set; } = string.Empty;
    public bool TipoEvaluacion { get; set; }
    public int? IdEvaluacion { get; set; }
    public string? Opciones { get; set; } // JSON con opciones
    public int? RespuestaCorrectaIndex { get; set; } // Índice de la opción correcta
    public int Puntos { get; set; } = 0; // Puntos que vale esta pregunta

    public virtual Evaluacion? IdEvaluacionNavigation { get; set; }
    public virtual ICollection<Respuesta> Respuestas { get; set; } = new List<Respuesta>();
}

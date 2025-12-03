namespace DELTATEST.Models;

public class EvaluacionTeoricaModelo
{
    public List<Pregunta> PreguntasAbiertas { get; set; } = new();
    public List<Pregunta> PreguntasMultiples { get; set; } = new();
}

public partial class Pregunta
{
    public int IdPregunta { get; set; }
    public string Texto { get; set; } = string.Empty;
    public bool TipoEvaluacion { get; set; }
    public int? IdEvaluacion { get; set; }
    public string? Opciones { get; set; }
    public int? RespuestaCorrectaIndex { get; set; }
    public int Puntos { get; set; } = 0;
    public List<Opcion> OpcionesLista { get; set; } = new();
}

public class Opcion
{
    public string Texto { get; set; } = string.Empty;
    public bool EsCorrecta { get; set; } = false;
}
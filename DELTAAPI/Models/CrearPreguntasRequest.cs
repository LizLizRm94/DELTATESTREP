namespace DELTAAPI.Models;

public class CrearPreguntasRequest
{
    public int? IdEvaluado { get; set; }
    public List<PreguntaDto> Preguntas { get; set; } = new();
}

public class PreguntaDto
{
    public string Texto { get; set; } = string.Empty;
    public bool TipoEvaluacion { get; set; }
}

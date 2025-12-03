namespace DELTAAPI.Models;

public class GuardarRespuestasRequest
{
    public int IdEvaluacion { get; set; }
    public List<RespuestaItemRequest> Respuestas { get; set; } = new();
}

public class RespuestaItemRequest
{
    public int IdPregunta { get; set; }
    public string TextoRespuesta { get; set; } = string.Empty;
}

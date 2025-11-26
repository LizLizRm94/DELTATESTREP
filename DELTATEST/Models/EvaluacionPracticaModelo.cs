namespace DELTATEST.Models;

public class EvaluacionPracticaModelo
{
  public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public List<TareaPractica> Tareas { get; set; } = new();
    public string? ResultadoEvaluacion { get; set; }
    public int? Puntuacion { get; set; }
}

public class TareaPractica
{
    public int IdTarea { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? Instrucciones { get; set; }
    public string? ResultadoObtenido { get; set; }
    public bool Completada { get; set; }
    public int? Calificacion { get; set; }
}

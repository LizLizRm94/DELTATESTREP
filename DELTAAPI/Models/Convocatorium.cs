using System;
using System.Collections.Generic;

namespace DELTAAPI.Models;

public partial class Convocatorium
{
    public int IdConvocatoria { get; set; }

    public DateOnly? FechaConvocatoria { get; set; }

    public string NombreConvocatoria { get; set; } = null!;

    public string? EstadoConvocatoria { get; set; }

    public string Codigo { get; set; } = "";
    public string EntidadEmisora { get; set; } = "";
    public string Resumen { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string Objetivo { get; set; } = "";
    public string Participantes { get; set; } = "";
    public string Requisitos { get; set; } = "";
    public string Modalidad { get; set; } = "";
    public string Cronograma { get; set; } = "";
    public string Instrucciones { get; set; } = "";
    public string Lugar { get; set; } = "";
    public string Contacto { get; set; } = "";
    public DateOnly? FechaCierre { get; set; }
    public DateTime CreadaUtc { get; set; }
    public int? CreadaPorUsuarioId { get; set; }
    public bool Publicada { get; set; }

    public virtual ICollection<Usuario> IdUsuarios { get; set; } = new List<Usuario>();
}

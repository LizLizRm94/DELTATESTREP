using System;
using System.Collections.Generic;

namespace DELTAAPI.Models;

public partial class Convocatorium
{
    public int IdConvocatoria { get; set; }

    public DateOnly? FechaConvocatoria { get; set; }

    public string NombreConvocatoria { get; set; } = null!;

    public string? EstadoConvocatoria { get; set; }

    public virtual ICollection<Usuario> IdUsuarios { get; set; } = new List<Usuario>();
}

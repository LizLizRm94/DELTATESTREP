using System;
using System.Collections.Generic;

namespace DELTAAPI.Models;

public partial class Area
{
    public int IdArea { get; set; }

    public string NombreArea { get; set; } = null!;

    public virtual ICollection<Evaluacion> Evaluacions { get; set; } = new List<Evaluacion>();
}

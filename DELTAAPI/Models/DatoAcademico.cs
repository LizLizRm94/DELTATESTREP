using System;
using System.Collections.Generic;

namespace DELTAAPI.Models;

public partial class DatoAcademico
{
    public int IdDatoAcademico { get; set; }

    public int IdUsuario { get; set; }

    public string? TituloAcademico { get; set; }

    public string? LugarEstudio { get; set; }

    public string? Carrera { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}

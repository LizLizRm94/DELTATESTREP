using System;
using System.Collections.Generic;

namespace DELTAAPI.Models;

public partial class Notificacion
{
    public int IdNotificacion { get; set; }

    public int? IdEvaluacion { get; set; }

    public int? IdAdministrador { get; set; }

    public int IdUsuarioDestino { get; set; }

    public string? Mensaje { get; set; }

    public DateTime? FechaEnvio { get; set; }

    public string? TipoNotificacion { get; set; }

    public virtual Usuario? IdAdministradorNavigation { get; set; }

    public virtual Evaluacion? IdEvaluacionNavigation { get; set; }

    public virtual Usuario IdUsuarioDestinoNavigation { get; set; } = null!;
}

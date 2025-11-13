using System;
using System.Collections.Generic;

namespace DELTAAPI.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string NombreCompleto { get; set; } = null!;

    public string Contraseña { get; set; } = null!;

    public string Ci { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Correo { get; set; }

    public string? Expedicion { get; set; }

    public string Rol { get; set; } = null!;

    public string? TipoEvaluado { get; set; }

    public DateOnly? FechaIngreso { get; set; }

    public string? Observaciones { get; set; }

    public string? Estado { get; set; }

    public string? PuestoActual { get; set; }

    public string? PuestoSolicitado { get; set; }

    public string? PuestoARotar { get; set; }

    public int? IdSupervisor { get; set; }

    public int? IdCreadoPor { get; set; }

    public virtual ICollection<DatoAcademico> DatoAcademicos { get; set; } = new List<DatoAcademico>();

    public virtual ICollection<Evaluacion> EvaluacionIdAdministradorNavigations { get; set; } = new List<Evaluacion>();

    public virtual ICollection<Evaluacion> EvaluacionIdEvaluadoNavigations { get; set; } = new List<Evaluacion>();

    public virtual Usuario? IdCreadoPorNavigation { get; set; }

    public virtual Usuario? IdSupervisorNavigation { get; set; }

    public virtual ICollection<Usuario> InverseIdCreadoPorNavigation { get; set; } = new List<Usuario>();

    public virtual ICollection<Usuario> InverseIdSupervisorNavigation { get; set; } = new List<Usuario>();

    public virtual ICollection<Notificacion> NotificacionIdAdministradorNavigations { get; set; } = new List<Notificacion>();

    public virtual ICollection<Notificacion> NotificacionIdUsuarioDestinoNavigations { get; set; } = new List<Notificacion>();

    public virtual ICollection<Convocatorium> IdConvocatoria { get; set; } = new List<Convocatorium>();
}

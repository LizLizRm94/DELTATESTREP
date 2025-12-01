using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DELTAAPI.Models;

public partial class DeltaTestContext : DbContext
{
    // Hago el ctor sin parámetros protected para evitar instancias manuales desde fuera del proyecto.
    protected DeltaTestContext()
    {
    }

    public DeltaTestContext(DbContextOptions<DeltaTestContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Area> Areas { get; set; }

    public virtual DbSet<Convocatorium> Convocatoria { get; set; }

    public virtual DbSet<DatoAcademico> DatoAcademicos { get; set; }

    public virtual DbSet<Evaluacion> Evaluacions { get; set; }

    public virtual DbSet<Notificacion> Notificacions { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Pregunta> Preguntas { get; set; }

    public virtual DbSet<Respuesta> Respuestas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Sólo usar la cadena hardcodeada como fallback (p. ej. diseño/ef tools).
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Name=DefaultConnection");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Area>(entity =>
        {
            entity.HasKey(e => e.IdArea).HasName("PK__AREA__8A8C837B5453672E");

            entity.ToTable("AREA");

            entity.HasIndex(e => e.NombreArea, "UQ__AREA__1346D27AC7230296").IsUnique();

            entity.Property(e => e.IdArea).HasColumnName("id_area");
            entity.Property(e => e.NombreArea)
                .HasMaxLength(100)
                .HasColumnName("nombre_area");
        });

        modelBuilder.Entity<Convocatorium>(entity =>
        {
            entity.HasKey(e => e.IdConvocatoria).HasName("PK__CONVOCAT__2EAE64DB1B80B372");

            entity.ToTable("CONVOCATORIA");

            entity.Property(e => e.IdConvocatoria).HasColumnName("id_convocatoria");
            entity.Property(e => e.EstadoConvocatoria)
                .HasMaxLength(50)
                .HasColumnName("estado_convocatoria");
            entity.Property(e => e.FechaConvocatoria).HasColumnName("fecha_convocatoria");
            entity.Property(e => e.NombreConvocatoria)
                .HasMaxLength(255)
                .HasColumnName("nombre_convocatoria");
        });

        modelBuilder.Entity<DatoAcademico>(entity =>
        {
            entity.HasKey(e => e.IdDatoAcademico).HasName("PK__DATO_ACA__AB51D4842666D8A3");

            entity.ToTable("DATO_ACADEMICO");

            entity.Property(e => e.IdDatoAcademico).HasColumnName("id_dato_academico");
            entity.Property(e => e.Carrera)
                .HasMaxLength(255)
                .HasColumnName("carrera");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.LugarEstudio)
                .HasMaxLength(255)
                .HasColumnName("lugar_estudio");
            entity.Property(e => e.TituloAcademico)
                .HasMaxLength(255)
                .HasColumnName("titulo_academico");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.DatoAcademicos)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DATO_ACAD__id_us__3D5E1FD2");
        });

        modelBuilder.Entity<Evaluacion>(entity =>
        {
            entity.HasKey(e => e.IdEvaluacion).HasName("PK__EVALUACI__65DE60C5F6D38ADE");

            entity.ToTable("EVALUACION");

            entity.Property(e => e.IdEvaluacion).HasColumnName("id_evaluacion");
            entity.Property(e => e.EstadoEvaluacion)
                .HasMaxLength(50)
                .HasColumnName("estado_evaluacion");
            entity.Property(e => e.FechaEvaluacion).HasColumnName("fecha_evaluacion");
            entity.Property(e => e.IdAdministrador).HasColumnName("id_administrador");
            entity.Property(e => e.IdArea).HasColumnName("id_area");
            entity.Property(e => e.IdEvaluado).HasColumnName("id_evaluado");
            entity.Property(e => e.Nota)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("nota");
            entity.Property(e => e.TipoEvaluacion).HasColumnName("tipo_evaluacion");

            entity.HasOne(d => d.IdAdministradorNavigation).WithMany(p => p.EvaluacionIdAdministradorNavigations)
                .HasForeignKey(d => d.IdAdministrador)
                .HasConstraintName("FK__EVALUACIO__id_ad__49C3F6B7");

            entity.HasOne(d => d.IdAreaNavigation).WithMany(p => p.Evaluacions)
                .HasForeignKey(d => d.IdArea)
                .HasConstraintName("FK__EVALUACIO__id_ar__4AB81AF0");

            entity.HasOne(d => d.IdEvaluadoNavigation).WithMany(p => p.EvaluacionIdEvaluadoNavigations)
                .HasForeignKey(d => d.IdEvaluado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EVALUACIO__id_ev__48CFD27E");
        });

        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.HasKey(e => e.IdNotificacion).HasName("PK__NOTIFICA__8270F9A58C1F6715");

            entity.ToTable("NOTIFICACION");

            entity.Property(e => e.IdNotificacion).HasColumnName("id_notificacion");
            entity.Property(e => e.FechaEnvio)
                .HasColumnType("datetime")
                .HasColumnName("fecha_envio");
            entity.Property(e => e.IdAdministrador).HasColumnName("id_administrador");
            entity.Property(e => e.IdEvaluacion).HasColumnName("id_evaluacion");
            entity.Property(e => e.IdUsuarioDestino).HasColumnName("id_usuario_destino");
            entity.Property(e => e.Mensaje).HasColumnName("mensaje");
            entity.Property(e => e.TipoNotificacion)
                .HasMaxLength(100)
                .HasColumnName("tipo_notificacion");

            entity.HasOne(d => d.IdAdministradorNavigation).WithMany(p => p.NotificacionIdAdministradorNavigations)
                .HasForeignKey(d => d.IdAdministrador)
                .HasConstraintName("FK__NOTIFICAC__id_ad__4E88ABD4");

            entity.HasOne(d => d.IdEvaluacionNavigation).WithMany(p => p.Notificacions)
                .HasForeignKey(d => d.IdEvaluacion)
                .HasConstraintName("FK__NOTIFICAC__id_ev__4D94879B");

            entity.HasOne(d => d.IdUsuarioDestinoNavigation).WithMany(p => p.NotificacionIdUsuarioDestinoNavigations)
                .HasForeignKey(d => d.IdUsuarioDestino)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NOTIFICAC__id_us__4F7CD00D");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__USUARIO__4E3E04AD77A0E458");

            entity.ToTable("USUARIO");

            entity.HasIndex(e => e.Correo, "UQ__USUARIO__2A586E0B2E389AD7").IsUnique();

            entity.HasIndex(e => e.Ci, "UQ__USUARIO__32136662237C1C6E").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Ci)
                .HasMaxLength(50)
                .HasColumnName("ci");
            entity.Property(e => e.Contraseña)
                .HasMaxLength(255)
                .HasColumnName("contraseña");
            entity.Property(e => e.Correo)
                .HasMaxLength(255)
                .HasColumnName("correo");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .HasColumnName("estado");
            entity.Property(e => e.Expedicion)
                .HasMaxLength(100)
                .HasColumnName("expedicion");
            entity.Property(e => e.FechaIngreso).HasColumnName("fecha_ingreso");
            entity.Property(e => e.IdCreadoPor).HasColumnName("id_creado_por");
            entity.Property(e => e.IdSupervisor).HasColumnName("id_supervisor");
            entity.Property(e => e.NombreCompleto)
                .HasMaxLength(255)
                .HasColumnName("nombre_completo");
            entity.Property(e => e.Observaciones).HasColumnName("observaciones");
            entity.Property(e => e.PuestoARotar)
                .HasMaxLength(100)
                .HasColumnName("puesto_a_rotar");
            entity.Property(e => e.PuestoActual)
                .HasMaxLength(100)
                .HasColumnName("puesto_actual");
            entity.Property(e => e.PuestoSolicitado)
                .HasMaxLength(100)
                .HasColumnName("puesto_solicitado");
            entity.Property(e => e.Rol)
                .HasMaxLength(50)
                .HasColumnName("rol");
            entity.Property(e => e.Telefono)
                .HasMaxLength(50)
                .HasColumnName("telefono");
            entity.Property(e => e.TipoEvaluado)
                .HasMaxLength(50)
                .HasColumnName("tipo_evaluado");

            entity.HasOne(d => d.IdCreadoPorNavigation).WithMany(p => p.InverseIdCreadoPorNavigation)
                .HasForeignKey(d => d.IdCreadoPor)
                .HasConstraintName("FK__USUARIO__id_crea__3A81B327");

            entity.HasOne(d => d.IdSupervisorNavigation).WithMany(p => p.InverseIdSupervisorNavigation)
                .HasForeignKey(d => d.IdSupervisor)
                .HasConstraintName("FK__USUARIO__id_supe__398D8EEE");

            entity.HasMany(d => d.IdConvocatoria).WithMany(p => p.IdUsuarios)
                .UsingEntity<Dictionary<string, object>>(
                    "EvaluadoConvocatorium",
                    r => r.HasOne<Convocatorium>().WithMany()
                        .HasForeignKey("IdConvocatoria")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__EVALUADO___id_co__45F365D3"),
                    l => l.HasOne<Usuario>().WithMany()
                        .HasForeignKey("IdUsuario")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__EVALUADO___id_us__44FF419A"),
                    j =>
                    {
                        j.HasKey("IdUsuario", "IdConvocatoria").HasName("PK__EVALUADO__ECD4E2E08E56B8A5");
                        j.ToTable("EVALUADO_CONVOCATORIA");
                        j.IndexerProperty<int>("IdUsuario").HasColumnName("id_usuario");
                        j.IndexerProperty<int>("IdConvocatoria").HasColumnName("id_convocatoria");
                    });
        });

        modelBuilder.Entity<Pregunta>(entity =>
        {
            entity.HasKey(e => e.IdPregunta).HasName("PK__PREGUNTA__3C69FB99");

            entity.ToTable("PREGUNTA");

            entity.Property(e => e.IdPregunta).HasColumnName("id_pregunta");
            entity.Property(e => e.Texto)
                .HasMaxLength(500)
                .HasColumnName("texto");
            entity.Property(e => e.TipoEvaluacion).HasColumnName("tipo_evaluacion");
        });

        modelBuilder.Entity<Respuesta>(entity =>
        {
            entity.HasKey(e => e.IdRespuesta).HasName("PK__RESPUEST__1AAA640C");

            entity.ToTable("RESPUESTA");

            entity.Property(e => e.IdRespuesta).HasColumnName("id_respuesta");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.IdPregunta).HasColumnName("id_pregunta");
            entity.Property(e => e.IdEvaluacion).HasColumnName("id_evaluacion");
            entity.Property(e => e.TextoRespuesta)
                .HasMaxLength(255)
                .IsRequired()
                .HasColumnName("texto_respuesta");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Respuestas)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Respuesta_Usuario");

            entity.HasOne(d => d.IdPreguntaNavigation).WithMany(p => p.Respuestas)
                .HasForeignKey(d => d.IdPregunta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Respuesta_Pregunta");

            entity.HasOne(d => d.IdEvaluacionNavigation).WithMany(p => p.Respuestas)
                .HasForeignKey(d => d.IdEvaluacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Respuesta_Evaluacion");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
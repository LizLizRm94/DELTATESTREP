using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DELTAAPI.Models;

public partial class DeltaTestContext : DbContext
{
    public DeltaTestContext()
    {
    }

    public DeltaTestContext(DbContextOptions<DeltaTestContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Pregunta> Pregunta { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-V5J0U51\\DELTATEST;Database=DeltaTest;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pregunta>(entity =>
        {
            entity.HasKey(e => e.IdPregunta).HasName("PK__PREGUNTA__6867FFA4BF35AC9F");

            entity.ToTable("PREGUNTA");

            entity.Property(e => e.IdPregunta).HasColumnName("id_pregunta");
            entity.Property(e => e.PreguntaTexto)
                .HasMaxLength(255)
                .HasColumnName("texto");
            entity.Property(e => e.TipoEvaluacion).HasColumnName("tipo_evaluacion");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

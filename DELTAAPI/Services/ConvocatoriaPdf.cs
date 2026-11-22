using DELTAAPI.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DELTAAPI.Services;

public static class ConvocatoriaPdf
{
    public static byte[] Generar(Convocatorium convocatoria, string estado) => Document.Create(document =>
    {
        document.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(45);
            page.DefaultTextStyle(t => t.FontSize(11).FontColor(Colors.Grey.Darken3));
            page.Header().PaddingBottom(18).Column(column =>
            {
                column.Item().Text(convocatoria.EntidadEmisora).FontSize(17).Bold().FontColor(Colors.Orange.Darken3);
                column.Item().Text("CONVOCATORIA FORMAL A LABORATORIOS").FontSize(10);
                column.Item().Text(convocatoria.Codigo).Bold();
            });
            page.Content().Column(column =>
            {
                column.Spacing(9);
                column.Item().Text(convocatoria.NombreConvocatoria).FontSize(19).Bold();
                column.Item().Text($"Publicación: {convocatoria.FechaConvocatoria:dd/MM/yyyy}   |   Fecha límite: {convocatoria.FechaCierre:dd/MM/yyyy}");
                column.Item().Text($"Estado: {estado} (fecha de referencia: {DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "America/La_Paz")):dd/MM/yyyy})");
                Seccion("Resumen", convocatoria.Resumen);
                Seccion("Descripción de la convocatoria", convocatoria.Descripcion);
                Seccion("Objetivo", convocatoria.Objetivo);
                Seccion("Participantes convocados", convocatoria.Participantes);
                Seccion("Requisitos y criterios", convocatoria.Requisitos);
                Seccion("Modalidad de evaluación", convocatoria.Modalidad);
                Seccion("Cronograma y horarios", convocatoria.Cronograma);
                Seccion("Instrucciones y obligatoriedad", convocatoria.Instrucciones);
                Seccion("Lugar / plataforma", convocatoria.Lugar);
                Seccion("Contacto", convocatoria.Contacto);
                void Seccion(string titulo, string contenido)
                {
                    column.Item().EnsureSpace(60).PaddingTop(6).Text(titulo).Bold().FontSize(12);
                    column.Item().Text(contenido);
                }
            });
            page.Footer().AlignCenter().Text(text =>
            {
                text.Span("Convocatoria | Página ");
                text.CurrentPageNumber();
                text.Span(" de ");
                text.TotalPages();
            });
        });
    }).GeneratePdf();
}

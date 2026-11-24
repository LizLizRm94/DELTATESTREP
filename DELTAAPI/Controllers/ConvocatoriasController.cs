using System.Security.Claims;
using Delta.Contracts;
using DELTAAPI.Models;
using DELTAAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DELTAAPI.Controllers;

[ApiController]
[Route("api/convocatorias")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class ConvocatoriasController(DeltaTestContext db) : ControllerBase
{
    // La fecha límite incluye todo el día civil de Bolivia.
    private static DateOnly Hoy => DateOnly.FromDateTime(
        TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "America/La_Paz"));
    private IQueryable<Convocatorium> Publicas(DateOnly hoy) => db.Convocatoria.AsNoTracking()
        .Where(c => c.Publicada && c.FechaConvocatoria <= hoy);
    private static string Estado(Convocatorium c, DateOnly hoy) =>
        c.EstadoConvocatoria == "Cerrada" || c.FechaCierre < hoy ? "Cerrada" : "Activa";

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<ConvocatoriaResumen>>> Ultimas(CancellationToken ct)
    {
        var hoy = Hoy;
        var filas = await Publicas(hoy).OrderByDescending(c => c.FechaConvocatoria)
            .ThenByDescending(c => c.CreadaUtc).ThenByDescending(c => c.IdConvocatoria).Take(3).ToListAsync(ct);
        return filas.Select(c => new ConvocatoriaResumen
        {
            IdConvocatoria = c.IdConvocatoria, Codigo = c.Codigo,
            NombreConvocatoria = c.NombreConvocatoria, Resumen = c.Resumen,
            FechaConvocatoria = c.FechaConvocatoria, FechaCierre = c.FechaCierre,
            EstadoConvocatoria = Estado(c, hoy)
        }).ToList();
    }

    [HttpGet("{id:int}/pdf")]
    [AllowAnonymous]
    public async Task<IActionResult> Pdf(int id, [FromQuery] bool descargar = false, CancellationToken ct = default)
    {
        var hoy = Hoy;
        var fila = await Publicas(hoy).SingleOrDefaultAsync(c => c.IdConvocatoria == id, ct);
        if (fila == null) return NotFound();
        var bytes = ConvocatoriaPdf.Generar(fila, Estado(fila, hoy));
        if (descargar) return File(bytes, "application/pdf", $"convocatoria-{id}.pdf");
        Response.Headers.ContentDisposition = $"inline; filename=convocatoria-{id}.pdf";
        return File(bytes, "application/pdf");
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Inspector")]
    public async Task<IActionResult> Crear(ConvocatoriaForm form, CancellationToken ct)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
            return Unauthorized();
        var codigo = form.Codigo.Trim().ToUpperInvariant();
        if (await db.Convocatoria.AnyAsync(c => c.Codigo == codigo, ct))
            return Conflict(new { mensaje = "Ya existe una convocatoria con ese código." });
        var fila = new Convocatorium
        {
            Codigo = codigo, NombreConvocatoria = form.NombreConvocatoria.Trim(),
            EntidadEmisora = form.EntidadEmisora.Trim(), Resumen = form.Resumen.Trim(),
            Descripcion = form.Descripcion.Trim(), Objetivo = form.Objetivo.Trim(),
            Participantes = form.Participantes.Trim(), Requisitos = form.Requisitos.Trim(),
            Modalidad = form.Modalidad.Trim(), Cronograma = form.Cronograma.Trim(),
            Instrucciones = form.Instrucciones.Trim(), Lugar = form.Lugar.Trim(), Contacto = form.Contacto.Trim(),
            FechaConvocatoria = form.FechaConvocatoria, FechaCierre = form.FechaCierre,
            EstadoConvocatoria = form.EstadoConvocatoria, Publicada = true,
            CreadaUtc = DateTime.UtcNow, CreadaPorUsuarioId = usuarioId
        };
        db.Convocatoria.Add(fila);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return Conflict(new { mensaje = "Ya existe una convocatoria con ese código." });
        }
        return Created($"/api/convocatorias/{fila.IdConvocatoria}/pdf", new { fila.IdConvocatoria });
    }
}

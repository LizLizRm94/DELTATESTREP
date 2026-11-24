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

    [HttpGet("administracion")]
    [Authorize(Roles = "Administrador,Inspector")]
    public async Task<ActionResult<List<ConvocatoriaGestion>>> Administracion(CancellationToken ct)
    {
        var hoy = Hoy;
        var filas = await db.Convocatoria.AsNoTracking()
            .OrderByDescending(c => c.CreadaUtc).ThenByDescending(c => c.IdConvocatoria).ToListAsync(ct);
        return filas.Select(c => new ConvocatoriaGestion
        {
            IdConvocatoria = c.IdConvocatoria, Codigo = c.Codigo,
            NombreConvocatoria = c.NombreConvocatoria, EstadoConvocatoria = Estado(c, hoy),
            FechaConvocatoria = c.FechaConvocatoria, FechaCierre = c.FechaCierre,
            PdfPublicoDisponible = c.Publicada && c.FechaConvocatoria <= hoy,
            Publicacion = !c.Publicada ? "No publicada" : c.FechaConvocatoria == null ? "Sin fecha de publicación"
                : c.FechaConvocatoria > hoy ? "Programada" : "Publicada"
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

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Administrador,Inspector")]
    public async Task<ActionResult<ConvocatoriaForm>> Detalle(int id, CancellationToken ct)
    {
        var fila = await db.Convocatoria.AsNoTracking().SingleOrDefaultAsync(c => c.IdConvocatoria == id, ct);
        if (fila == null) return NotFound();
        return new ConvocatoriaForm
        {
            Codigo = fila.Codigo, NombreConvocatoria = fila.NombreConvocatoria,
            EntidadEmisora = fila.EntidadEmisora, Resumen = fila.Resumen,
            Descripcion = fila.Descripcion, Objetivo = fila.Objetivo,
            Participantes = fila.Participantes, Requisitos = fila.Requisitos,
            Modalidad = fila.Modalidad, Cronograma = fila.Cronograma,
            Instrucciones = fila.Instrucciones, Lugar = fila.Lugar, Contacto = fila.Contacto,
            FechaConvocatoria = fila.FechaConvocatoria, FechaCierre = fila.FechaCierre,
            EstadoConvocatoria = fila.EstadoConvocatoria ?? "Cerrada", Publicada = fila.Publicada
        };
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador,Inspector")]
    public async Task<IActionResult> Actualizar(int id, ConvocatoriaForm form, CancellationToken ct)
    {
        var fila = await db.Convocatoria.SingleOrDefaultAsync(c => c.IdConvocatoria == id, ct);
        if (fila == null) return NotFound();
        var codigo = form.Codigo.Trim().ToUpperInvariant();
        if (await db.Convocatoria.AnyAsync(c => c.IdConvocatoria != id && c.Codigo == codigo, ct))
            return Conflict(new { mensaje = "Ya existe una convocatoria con ese código." });
        Aplicar(fila, form);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { return Conflict(new { mensaje = "Ya existe una convocatoria con ese código." }); }
        catch (DbUpdateConcurrencyException) { return NotFound(); }
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador,Inspector")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        var fila = await db.Convocatoria.Include(c => c.IdUsuarios).SingleOrDefaultAsync(c => c.IdConvocatoria == id, ct);
        if (fila == null) return NotFound();
        if (fila.IdUsuarios.Count != 0)
            return Conflict(new { mensaje = "La convocatoria tiene usuarios vinculados. Puedes cerrarla o retirar su publicación para conservar el historial." });
        db.Convocatoria.Remove(fila);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation })
        { return Conflict(new { mensaje = "La convocatoria tiene registros vinculados y no puede eliminarse." }); }
        catch (DbUpdateConcurrencyException) { return NotFound(); }
        return NoContent();
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
        var fila = new Convocatorium { CreadaUtc = DateTime.UtcNow, CreadaPorUsuarioId = usuarioId };
        Aplicar(fila, form);
        db.Convocatoria.Add(fila);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return Conflict(new { mensaje = "Ya existe una convocatoria con ese código." });
        }
        return Created($"/api/convocatorias/{fila.IdConvocatoria}/pdf", new { fila.IdConvocatoria });
    }

    private static void Aplicar(Convocatorium fila, ConvocatoriaForm form)
    {
        fila.Codigo = form.Codigo.Trim().ToUpperInvariant();
        fila.NombreConvocatoria = form.NombreConvocatoria.Trim();
        fila.EntidadEmisora = form.EntidadEmisora.Trim(); fila.Resumen = form.Resumen.Trim();
        fila.Descripcion = form.Descripcion.Trim(); fila.Objetivo = form.Objetivo.Trim();
        fila.Participantes = form.Participantes.Trim(); fila.Requisitos = form.Requisitos.Trim();
        fila.Modalidad = form.Modalidad.Trim(); fila.Cronograma = form.Cronograma.Trim();
        fila.Instrucciones = form.Instrucciones.Trim(); fila.Lugar = form.Lugar.Trim(); fila.Contacto = form.Contacto.Trim();
        fila.FechaConvocatoria = form.FechaConvocatoria; fila.FechaCierre = form.FechaCierre;
        fila.EstadoConvocatoria = form.EstadoConvocatoria; fila.Publicada = form.Publicada;
    }
}

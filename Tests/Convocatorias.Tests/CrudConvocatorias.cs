using System.Net;
using System.Net.Http.Json;
using Delta.Contracts;
using DELTAAPI.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Convocatorias.Tests;

public class CrudConvocatorias
{
    private static ConvocatoriaForm Form(string codigo = "CRUD-1") => new()
    {
        Codigo = codigo, NombreConvocatoria = "Convocatoria de prueba", Resumen = "Resumen",
        Descripcion = "Descripción", Objetivo = "Objetivo", Participantes = "Laboratorios",
        Requisitos = "Requisitos", Modalidad = "Presencial", Cronograma = "Cronograma",
        Instrucciones = "Instrucciones", Lugar = "Laboratorio", Contacto = "Unidad responsable",
        FechaConvocatoria = new DateOnly(2020, 1, 1), FechaCierre = new DateOnly(2090, 1, 1)
    };

    [Theory]
    [InlineData("Administrador")]
    [InlineData("Inspector")]
    public async Task Gestion_CreaConsultaEditaRetiraPublicacionYElimina(string rol)
    {
        using var f = new ApiFactory(); using var c = f.Client();
        c.DefaultRequestHeaders.Add("Cookie", await f.Cookie(rol));
        c.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        Assert.Equal(HttpStatusCode.Created, (await c.PostAsJsonAsync("api/convocatorias", Form())).StatusCode);
        var inicial = await f.WithDb(async db => await db.Convocatoria.AsNoTracking().SingleAsync());
        var id = inicial.IdConvocatoria;
        var detalle = await c.GetFromJsonAsync<ConvocatoriaForm>($"api/convocatorias/{id}");
        Assert.Equal("Cronograma", detalle!.Cronograma);
        detalle.NombreConvocatoria = "Título actualizado"; detalle.Publicada = false;
        Assert.Equal(HttpStatusCode.NoContent, (await c.PutAsJsonAsync($"api/convocatorias/{id}", detalle)).StatusCode);
        await f.WithDb(async db =>
        {
            var fila = await db.Convocatoria.SingleAsync();
            Assert.Equal("Título actualizado", fila.NombreConvocatoria);
            Assert.Equal(inicial.CreadaUtc, fila.CreadaUtc);
            Assert.Equal(inicial.CreadaPorUsuarioId, fila.CreadaPorUsuarioId);
            return true;
        });
        var lista = await c.GetFromJsonAsync<List<ConvocatoriaGestion>>("api/convocatorias/administracion");
        Assert.Equal("No publicada", Assert.Single(lista!).Publicacion);
        using var publico = f.Client();
        Assert.Empty((await publico.GetFromJsonAsync<List<ConvocatoriaResumen>>("api/convocatorias"))!);
        Assert.Equal(HttpStatusCode.NotFound, (await publico.GetAsync($"api/convocatorias/{id}/pdf")).StatusCode);
        detalle.Publicada = true;
        Assert.Equal(HttpStatusCode.NoContent, (await c.PutAsJsonAsync($"api/convocatorias/{id}", detalle)).StatusCode);
        Assert.Equal("Título actualizado", Assert.Single((await publico.GetFromJsonAsync<List<ConvocatoriaResumen>>("api/convocatorias"))!).NombreConvocatoria);
        Assert.Equal(HttpStatusCode.NoContent, (await c.DeleteAsync($"api/convocatorias/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync($"api/convocatorias/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await publico.GetAsync($"api/convocatorias/{id}/pdf")).StatusCode);
    }

    [Theory]
    [InlineData("GET", "administracion")]
    [InlineData("GET", "1")]
    [InlineData("PUT", "1")]
    [InlineData("DELETE", "1")]
    public async Task Gestion_ExigeAutenticacionYRol(string method, string ruta)
    {
        using var f = new ApiFactory();
        foreach (var rol in new string?[] { null, "Usuario" })
        {
            using var c = f.Client();
            if (rol != null) c.DefaultRequestHeaders.Add("Cookie", await f.Cookie(rol));
            c.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            using var request = new HttpRequestMessage(new HttpMethod(method), $"api/convocatorias/{ruta}");
            if (method == "PUT") request.Content = JsonContent.Create(Form());
            Assert.Equal(rol == null ? HttpStatusCode.Unauthorized : HttpStatusCode.Forbidden, (await c.SendAsync(request)).StatusCode);
        }
    }

    [Fact]
    public async Task Gestion_ListaCompleta_ValidaEdicion_Y_ConservaVinculos()
    {
        using var f = new ApiFactory(); using var c = f.Client();
        c.DefaultRequestHeaders.Add("Cookie", await f.Cookie("Inspector"));
        c.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        for (var i = 1; i <= 5; i++)
        {
            var form = Form("CRUD-" + i);
            if (i == 5) { form.FechaConvocatoria = new DateOnly(2080, 1, 1); }
            Assert.Equal(HttpStatusCode.Created, (await c.PostAsJsonAsync("api/convocatorias", form)).StatusCode);
        }
        var lista = (await c.GetFromJsonAsync<List<ConvocatoriaGestion>>("api/convocatorias/administracion"))!;
        Assert.Equal(5, lista.Count);
        Assert.Equal("CRUD-5", lista[0].Codigo); Assert.Equal("Programada", lista[0].Publicacion);
        Assert.False(lista[0].PdfPublicoDisponible);
        var id = lista[0].IdConvocatoria;
        Assert.Equal(HttpStatusCode.Conflict, (await c.PutAsJsonAsync($"api/convocatorias/{id}", Form(" crud-1 "))).StatusCode);
        var invalida = Form("CRUD-5"); invalida.FechaCierre = new DateOnly(2010, 1, 1);
        Assert.Equal(HttpStatusCode.BadRequest, (await c.PutAsJsonAsync($"api/convocatorias/{id}", invalida)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await c.PutAsJsonAsync("api/convocatorias/999999", Form())).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await c.DeleteAsync("api/convocatorias/999999")).StatusCode);
        await f.WithDb(async db =>
        {
            var fila = await db.Convocatoria.SingleAsync(x => x.IdConvocatoria == id);
            fila.IdUsuarios.Add(await db.Usuarios.FirstAsync());
            await db.SaveChangesAsync(); return true;
        });
        Assert.Equal(HttpStatusCode.Conflict, (await c.DeleteAsync($"api/convocatorias/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await c.GetAsync($"api/convocatorias/{id}")).StatusCode);
    }
}

using System.Net;
using System.Net.Http.Json;
using DELTAAPI.Models;
using Xunit;

namespace Convocatorias.Tests;

public class AuditoriaModulos
{
    public static IEnumerable<object[]> Privados => new[] {
        "api/usuarios", "api/usuarios/2", "api/evaluaciones", "api/evaluaciones/10",
        "api/evaluaciones/usuario/2", "api/evaluaciones/ultima-practica/2",
        "api/evaluados/estado/2", "api/notificaciones/usuario/2", "api/preguntas/teoricas",
        "api/preguntas/evaluacion/10", "api/preguntas/20", "api/respuestas/evaluacion/10",
        "api/respuestas/usuario/2", "api/evaluacionesteoricass", "api/evaluacionesteoricass/usuario/2",
        "api/evaluacionesteoricass/respuestas/2/10", "api/evaluacionesteoricass/descargar-pdf/2/10",
        "api/evaluacionesteoricass/pendientes", "api/evaluacionesteoricass/calificar/10",
        "api/evaluacionesteoricass/debug/todas"
    }.Select(p => new object[] { p });

    [Theory, MemberData(nameof(Privados))]
    public async Task SinSesion_NoExponeModulosPrivados(string url)
    {
        using var f = new ApiFactory(); using var c = f.Client();
        var response = await c.GetAsync(url);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }
    public static IEnumerable<object[]> Ajenos => new[] {
        "api/usuarios/2", "api/evaluaciones/10", "api/evaluaciones/usuario/2",
        "api/evaluaciones/ultima-practica/2", "api/evaluados/estado/2", "api/notificaciones/usuario/2",
        "api/preguntas/evaluacion/10", "api/respuestas/evaluacion/10", "api/respuestas/usuario/2",
        "api/evaluacionesteoricass/usuario/2", "api/evaluacionesteoricass/respuestas/2/10",
        "api/evaluacionesteoricass/descargar-pdf/2/10", "api/usuarios", "api/evaluaciones",
        "api/preguntas/teoricas", "api/evaluacionesteoricass/pendientes"
    }.Select(p => new object[] { p });
    private static async Task<HttpClient> Usuario(ApiFactory f)
    {
        var cookie = await f.Cookie("Usuario");
        await f.WithDb(async db => {
            db.Usuarios.Add(new Usuario { IdUsuario=2, NombreCompleto="Otro", Ci="otro", Contraseña="hash-privado", Rol="Usuario", Estado="Activo" });
            db.Evaluacions.Add(new Evaluacion { IdEvaluacion=10, IdEvaluado=2, TipoEvaluacion=true, EstadoEvaluacion="Pendiente" });
            db.Preguntas.Add(new Pregunta { IdPregunta=20, IdEvaluacion=10, Texto="Pregunta", TipoEvaluacion=true });
            await db.SaveChangesAsync(); return true;
        });
        var c=f.Client();c.DefaultRequestHeaders.Add("Cookie",cookie);c.DefaultRequestHeaders.Add("X-Requested-With","XMLHttpRequest");return c;
    }
    [Theory, MemberData(nameof(Ajenos))]
    public async Task Usuario_NoLeeDatosAjenosNiFuncionesAdministrativas(string url)
    {
        using var f=new ApiFactory();using var c=await Usuario(f);
        Assert.Equal(HttpStatusCode.Forbidden,(await c.GetAsync(url)).StatusCode);
    }
    [Theory]
    [InlineData("api/respuestas/guardar")]
    [InlineData("api/evaluacionesteoricass/guardar-respuestas")]
    public async Task Respuestas_VaciasYDuplicadas_NoDestruyenEnvioAnterior(string url)
    {
        using var f=new ApiFactory();using var c=await Usuario(f);
        await f.WithDb(async db => {var e=await db.Evaluacions.FindAsync(10);e!.IdEvaluado=1;
            db.Respuestas.Add(new Respuesta { IdEvaluacion=10, IdPregunta=20, IdUsuario=1,TextoRespuesta="Anterior" });await db.SaveChangesAsync();return true;});
        foreach(var respuestas in new[] { Array.Empty<object>(), new object[] { new {idPregunta=20,textoRespuesta="a"},new {idPregunta=20,textoRespuesta="b"} } })
            Assert.Equal(HttpStatusCode.BadRequest,(await c.PostAsJsonAsync(url,new {idUsuario=1,idEvaluacion=10,respuestas})).StatusCode);
        await f.WithDb(async db => {Assert.Equal("Anterior",(await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.SingleAsync(db.Respuestas)).TextoRespuesta);return true;});
        Assert.Equal(HttpStatusCode.OK,(await c.PostAsJsonAsync(url,new {idUsuario=1,idEvaluacion=10,respuestas=new[]{new{idPregunta=20,textoRespuesta="Nueva"}}})).StatusCode);
        await f.WithDb(async db => {Assert.Equal("Nueva",(await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.SingleAsync(db.Respuestas)).TextoRespuesta);return true;});
    }
    [Fact]
    public async Task ResumenPropio_NoSerializaContraseñasNiEntidadesRelacionadas()
    {
        using var f=new ApiFactory();using var c=await Usuario(f);
        await f.WithDb(async db => {var e=await db.Evaluacions.FindAsync(10);e!.IdEvaluado=1;await db.SaveChangesAsync();return true;});
        var response=await c.GetAsync("api/evaluados/estado/1");Assert.Equal(HttpStatusCode.OK,response.StatusCode);
        var json=await response.Content.ReadAsStringAsync();Assert.DoesNotContain("contraseña",json,StringComparison.OrdinalIgnoreCase);Assert.DoesNotContain("hash-privado",json);
    }

    [Theory]
    [InlineData("api/usuarios/1", "PUT")]
    [InlineData("api/usuarios/2", "DELETE")]
    [InlineData("api/evaluaciones/10", "DELETE")]
    [InlineData("api/evaluacionesteoricass/calificar/10", "PUT")]
    [InlineData("api/preguntas/20", "DELETE")]
    public async Task Usuario_NoEscalaPrivilegios(string url, string method)
    {
        using var f=new ApiFactory();using var c=await Usuario(f);
        using var request=new HttpRequestMessage(new HttpMethod(method),url) { Content=JsonContent.Create(new {rol="Administrador",nota=100}) };
        Assert.Equal(HttpStatusCode.Forbidden,(await c.SendAsync(request)).StatusCode);
    }
    [Fact]
    public async Task EvaluacionCalificada_NoAdmiteReescrituraDeRespuestas()
    {
        using var f=new ApiFactory();using var c=await Usuario(f);
        await f.WithDb(async db=>{var e=await db.Evaluacions.FindAsync(10);e!.IdEvaluado=1;e.EstadoEvaluacion="Aprobada";await db.SaveChangesAsync();return true;});
        Assert.Equal(HttpStatusCode.Conflict,(await c.PostAsJsonAsync("api/respuestas/guardar",new {idEvaluacion=10,respuestas=new[]{new{idPregunta=20,textoRespuesta="Cambio"}}})).StatusCode);
    }
    [Fact]
    public async Task PreguntasPropias_NoRevelanLaRespuestaCorrecta()
    {
        using var f=new ApiFactory();using var c=await Usuario(f);
        await f.WithDb(async db=>{var e=await db.Evaluacions.FindAsync(10);e!.IdEvaluado=1;var q=await db.Preguntas.FindAsync(20);q!.RespuestaCorrectaIndex=0;await db.SaveChangesAsync();return true;});
        var response=await c.GetAsync("api/preguntas/evaluacion/10");Assert.Equal(HttpStatusCode.OK,response.StatusCode);
        Assert.DoesNotContain("respuestaCorrectaIndex",await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Administrador_CreaTeoricaConPreguntas_Y_RechazaLoteVacio()
    {
        using var f=new ApiFactory();using var usuario=await Usuario(f);
        using var admin=f.Client();admin.DefaultRequestHeaders.Add("Cookie",await f.Cookie());admin.DefaultRequestHeaders.Add("X-Requested-With","XMLHttpRequest");
        var invalid=await admin.PostAsJsonAsync("api/evaluaciones/crear-evaluacion-teorica",new{idUsuario=2,preguntas=new[]{new{texto=" ",tipoEvaluacion=true,puntos=10}}});
        Assert.Equal(HttpStatusCode.BadRequest,invalid.StatusCode);
        var valid=await admin.PostAsJsonAsync("api/evaluaciones/crear-evaluacion-teorica",new{idUsuario=2,preguntas=new[]{new{texto="Pregunta nueva",tipoEvaluacion=true,puntos=10}}});
        Assert.Equal(HttpStatusCode.OK,valid.StatusCode);
        await f.WithDb(async db=>{var q=await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.SingleAsync(db.Preguntas,p=>p.Texto=="Pregunta nueva");Assert.True(q.IdEvaluacion>0);Assert.NotNull(await db.Evaluacions.FindAsync(q.IdEvaluacion));return true;});
    }
    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public async Task Practica_NoAceptaCalificacionesFueraDeEscala(int calificacion)
    {
        using var f=new ApiFactory();using var c=f.Client();c.DefaultRequestHeaders.Add("Cookie",await f.Cookie());c.DefaultRequestHeaders.Add("X-Requested-With","XMLHttpRequest");
        Assert.Equal(HttpStatusCode.BadRequest,(await c.PostAsJsonAsync("api/evaluaciones/crear-evaluacion-practica",new{idUsuario=1,tareas=new[]{new{descripcion="Tarea",calificacion}}})).StatusCode);
    }
}

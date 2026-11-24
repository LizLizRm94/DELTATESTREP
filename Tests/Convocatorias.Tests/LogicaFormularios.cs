using System.Net;
using System.Net.Http.Json;
using DELTAAPI.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
namespace Convocatorias.Tests;
public class LogicaFormularios
{
    [Fact]
    public void CheckDeEstado_ConvierteAmbosValoresSinTextoLibre()
    {
        var model=new DELTATEST.Models.EditUsuarioModel{Estado="Inactivo"};Assert.False(model.Activo);
        model.Activo=true;Assert.Equal("Activo",model.Estado);
        model.Activo=false;Assert.Equal("Inactivo",model.Estado);
    }

    [Theory]
    [InlineData(true,"Activo")]
    [InlineData(false,"Inactivo")]
    public async Task Inspector_ActualizaEstadoConBooleano(bool activo,string esperado)
    {
        using var f=new ApiFactory();using var c=f.Client();c.DefaultRequestHeaders.Add("Cookie",await f.Cookie("Inspector"));c.DefaultRequestHeaders.Add("X-Requested-With","XMLHttpRequest");
        await f.WithDb(async db=>{db.Usuarios.Add(new Usuario{IdUsuario=2,NombreCompleto="Usuario",Ci="000123",Contraseña="hash",Rol="Usuario",Estado="Activo"});await db.SaveChangesAsync();return true;});
        Assert.Equal(HttpStatusCode.NoContent,(await c.PutAsJsonAsync("api/usuarios/2",new{activo})).StatusCode);
        await f.WithDb(async db=>{Assert.Equal(esperado,(await db.Usuarios.FindAsync(2))!.Estado);return true;});
    }
    [Fact]
    public async Task Booleano_NoPermiteDesactivarAdministradorPropio_NiEstadosContradictorios()
    {
        using var f=new ApiFactory();using var c=f.Client();c.DefaultRequestHeaders.Add("Cookie",await f.Cookie());c.DefaultRequestHeaders.Add("X-Requested-With","XMLHttpRequest");
        Assert.Equal(HttpStatusCode.BadRequest,(await c.PutAsJsonAsync("api/usuarios/1",new{activo=false})).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await c.PutAsJsonAsync("api/usuarios/1",new{activo=true,estado="Inactivo"})).StatusCode);
    }
    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public async Task PreguntaMultiple_ExigeUnaUnicaCorrecta(int correctas)
    {
        using var f=new ApiFactory();using var c=f.Client();c.DefaultRequestHeaders.Add("Cookie",await f.Cookie());c.DefaultRequestHeaders.Add("X-Requested-With","XMLHttpRequest");
        var options=new[]{new{texto="A",esCorrecta=correctas>0},new{texto="B",esCorrecta=correctas>1}};
        Assert.Equal(HttpStatusCode.BadRequest,(await c.PostAsJsonAsync("api/evaluaciones/crear-evaluacion-teorica",new{idUsuario=1,preguntas=new[]{new{texto="Pregunta",tipoEvaluacion=false,puntos=10,opcionesConCorrecta=options}}})).StatusCode);
        await f.WithDb(async db=>{Assert.Empty(await db.Evaluacions.ToListAsync());return true;});
    }
    [Fact]
    public async Task PreguntasHistoricas_NoExponenIndicadoresDeSolucionDentroDelJson()
    {
        using var f=new ApiFactory();using var c=f.Client();c.DefaultRequestHeaders.Add("Cookie",await f.Cookie("Usuario"));
        await f.WithDb(async db=>{db.Evaluacions.Add(new Evaluacion{IdEvaluacion=1,IdEvaluado=1,TipoEvaluacion=true});db.Preguntas.Add(new Pregunta{IdPregunta=1,IdEvaluacion=1,Texto="Pregunta",Opciones="[{\"texto\":\"A\",\"esCorrecta\":true},{\"texto\":\"B\",\"esCorrecta\":false}]",RespuestaCorrectaIndex=0});await db.SaveChangesAsync();return true;});
        var response=await c.GetAsync("api/preguntas/evaluacion/1");Assert.Equal(HttpStatusCode.OK,response.StatusCode);Assert.DoesNotContain("esCorrecta",await response.Content.ReadAsStringAsync());
    }
}

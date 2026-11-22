using System.Net;
using System.Net.Http.Json;
using DELTAAPI.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
namespace Convocatorias.Tests;
public class DetallesYPuntajes
{
    [Fact]
    public async Task Practica_ConservaDetalleEInstrumento_Y_LoDevuelve()
    {
        using var f=new ApiFactory();using var c=f.Client();c.DefaultRequestHeaders.Add("Cookie",await f.Cookie());c.DefaultRequestHeaders.Add("X-Requested-With","XMLHttpRequest");
        var response=await c.PostAsJsonAsync("api/evaluaciones/crear-evaluacion-practica",new{idUsuario=1,tareas=new[]{new{idTarea=1,descripcion="Pesaje",instrumento="Balanza",calificacion=8,resultadoObtenido="Correcto",completada=true}}});
        Assert.Equal(HttpStatusCode.OK,response.StatusCode);
        var id=await f.WithDb(async db=>{var e=await db.Evaluacions.SingleAsync();Assert.Equal(80m,e.Nota);Assert.Contains("Balanza",e.DetallePracticoJson!);return e.IdEvaluacion;});
        var detail=await c.GetAsync($"api/evaluaciones/{id}");Assert.Equal(HttpStatusCode.OK,detail.StatusCode);Assert.Contains("Balanza",await detail.Content.ReadAsStringAsync());
    }
    [Theory]
    [InlineData(6, HttpStatusCode.OK)]
    [InlineData(11, HttpStatusCode.BadRequest)]
    public async Task Calificar_NoSobrescribeMaximo_Y_ValidaLimite(int puntos,HttpStatusCode expected)
    {
        using var f=new ApiFactory();using var c=f.Client();c.DefaultRequestHeaders.Add("Cookie",await f.Cookie());c.DefaultRequestHeaders.Add("X-Requested-With","XMLHttpRequest");
        await f.WithDb(async db=>{db.Evaluacions.Add(new Evaluacion{IdEvaluacion=10,IdEvaluado=1,TipoEvaluacion=true,EstadoEvaluacion="Respondida"});db.Preguntas.Add(new Pregunta{IdPregunta=20,IdEvaluacion=10,Texto="Pregunta",TipoEvaluacion=true,Puntos=10});await db.SaveChangesAsync();return true;});
        var response=await c.PutAsJsonAsync("api/evaluacionesteoricass/calificar/10",new{nota=60,preguntasConPuntos=new[]{new{idPregunta=20,puntos}}});
        Assert.Equal(expected,response.StatusCode);
        await f.WithDb(async db=>{var p=await db.Preguntas.FindAsync(20);Assert.Equal(10,p!.Puntos);Assert.Equal(expected==HttpStatusCode.OK?6:(int?)null,p.PuntosObtenidos);return true;});
    }
}

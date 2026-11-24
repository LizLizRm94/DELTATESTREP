using System.Net;
using System.Net.Http.Json;
using DELTAAPI.Models;
using DELTATEST.Services;
using Xunit;
namespace Convocatorias.Tests;
public class AccesoYPanel
{
    [Fact]
    public async Task Panel_SeleccionaUltimaNotaReal_Y_RechazaOtroUsuario()
    {
        using var f=new ApiFactory();using var c=f.Client();c.DefaultRequestHeaders.Add("Cookie",await f.Cookie("Usuario"));
        await f.WithDb(async db=>{
            db.Evaluacions.AddRange(new Evaluacion{IdEvaluacion=1,IdEvaluado=1,TipoEvaluacion=false,Nota=90,FechaEvaluacion=new DateOnly(2026,1,1)},new Evaluacion{IdEvaluacion=2,IdEvaluado=1,TipoEvaluacion=true,Nota=85,FechaEvaluacion=new DateOnly(2026,2,1)},new Evaluacion{IdEvaluacion=3,IdEvaluado=1,TipoEvaluacion=true,Nota=null,FechaEvaluacion=new DateOnly(2026,3,1)});
            await db.SaveChangesAsync();return true;});
        var response=await c.GetAsync("api/evaluados/panel/1");Assert.Equal(HttpStatusCode.OK,response.StatusCode);
        var json=await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();Assert.Equal(2,json.GetProperty("ultimaEvaluacion").GetProperty("idEvaluacion").GetInt32());
        Assert.True(response.Headers.CacheControl!.NoStore);
        Assert.Equal(HttpStatusCode.Forbidden,(await c.GetAsync("api/evaluados/panel/2")).StatusCode);
        using var anonymous=f.Client();Assert.Equal(HttpStatusCode.Unauthorized,(await anonymous.GetAsync("api/evaluados/panel/1")).StatusCode);
    }
    [Fact]
    public async Task EstadoAutenticacion_AgrupaSoloConsultasConcurrentes_Y_RevalidaDespues()
    {
        var handler=new ControlledHandler();using var http=new HttpClient(handler){BaseAddress=new Uri("https://localhost")};
        var provider=new CookieAuthenticationStateProvider(http);
        var first=provider.GetAuthenticationStateAsync();var second=provider.GetAuthenticationStateAsync();
        handler.Ready.SetResult();var result=await Task.WhenAll(first,second);
        Assert.Equal(1,handler.Count);Assert.All(result,s=>Assert.True(s.User.IsInRole("Inspector")));
        handler.Expired=true;Assert.False((await provider.GetAuthenticationStateAsync()).User.Identity!.IsAuthenticated);Assert.Equal(2,handler.Count);
    }
    private class ControlledHandler:HttpMessageHandler
    {
        public TaskCompletionSource Ready=new(TaskCreationOptions.RunContinuationsAsynchronously);public int Count;public bool Expired;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            Count++;await Ready.Task;
            return Expired?new(HttpStatusCode.Unauthorized):new(HttpStatusCode.OK){Content=JsonContent.Create(new{idUsuario=1,nombreCompleto="Inspector",rol="Inspector"})};
        }
    }
}

using System.Net;
using System.Net.Http.Json;
using Delta.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;
namespace Convocatorias.Tests;
public class RegistroLaboral
{
    [Theory]
    [InlineData("Empleado","Cargo A","Cargo B",null)]
    [InlineData("Pasante",null,null,"Cargo A")]
    public async Task Registro_ConservaTipoYPuestosCorrespondientes(string tipo,string? actual,string? rotar,string? solicitado)
    {
        using var f=new ApiFactory();using var c=f.Client();c.DefaultRequestHeaders.Add("Cookie",await f.Cookie());c.DefaultRequestHeaders.Add("X-Requested-With","XMLHttpRequest");
        var response=await c.PostAsJsonAsync("api/auth/register",new{nombreCompleto="Cuenta prueba",correo="laboral@example.test",ci="000555",password="PruebaSegura123!",role="Usuario",tipoEvaluado=tipo,puestoActual=actual,puestoARotar=rotar,puestoSolicitado=solicitado});
        Assert.Equal(HttpStatusCode.Created,response.StatusCode);
        await f.WithDb(async db=>{var u=await db.Usuarios.SingleAsync(u=>u.Correo=="laboral@example.test");Assert.Equal(tipo,u.TipoEvaluado);Assert.Equal(actual,u.PuestoActual);Assert.Equal(rotar,u.PuestoARotar);Assert.Equal(solicitado,u.PuestoSolicitado);return true;});
    }
    [Theory]
    [InlineData("Empleado",null,null,"Cargo A")]
    [InlineData("Pasante","Cargo A",null,"Cargo B")]
    [InlineData("Pasante",null,null,null)]
    [InlineData("Desconocido","Cargo A","Cargo B",null)]
    public void ValidacionLaboral_NoAceptaCamposIncompatibles(string tipo,string? actual,string? rotar,string? solicitado)
        =>Assert.NotNull(VinculacionLaboral.Validar(tipo,actual,rotar,solicitado));
}

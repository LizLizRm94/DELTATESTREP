using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Convocatorias.Tests;

public class CatalogoAreas
{
    [Theory]
    [InlineData("Administrador")]
    [InlineData("Inspector")]
    public async Task PersonalAutorizado_RecibeAreasProvisionales(string rol)
    {
        using var factory = new ApiFactory();
        using var client = factory.Client();
        client.DefaultRequestHeaders.Add("Cookie", await factory.Cookie(rol));
        var response = await client.GetAsync("api/usuarios/cargos");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var areas = await response.Content.ReadFromJsonAsync<List<string>>();
        Assert.NotNull(areas);
        Assert.Contains("Producción farmacéutica", areas);
        Assert.Contains("Farmacovigilancia", areas);
        Assert.True(areas.Count >= 10);
        Assert.Equal(areas.Count, areas.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public async Task Catalogo_NoEsPublico()
    {
        using var factory = new ApiFactory();
        using var client = factory.Client();
        var response = await client.GetAsync("api/usuarios/cargos");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Convocatorias.Tests;

public class CatalogoAreas
{
    [Theory]
    [InlineData("Administrador")]
    [InlineData("Inspector")]
    public async Task PersonalAutorizado_RecibeCargosProporcionados(string rol)
    {
        using var factory = new ApiFactory();
        using var client = factory.Client();
        client.DefaultRequestHeaders.Add("Cookie", await factory.Cookie(rol));
        var response = await client.GetAsync("api/usuarios/cargos");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var areas = await response.Content.ReadFromJsonAsync<List<string>>();
        Assert.NotNull(areas);
        Assert.Contains("Jefe de control de calidad", areas);
        Assert.Contains("Inspector", areas);
        Assert.Contains("Encargado de validaciones de metodologías analíticas", areas);
        Assert.DoesNotContain("Producción farmacéutica", areas);
        Assert.Equal(8, areas.Count);
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

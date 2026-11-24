using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Delta.Contracts;
using DELTAAPI.Controllers;
using DELTAAPI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Convocatorias.Tests;

public sealed class ApiFactory : WebApplicationFactory<ConvocatoriasController>
{
    private readonly string databaseName = Guid.NewGuid().ToString();
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Host=localhost;Database=pruebas;Username=pruebas;Password=solo-pruebas");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<DeltaTestContext>>();
            services.RemoveAll<DeltaTestContext>();
            services.AddDbContext<DeltaTestContext>(options => options.UseInMemoryDatabase(databaseName));
            services.AddDataProtection().UseEphemeralDataProtectionProvider();
        });
    }
    public HttpClient Client() => CreateClient(new WebApplicationFactoryClientOptions
    { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false, HandleCookies = false });
    public async Task<T> WithDb<T>(Func<DeltaTestContext, Task<T>> action)
    {
        using var scope = Services.CreateScope();
        return await action(scope.ServiceProvider.GetRequiredService<DeltaTestContext>());
    }
    public async Task<string> Cookie(string role = "Administrador", bool expired = false)
    {
        var id = await WithDb(async db =>
        {
            var user = new Usuario { NombreCompleto = "Prueba", Ci = Guid.NewGuid().ToString(),
                Contraseña = "sin-login", Rol = role, Estado = "Activo" };
            db.Usuarios.Add(user); await db.SaveChangesAsync(); return user.IdUsuario;
        });
        var options = Services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(CookieAuthenticationDefaults.AuthenticationScheme);
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()),
            new Claim(ClaimTypes.Role, role), new Claim("sid", Guid.NewGuid().ToString()) };
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies")),
            new AuthenticationProperties { IssuedUtc = DateTimeOffset.UtcNow.AddMinutes(-10),
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(expired ? -1 : 20) }, "Cookies");
        return "__Host-DeltaAuth=" + options.TicketDataFormat.Protect(ticket);
    }
}

public class FlujoConvocatorias
{
    private static ConvocatoriaForm Form(string code = "LAB-001") => new()
    {
        Codigo = code, NombreConvocatoria = "Evaluación del informe técnico",
        Resumen = "Participación de laboratorios", Descripcion = "Evaluación de desempeño.",
        Objetivo = "Evaluar habilidades y capacidades.", Participantes = "Control de Calidad, Rotación y pasantes.",
        Requisitos = "Documentación y cumplimiento de procedimientos.", Modalidad = "Teórica digital y práctica presencial.",
        Cronograma = "Horarios comunicados por inspectores.", Instrucciones = "La participación es obligatoria.",
        Lugar = "Plataforma interna y laboratorios", Contacto = "Unidad de inspección",
        FechaConvocatoria = Today.AddDays(-1), FechaCierre = Today.AddDays(5)
    };
    private static DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "America/La_Paz"));
    private static HttpClient Admin(ApiFactory factory, string cookie)
    {
        var client = factory.Client(); client.DefaultRequestHeaders.Add("Cookie", cookie);
        client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest"); return client;
    }

    [Fact]
    public async Task Crear_Persiste_ValidaDuplicado_Y_PdfPublico()
    {
        using var factory = new ApiFactory();
        using var admin = Admin(factory, await factory.Cookie());
        var created = await admin.PostAsJsonAsync("api/convocatorias", Form(" lab-001 "));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        await factory.WithDb(async db =>
        {
            var fila = await db.Convocatoria.SingleAsync();
            Assert.Equal("LAB-001", fila.Codigo); Assert.NotNull(fila.CreadaPorUsuarioId);
            Assert.True(fila.Publicada); Assert.Equal("Horarios comunicados por inspectores.", fila.Cronograma);
            return true;
        });
        Assert.Equal(HttpStatusCode.Conflict, (await admin.PostAsJsonAsync("api/convocatorias", Form())).StatusCode);
        using var publico = factory.Client();
        var filas = await publico.GetFromJsonAsync<List<ConvocatoriaResumen>>("api/convocatorias");
        var id = Assert.Single(filas!).IdConvocatoria;
        var pdf = await publico.GetAsync($"api/convocatorias/{id}/pdf");
        Assert.Equal("application/pdf", pdf.Content.Headers.ContentType!.MediaType);
        Assert.Equal("inline", pdf.Content.Headers.ContentDisposition!.DispositionType);
        Assert.StartsWith("%PDF-", System.Text.Encoding.ASCII.GetString(await pdf.Content.ReadAsByteArrayAsync()));
        var download = await publico.GetAsync($"api/convocatorias/{id}/pdf?descargar=true");
        Assert.Equal("attachment", download.Content.Headers.ContentDisposition!.DispositionType);
        Assert.True(pdf.Headers.CacheControl!.NoStore);
        // Para inspección visual opcional; el test nunca escribe datos de prueba en Supabase.
        var output = Environment.GetEnvironmentVariable("CONVOCATORIA_PDF_QA");
        if (!string.IsNullOrEmpty(output)) await File.WriteAllBytesAsync(output, await pdf.Content.ReadAsByteArrayAsync());
    }

    [Theory]
    [InlineData(null, false, HttpStatusCode.Unauthorized)]
    [InlineData("Inspector", false, HttpStatusCode.Created)]
    [InlineData("Usuario", false, HttpStatusCode.Forbidden)]
    [InlineData("Administrador", true, HttpStatusCode.Unauthorized)]
    public async Task Crear_PermiteAdministradorEInspector_Y_ExigeCookieVigente(string? role, bool expired, HttpStatusCode expected)
    {
        using var factory = new ApiFactory();
        using var client = factory.Client();
        client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        if (role != null) client.DefaultRequestHeaders.Add("Cookie", await factory.Cookie(role, expired));
        var response = await client.PostAsJsonAsync("api/convocatorias", Form());
        Assert.Equal(expected, response.StatusCode);
        if (expected == HttpStatusCode.Created) Assert.NotNull(response.Headers.Location);
        else Assert.Null(response.Headers.Location);
    }

    [Fact]
    public async Task Crear_RechazaCsrf_FechasInvalidas_Y_CamposVacios()
    {
        using var factory = new ApiFactory();
        var cookie = await factory.Cookie();
        using var client = factory.Client(); client.DefaultRequestHeaders.Add("Cookie", cookie);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("api/convocatorias", Form())).StatusCode);
        client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        var form = Form(); form.FechaCierre = form.FechaConvocatoria!.Value.AddDays(-1);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("api/convocatorias", form)).StatusCode);
        form = Form(); form.Requisitos = " ";
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("api/convocatorias", form)).StatusCode);
        form = Form(); form.EstadoConvocatoria = "Desconocido";
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("api/convocatorias", form)).StatusCode);
    }

    [Fact]
    public async Task Inicio_SoloTresRecientes_ExcluyeFuturasYLegadas_CierraAlVencer()
    {
        using var factory = new ApiFactory();
        using var admin = Admin(factory, await factory.Cookie());
        for (var i = 1; i <= 4; i++)
        {
            var form = Form("LAB-" + i); form.FechaConvocatoria = Today.AddDays(-i);
            form.FechaCierre = i == 1 ? Today.AddDays(-1) : Today;
            Assert.Equal(HttpStatusCode.Created, (await admin.PostAsJsonAsync("api/convocatorias", form)).StatusCode);
        }
        var future = Form("FUTURA"); future.FechaConvocatoria = Today.AddDays(1); future.FechaCierre = Today.AddDays(2);
        var response = await admin.PostAsJsonAsync("api/convocatorias", future);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var legacyId = await factory.WithDb(async db =>
        {
            var old = new Convocatorium { NombreConvocatoria = "Legada", FechaConvocatoria = Today, Publicada = false };
            db.Convocatoria.Add(old); await db.SaveChangesAsync(); return old.IdConvocatoria;
        });
        using var publico = factory.Client();
        var filas = await publico.GetFromJsonAsync<List<ConvocatoriaResumen>>("api/convocatorias");
        Assert.Equal(new[] { "LAB-1", "LAB-2", "LAB-3" }, filas!.Select(c => c.Codigo));
        Assert.Equal("Cerrada", filas![0].EstadoConvocatoria); Assert.Equal("Activa", filas![1].EstadoConvocatoria);
        Assert.Equal(HttpStatusCode.NotFound, (await publico.GetAsync(response.Headers.Location)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await publico.GetAsync($"api/convocatorias/{legacyId}/pdf")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await publico.GetAsync("api/convocatorias/999999/pdf")).StatusCode);
    }

    [Fact]
    public async Task Cookie_SeRevocaAlCerrarSesion_Y_AlCambiarRol()
    {
        using var factory = new ApiFactory();
        var cookie = await factory.Cookie();
        using var admin = Admin(factory, cookie);
        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsync("api/auth/logout", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await admin.PostAsJsonAsync("api/convocatorias", Form())).StatusCode);
        using var otra = Admin(factory, await factory.Cookie());
        await factory.WithDb(async db =>
        {
            var user = await db.Usuarios.OrderByDescending(u => u.IdUsuario).FirstAsync();
            user.Rol = "Usuario"; await db.SaveChangesAsync(); return true;
        });
        Assert.Equal(HttpStatusCode.Unauthorized, (await otra.PostAsJsonAsync("api/convocatorias", Form())).StatusCode);
    }
    [Fact]
    public async Task Login_EmiteCookieSegura_NoPersistente_Y_SinCacheDeUsuario()
    {
        using var factory = new ApiFactory();
        var hash = BCrypt.Net.BCrypt.HashPassword("PruebaSegura123!", workFactor: 4);
        await factory.WithDb(async db =>
        {
            db.Usuarios.Add(new Usuario { NombreCompleto = "Admin", Ci = "1234567", Correo = "admin@example.test",
                Contraseña = hash, Rol = "Administrador", Estado = "Activo" });
            await db.SaveChangesAsync(); return true;
        });
        using var client = factory.Client();
        client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        var login = await client.PostAsJsonAsync("api/auth/login", new { Username = "admin@example.test", Password = "PruebaSegura123!" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var cookie = Assert.Single(login.Headers.GetValues("Set-Cookie"));
        Assert.StartsWith("__Host-DeltaAuth=", cookie);
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("path=/", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("domain=", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("expires=", cookie, StringComparison.OrdinalIgnoreCase);
        client.DefaultRequestHeaders.Add("Cookie", cookie.Split(';')[0]);
        var current = await client.GetAsync("api/auth/current-user");
        Assert.Equal(HttpStatusCode.OK, current.StatusCode);
        Assert.True(current.Headers.CacheControl!.NoStore);
    }

    [Fact]
    public async Task Pdf_SoportaContenidoLargo_Y_ConservaTodosLosApartados()
    {
        using var factory = new ApiFactory();
        using var admin = Admin(factory, await factory.Cookie());
        var form = Form("LAB-LARGO");
        form.Requisitos = string.Concat(Enumerable.Repeat("Requisito: presentar documentación técnica vigente y cumplir los procedimientos de calidad.\n", 100));
        var created = await admin.PostAsJsonAsync("api/convocatorias", form);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var publico = factory.Client();
        var pdf = await publico.GetAsync(created.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, pdf.StatusCode);
        var bytes = await pdf.Content.ReadAsByteArrayAsync();
        Assert.True(bytes.Length > 10000);
        var output = Environment.GetEnvironmentVariable("CONVOCATORIA_PDF_QA");
        if (!string.IsNullOrEmpty(output))
            await File.WriteAllBytesAsync(Path.Combine(Path.GetDirectoryName(output)!, "convocatoria-larga-qa.pdf"), bytes);
    }

}

using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using BlazorApp2.Infrastructure;
using BlazorApp2.Security;
using TechToysDominio;

namespace BlazorApp2.Tests;

public class SegurancaTests
{
    private static readonly Assembly AppAssembly = typeof(BlazorApp2.Components.App).Assembly;

    private static IEnumerable<(Type Tipo, string Rota)> ComponentesComRota()
    {
        return AppAssembly.GetTypes()
            .Select(tipo => (Tipo: tipo, Rotas: tipo.GetCustomAttributes<RouteAttribute>(inherit: false)))
            .SelectMany(x => x.Rotas.Select(rota => (x.Tipo, Rota: rota.Template)));
    }

    [Theory]
    [InlineData("/placas/qr/{CodigoPublico}")]
    [InlineData("/placas/qr/{CodigoPublico}/editar")]
    public void PaginasPublicasDeQrCode_DevemSerAnonimas(string rota)
    {
        var tipo = ComponentesComRota().Single(x => x.Rota == rota).Tipo;

        Assert.NotNull(tipo.GetCustomAttribute<AllowAnonymousAttribute>(inherit: false));
        Assert.Null(tipo.GetCustomAttribute<AuthorizeAttribute>(inherit: false));
    }

    [Fact]
    public void TodaPaginaForaDePublico_DeveExigirAutorizacao()
    {
        var paginas = ComponentesComRota()
            .Where(x => !x.Tipo.Namespace!.Contains(".Publico", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(paginas);

        foreach (var (tipo, rota) in paginas)
        {
            var autorizacoes = tipo.GetCustomAttributes<AuthorizeAttribute>(inherit: false).ToList();
            Assert.True(autorizacoes.Count > 0, $"A rota {rota} ({tipo.FullName}) não exige autorização.");
            Assert.Null(tipo.GetCustomAttribute<AllowAnonymousAttribute>(inherit: false));
        }
    }

    [Fact]
    public void TodaPaginaPublica_DeveSerExplicitamenteAnonima()
    {
        var paginas = ComponentesComRota()
            .Where(x => x.Tipo.Namespace!.Contains(".Publico", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(paginas);

        foreach (var (tipo, rota) in paginas)
        {
            Assert.True(tipo.GetCustomAttribute<AllowAnonymousAttribute>(inherit: false) is not null,
                $"A rota pública {rota} ({tipo.FullName}) não possui [AllowAnonymous].");
            Assert.Null(tipo.GetCustomAttribute<AuthorizeAttribute>(inherit: false));
        }
    }

    [Fact]
    public void GerenciamentoDeQrCode_RestritoAAdminEGerente()
    {
        var tipo = ComponentesComRota().Single(x => x.Rota == "/placas/qr").Tipo;
        var autorizacao = AutorizacaoPorPapel(tipo);

        Assert.NotNull(autorizacao);
        Assert.Equal("admin, gerente", autorizacao!.Roles);
        Assert.DoesNotContain("funcionario", autorizacao.Roles, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("/usuarios/cadastrar", "admin")]
    [InlineData("/usuarios/editar", "admin")]
    [InlineData("/usuarios/remover", "admin")]
    [InlineData("/auditoria", "admin")]
    [InlineData("/visao-geral/limpar-banco", "admin")]
    [InlineData("/funcionarios/cadastrar", "admin, gerente")]
    [InlineData("/placas", "admin, gerente")]
    public void PaginasSensiveis_DevemTerPapelEsperado(string rota, string papeis)
    {
        var tipo = ComponentesComRota().Single(x => x.Rota == rota).Tipo;
        var autorizacao = AutorizacaoPorPapel(tipo);

        Assert.NotNull(autorizacao);
        Assert.Equal(papeis, autorizacao!.Roles);
    }

    private static AuthorizeAttribute? AutorizacaoPorPapel(Type tipo)
    {
        return tipo.GetCustomAttributes<AuthorizeAttribute>(inherit: false)
            .FirstOrDefault(a => !string.IsNullOrWhiteSpace(a.Roles));
    }

    [Theory]
    [InlineData("admin", true)]
    [InlineData("gerente", false)]
    [InlineData("funcionario", false)]
    [InlineData("", false)]
    public void CodigoDeAtivacao_SoPodeSerVistoPorAdmin(string papel, bool esperado)
    {
        var usuario = CriarUsuario(papel);

        Assert.Equal(esperado, QrCodeAccessPolicy.PodeVerCodigoAtivacao(usuario));
    }

    [Theory]
    [InlineData("admin", true)]
    [InlineData("gerente", true)]
    [InlineData("funcionario", false)]
    [InlineData("", false)]
    public void GerenciamentoDeQrCode_DeveAceitarSomenteAdminEGerente(string papel, bool esperado)
    {
        var usuario = CriarUsuario(papel);

        Assert.Equal(esperado, QrCodeAccessPolicy.PodeGerenciar(usuario));
    }

    private static ClaimsPrincipal CriarUsuario(string papel)
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, "teste") };
        if (!string.IsNullOrWhiteSpace(papel))
            claims.Add(new Claim(ClaimTypes.Role, papel));

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Teste"));
    }

    [Fact]
    public void NaoDeveExistirRotaDuplicada()
    {
        var duplicadas = ComponentesComRota()
            .GroupBy(x => x.Rota, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => $"{g.Key}: {string.Join(", ", g.Select(x => x.Tipo.FullName))}")
            .ToList();

        Assert.True(duplicadas.Count == 0, "Rotas duplicadas: " + string.Join(" | ", duplicadas));
    }

    [Fact]
    public void UnidadesDeMedida_DevemRespeitarGramaEUnidade()
    {
        Assert.Equal("g", new MateriaPrima().Unidade_Medida);
        Assert.Equal("g", new MateriaPrima(500).Unidade_Medida);
        Assert.Equal("UN", new Embalagem().Unidade_Medida);
        Assert.Equal("UN", new Produto().Unidade_Medida);
    }

    [Fact]
    public void ConversorDeConnectionString_DeveAceitarUriDoNeon()
    {
        var resultado = ConnectionStringResolver.Resolve(
            "postgresql://usuario:senha%40teste@host.neon.tech:5432/neondb?sslmode=require",
            null);

        Assert.NotNull(resultado);
        Assert.Contains("Host=host.neon.tech", resultado, StringComparison.Ordinal);
        Assert.Contains("Database=neondb", resultado, StringComparison.Ordinal);
        Assert.Contains("Username=usuario", resultado, StringComparison.Ordinal);
        Assert.Contains("Password=senha@teste", resultado, StringComparison.Ordinal);
        Assert.Contains("SSL Mode=Require", resultado, StringComparison.Ordinal);
    }

    [Fact]
    public void ConversorDeConnectionString_DeveManterFormatoNpgsql()
    {
        const string connectionString = "Host=localhost;Database=teste;Username=teste;Password=teste";

        var resultado = ConnectionStringResolver.Resolve(null, connectionString);

        Assert.Equal(connectionString, resultado);
    }

    [Fact]
    public void ConversorDeConnectionString_DeveRetornarNuloSemConfiguracao()
    {
        var resultado = ConnectionStringResolver.Resolve(null, null);

        Assert.Null(resultado);
    }
}

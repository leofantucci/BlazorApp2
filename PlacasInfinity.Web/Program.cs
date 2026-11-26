using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TechToysDominio;
using PlacasInfinity.Web.Components;
using Repositorio;
using PlacasInfinity.Web.Services;
using Microsoft.AspNetCore.Authentication;

DatabaseEnvironment.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/conta";
        options.AccessDeniedPath = "/conta";
        options.Cookie.Name = "PlacasInfinity.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// Banco de dados compartilhado com o Gestão Infinity
var connectionString = ConnectionStringResolver.Resolve(
    Environment.GetEnvironmentVariable("DATABASE_URL"),
    builder.Configuration.GetConnectionString("DefaultConnection"));

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string não encontrada. Defina a variável DATABASE_URL.");
}

builder.Services.AddDbContextFactory<MyDBContext>(
    options => options.UseNpgsql(connectionString),
    ServiceLifetime.Scoped);
builder.Services.AddScoped<LojaService>();
builder.Services.AddScoped<ContaService>();
builder.Services.AddScoped<CarrinhoState>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute(
    "/not-found",
    createScopeForStatusCodePages: true);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapPost("/conta/cadastrar", async (HttpContext http, ContaService conta) =>
{
    var form = await http.Request.ReadFormAsync();
    var nome = form["Nome"].ToString();
    var email = form["Email"].ToString();
    var senha = form["Senha"].ToString();
    var telefone = form["Telefone"].ToString();
    if (!await conta.RegistrarAsync(nome, email, senha, telefone))
        return Results.Redirect("/conta?erro=cadastro");
    var usuario = await conta.AutenticarAsync(email, senha);
    if (usuario is null) return Results.Redirect("/conta?erro=login");
    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, ContaService.CriarPrincipal(usuario),
        new Microsoft.AspNetCore.Authentication.AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
        });
    return Results.Redirect("/minha-conta");
});

app.MapPost("/conta/entrar", async (HttpContext http, ContaService conta) =>
{
    var form = await http.Request.ReadFormAsync();
    var usuario = await conta.AutenticarAsync(form["Email"].ToString(), form["Senha"].ToString());
    if (usuario is null) return Results.Redirect("/conta?erro=credenciais");
    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, ContaService.CriarPrincipal(usuario),
        new Microsoft.AspNetCore.Authentication.AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
        });
    return Results.Redirect("/minha-conta");
});

app.MapPost("/conta/sair", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

app.MapPost("/conta/perfil", async (HttpContext http, ContaService conta) =>
{
    if (http.User.Identity?.IsAuthenticated != true) return Results.Redirect("/conta");
    var rawId = http.User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!int.TryParse(rawId, out var id)) return Results.Redirect("/conta");
    var f = await http.Request.ReadFormAsync();
    var cliente = new Cliente
    {
        Nome = f["Nome"].ToString(), NomeFantasia = f["NomeFantasia"].ToString(),
        TipoCliente = Enum.TryParse<TipoCliente>(f["TipoCliente"].ToString(), out var tipo) ? tipo : TipoCliente.PF,
        CPF = f["CPF"].ToString(), CNPJ = f["CNPJ"].ToString(),
        Telefone = f["Telefone"].ToString(), Endereco = f["Endereco"].ToString(),
        Numero = f["Numero"].ToString(), Complemento = f["Complemento"].ToString(),
        Bairro = f["Bairro"].ToString(), Cidade = f["Cidade"].ToString(),
        Estado = f["Estado"].ToString(), CEP = f["CEP"].ToString()
    };
    if (string.IsNullOrWhiteSpace(cliente.Nome)) return Results.Redirect("/minha-conta?erro=nome");
    await conta.AtualizarClienteAsync(id, cliente);
    var usuarioAtualizado = await conta.ObterUsuarioAsync(id);
    if (usuarioAtualizado is not null)
        await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            ContaService.CriarPrincipal(usuarioAtualizado),
            new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
            });
    return Results.Redirect("/minha-conta?salvo=1");
}).RequireAuthorization();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

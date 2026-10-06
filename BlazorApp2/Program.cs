using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Repositorio;
using BlazorApp2.Components; // Namespace do seu App.razor

var builder = WebApplication.CreateBuilder(args);

// 1. Serviços do Blazor .NET 8
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 2. Banco de dados e Repositório
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
                       ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string não encontrada.");
}

builder.Services.AddDbContextFactory<MyDBContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<RepositorioTechToys>();

// 3. Autenticação e Autorização com Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "TechToys_Auth";
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/acesso-negado";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// 4. Inicializa tabelas
using (var scope = app.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<RepositorioTechToys>();
    repo.CriarBanco();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// 5. Middlewares de Segurança (na ordem correta antes do Blazor)
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// 6. Endpoint de Login (recebe os dados do formulário da tela /login)
app.MapPost("/api/auth/login", async (HttpContext context, RepositorioTechToys repo) =>
{
    var form = await context.Request.ReadFormAsync();
    string login = form["login"].ToString();
    string senha = form["senha"].ToString();

    var usuario = repo.ValidarUsuario(login, senha);

    if (usuario is null)
    {
        // Se errar login/senha, devolve para a tela de login com flag de erro
        context.Response.Redirect("/login?erro=invalido");
        return;
    }

    // Registra as claims contendo o papel (Role) do banco
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, usuario.Login),
        new Claim(ClaimTypes.Role, usuario.Role)
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    // Grava o cookie de autenticação na resposta HTTP
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    context.Response.Redirect("/visao-geral");
});

// 7. Endpoint de Logout
app.MapGet("/api/auth/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    context.Response.Redirect("/login");
});

// 8. Mapeamento do Blazor .NET 8
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
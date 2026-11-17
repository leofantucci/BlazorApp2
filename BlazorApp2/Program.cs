using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Antiforgery;
using System.Threading.RateLimiting;
using System.Net;
using Repositorio;
using BlazorApp2.Components;
using BlazorApp2.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. Serviços do Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.Name = "TechToys_Antiforgery";
});

// 2. Banco de dados e Repositório
var connectionString = ConnectionStringResolver.Resolve(
    Environment.GetEnvironmentVariable("DATABASE_URL"),
    builder.Configuration.GetConnectionString("DefaultConnection"));

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string não encontrada. Defina DATABASE_URL ou ConnectionStrings:DefaultConnection.");
}

// 👇 Serviços de auditoria (ordem: serviço do usuário → interceptor → DbContext)
builder.Services.AddScoped<UsuarioAtualService>();
builder.Services.AddScoped<AuditoriaInterceptor>();

builder.Services.AddDbContextFactory<MyDBContext>((sp, options) =>
{
    var interceptor = sp.GetRequiredService<AuditoriaInterceptor>();
    options.UseNpgsql(connectionString)
           .AddInterceptors(interceptor);
}, ServiceLifetime.Scoped);

builder.Services.AddScoped<RepositorioTechToys>();

// 3. Autenticação e Autorização com Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "TechToys_Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.Path = "/";
        options.Cookie.IsEssential = true;
        options.LoginPath = "/";
        options.AccessDeniedPath = "/acesso-negado";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                // Circuitos Blazor e chamadas internas não devem receber um 302 com HTML.
                if (context.Request.Path.StartsWithSegments("/_blazor"))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AcessoEquipe", policy =>
        policy.RequireRole("admin", "gerente", "funcionario"));

    options.AddPolicy("AcessoAltoEscalao", policy =>
        policy.RequireRole("admin"));

    // Toda página/endpoint privado por padrão. Páginas públicas precisam de [AllowAnonymous].
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

// Cabeçalhos encaminhados só são aceitos quando enviados por proxy explicitamente confiável.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
    // Nunca confiar indiscriminadamente em X-Forwarded-For/Proto.
    // Configure TRUSTED_PROXY_IPS com os IPs dos proxies realmente utilizados.
    var trustedProxyIps = Environment.GetEnvironmentVariable("TRUSTED_PROXY_IPS");
    if (!string.IsNullOrWhiteSpace(trustedProxyIps))
    {
        foreach (var value in trustedProxyIps.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (IPAddress.TryParse(value, out var proxyIp))
                options.KnownProxies.Add(proxyIp);
            else
                throw new InvalidOperationException($"IP inválido em TRUSTED_PROXY_IPS: {value}");
        }
    }
});

// =========================================================================
// A partir daqui, a coleção de serviços vira somente leitura (Read-Only)
// =========================================================================
var app = builder.Build();

app.UseForwardedHeaders();

// 4. Inicializa tabelas
using (var scope = app.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<RepositorioTechToys>();
    repo.CriarBanco();

    // Bootstrap seguro opcional: só cria o primeiro admin se a tabela Usuario estiver vazia.
    // Não há credenciais padrão no código-fonte.
    var adminInicial = Environment.GetEnvironmentVariable("BOOTSTRAP_ADMIN_LOGIN");
    var senhaAdminInicial = Environment.GetEnvironmentVariable("BOOTSTRAP_ADMIN_PASSWORD");
    if (!string.IsNullOrWhiteSpace(adminInicial) && !string.IsNullOrWhiteSpace(senhaAdminInicial))
        repo.CriarAdminInicialSeVazio(adminInicial, senhaAdminInicial);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// 5. Middlewares de Segurança
app.Use(async (context, next) =>
{
    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
    context.Response.Headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.TryAdd("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
    await next();
});

app.UseAuthentication();
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
        context.Response.Headers["Pragma"] = "no-cache";
    }
    await next();
});
app.UseRateLimiter();
app.UseAuthorization();
app.UseAntiforgery();

// 6. Endpoint de Login
app.MapPost("/api/auth/login", async (HttpContext context, RepositorioTechToys repo, IAntiforgery antiforgery) =>
{
    context.Response.Headers["Cache-Control"] = "no-store";

    if (!await antiforgery.IsRequestValidAsync(context))
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }

    var form = await context.Request.ReadFormAsync();
    string login = form["login"].ToString().Trim();
    string senha = form["senha"].ToString();

    if (login.Length > 100 || senha.Length > 1024)
    {
        context.Response.Redirect("/?erro=invalido");
        return;
    }

    var usuario = repo.ValidarUsuario(login, senha);

    if (usuario is null)
    {
        // Mensagem genérica: não revela se o login existe.
        context.Response.Redirect("/?erro=invalido");
        return;
    }

    // O evento é gravado somente depois da validação correta das credenciais
    // e antes da emissão do cookie de autenticação.
    repo.RegistrarLogin(usuario.Id, usuario.Login);

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, usuario.Login),
        new Claim(ClaimTypes.Role, usuario.Role)
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await context.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        principal,
        new AuthenticationProperties { IsPersistent = false, AllowRefresh = true });

    context.Response.Redirect("/inicio");
}).RequireRateLimiting("login").AllowAnonymous();

// 7. Endpoint de Logout (POST + validação antifalsificação explícita)
app.MapPost("/api/auth/logout", async (HttpContext context, IAntiforgery antiforgery) =>
{
    if (!await antiforgery.IsRequestValidAsync(context))
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }

    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    context.Response.Redirect("/");
});

// Health check público para o Render e para monitoramento.
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

// 8. Mapeamento do Blazor
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AllowAnonymous();

app.Run();

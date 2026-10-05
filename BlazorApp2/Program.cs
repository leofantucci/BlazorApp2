using Microsoft.EntityFrameworkCore;
using Repositorio;
using BlazorApp2.Components; // Ajuste para o namespace do seu App.razor

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

var app = builder.Build();

// 3. Inicializa tabelas
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
app.UseAntiforgery();

// 4. Mapeamento correto do Blazor .NET 8
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
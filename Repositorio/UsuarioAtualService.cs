using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Repositorio;

public sealed class UsuarioAtualService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AuthenticationStateProvider? _authenticationStateProvider;

    public UsuarioAtualService(
        IHttpContextAccessor httpContextAccessor,
        IServiceProvider services)
    {
        _httpContextAccessor = httpContextAccessor;
        _authenticationStateProvider = services.GetService<AuthenticationStateProvider>();
    }

    public async Task<string> ObterPapelAsync()
    {
        var httpUser = _httpContextAccessor.HttpContext?.User;
        if (httpUser?.Identity?.IsAuthenticated == true)
            return (httpUser.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty).Trim().ToLowerInvariant();

        try
        {
            if (_authenticationStateProvider == null) return string.Empty;
            var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = state.User;
            if (user.Identity?.IsAuthenticated == true)
                return (user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty).Trim().ToLowerInvariant();
        }
        catch
        {
            // Fora de uma requisição/circuito autenticado, não há papel autorizado.
        }

        return string.Empty;
    }

    public async Task<string> ObterUsuarioAsync()
    {
        var httpUser = _httpContextAccessor.HttpContext?.User;
        if (httpUser?.Identity?.IsAuthenticated == true)
            return httpUser.Identity.Name ?? "Anônimo";

        try
        {
            if (_authenticationStateProvider == null) return "Sistema";
            var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = state.User;
            if (user.Identity?.IsAuthenticated == true)
                return user.Identity.Name ?? "Anônimo";
        }
        catch
        {
            // Em tarefas de inicialização fora de uma requisição/circuito não há usuário.
        }

        return "Sistema";
    }
}

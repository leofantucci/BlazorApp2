using System.Security.Claims;

namespace BlazorApp2.Security;

/// <summary>
/// Regras de acesso da área de QR Codes que precisam ser verificadas no servidor,
/// além das restrições declaradas nas páginas.
/// </summary>
public static class QrCodeAccessPolicy
{
    public static bool PodeGerenciar(ClaimsPrincipal usuario)
    {
        return usuario.IsInRole("admin") || usuario.IsInRole("gerente");
    }

    public static bool PodeVerCodigoAtivacao(ClaimsPrincipal usuario)
    {
        // O código de ativação é equivalente a uma credencial da placa.
        return usuario.IsInRole("admin");
    }
}

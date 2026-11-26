namespace BlazorApp2.Components.Placas;

public static class PlacaNfcLinks
{
    public static bool TentarNormalizarLink(string? valor, out string normalizado)
    {
        normalizado = string.Empty;
        if (string.IsNullOrWhiteSpace(valor)) return false;

        if (!Uri.TryCreate(valor.Trim(), UriKind.Absolute, out var uri)) return false;
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) return false;

        normalizado = uri.ToString();
        return true;
    }

    public static bool LinkValido(string? valor) => TentarNormalizarLink(valor, out _);

    public static bool TentarNormalizarIcone(string? valor, out string normalizado)
    {
        normalizado = string.Empty;
        if (string.IsNullOrWhiteSpace(valor)) return true;

        var texto = valor.Trim();

        // Caminhos locais servidos pelo wwwroot.
        if (texto.StartsWith("/", StringComparison.Ordinal) ||
            texto.StartsWith("Media/", StringComparison.OrdinalIgnoreCase))
        {
            normalizado = texto;
            return true;
        }

        // Ícones externos.
        if (!Uri.TryCreate(texto, UriKind.Absolute, out var uri)) return false;
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) return false;

        normalizado = uri.ToString();
        return true;
    }
}

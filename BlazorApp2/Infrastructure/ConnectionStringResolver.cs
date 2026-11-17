using Npgsql;

namespace BlazorApp2.Infrastructure;

/// <summary>
/// Normaliza a configuração de banco aceitando tanto a URI do Neon
/// (postgresql://...) quanto uma connection string nativa do Npgsql.
/// </summary>
public static class ConnectionStringResolver
{
    public static string? Resolve(string? databaseUrl, string? defaultConnection)
    {
        var bruta = string.IsNullOrWhiteSpace(databaseUrl) ? defaultConnection : databaseUrl;
        if (string.IsNullOrWhiteSpace(bruta)) return null;

        const string prefixoPostgres = "postgres://";
        const string prefixoPostgresql = "postgresql://";

        if (!bruta.StartsWith(prefixoPostgres, StringComparison.OrdinalIgnoreCase) &&
            !bruta.StartsWith(prefixoPostgresql, StringComparison.OrdinalIgnoreCase))
            return bruta;

        var uri = new Uri(bruta);
        var usuarioSenha = uri.UserInfo.Split(':', 2);
        var banco = uri.AbsolutePath.Trim('/');

        var builderConexao = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = string.IsNullOrWhiteSpace(banco) ? "neondb" : Uri.UnescapeDataString(banco),
            Username = usuarioSenha.Length > 0 ? Uri.UnescapeDataString(usuarioSenha[0]) : string.Empty,
            Password = usuarioSenha.Length > 1 ? Uri.UnescapeDataString(usuarioSenha[1]) : string.Empty,
            SslMode = SslMode.Require
        };

        return builderConexao.ConnectionString;
    }
}

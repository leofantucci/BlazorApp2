using Npgsql;

namespace Repositorio;

public static class ConnectionStringResolver
{
    public static string? Resolve(string? databaseUrl, string? defaultConnection = null)
    {
        var value = string.IsNullOrWhiteSpace(databaseUrl) ? defaultConnection : databaseUrl;
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (!value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            return value;

        var uri = new Uri(value);
        var credentials = uri.UserInfo.Split(':', 2);
        return new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = string.IsNullOrWhiteSpace(uri.AbsolutePath.Trim('/')) ? "neondb" : Uri.UnescapeDataString(uri.AbsolutePath.Trim('/')),
            Username = credentials.Length > 0 ? Uri.UnescapeDataString(credentials[0]) : string.Empty,
            Password = credentials.Length > 1 ? Uri.UnescapeDataString(credentials[1]) : string.Empty,
            SslMode = SslMode.Require
        }.ConnectionString;
    }
}

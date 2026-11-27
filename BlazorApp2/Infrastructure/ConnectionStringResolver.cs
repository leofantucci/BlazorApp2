namespace BlazorApp2.Infrastructure;

[Obsolete("Use Repositorio.ConnectionStringResolver.")]
public static class ConnectionStringResolver
{
    public static string? Resolve(string? databaseUrl, string? defaultConnection) =>
        Repositorio.ConnectionStringResolver.Resolve(databaseUrl, defaultConnection);
}

namespace TechToysDominio;

public class Usuario
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    // Mantido para compatibilidade com contas legadas; novas contas usam somente SenhaHash.
    public string Senha { get; set; } = string.Empty;
    public string? SenhaHash { get; set; }
    public string Role { get; set; } = string.Empty;
}

public class UsuarioComercial : Usuario
{
    public int? ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
}

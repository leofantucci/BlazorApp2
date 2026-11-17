namespace TechToysDominio;

public class Usuario
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;

    // Senha é mantida temporariamente para migrar contas antigas em texto puro/SHA-256.
    // Novas senhas são armazenadas somente em SenhaHash.
    public string Senha { get; set; } = string.Empty;
    public string? SenhaHash { get; set; }

    public string Role { get; set; } = string.Empty;
}

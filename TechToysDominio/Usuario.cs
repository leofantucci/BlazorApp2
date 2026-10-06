namespace TechToysDominio;

public class Usuario
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // Ex: "Funcionario", "Gerente"
}
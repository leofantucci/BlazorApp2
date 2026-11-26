namespace TechToysDominio;

public enum StatusQRCode
{
    Pendente = 0,
    Ativo = 1,
    Bloqueado = 2,
    Cancelado = 3
}

public class QRCode
{
    public int Id { get; set; }

    // Identificador aleatório usado na URL pública. Não é o ID sequencial do banco.
    public string CodigoPublico { get; set; } = string.Empty;

    // Hashes: nunca armazenar código de ativação ou senha em texto puro.
    public string? CodigoAtivacaoHash { get; set; }
    public string? SenhaHash { get; set; }

    // Cópia em texto puro do código de ativação, exibida somente para administradores
    // enquanto o QR Code não foi ativado. É apagada na ativação, junto com o hash.
    public string? CodigoAtivacao { get; set; }

    public StatusQRCode Status { get; set; } = StatusQRCode.Pendente;
    public string? Nome { get; set; }
    public string? LinkDestino { get; set; }

    public DateTime DataCriacaoUtc { get; set; } = DateTime.UtcNow;
    public DateTime? DataAtivacaoUtc { get; set; }
    public DateTime? DataAtualizacaoUtc { get; set; }
    public DateTime? UltimoAcessoUtc { get; set; }

    public int TotalAcessos { get; set; }
    public int TentativasSenha { get; set; }
    public DateTime? BloqueadoAteUtc { get; set; }
}

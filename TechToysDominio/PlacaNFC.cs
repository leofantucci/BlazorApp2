namespace TechToysDominio;

public enum StatusPlacaNFC
{
    Pendente = 0,
    Ativo = 1,
    Bloqueado = 2,
    Cancelado = 3
}

public class PlacaNFC
{
    public int Id { get; set; }

    // Identificador aleatório usado na URL pública da tag NFC. Não é o ID sequencial do banco.
    public string CodigoPublico { get; set; } = string.Empty;

    // Hashes: nunca armazenar código de ativação ou senha em texto puro.
    public string? CodigoAtivacaoHash { get; set; }
    public string? SenhaHash { get; set; }

    // Cópia em texto puro do código de ativação, exibida somente para administradores
    // enquanto a placa não foi ativada. É apagada na ativação, junto com o hash.
    public string? CodigoAtivacao { get; set; }

    public StatusPlacaNFC Status { get; set; } = StatusPlacaNFC.Pendente;
    public string? Nome { get; set; }

    // Vínculos opcionais: a placa pode ser criada em estoque e vinculada depois da venda.
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public int? ProdutoId { get; set; }
    public Produto? Produto { get; set; }

    // QR Code impresso opcional, vinculado a um QR Code dinâmico já existente.
    public int? QRCodeId { get; set; }
    public QRCode? QRCode { get; set; }

    // Vínculo opcional com a unidade comercial que originou esta placa.
    public int? VendaComercialItemId { get; set; }
    public VendaComercialItem? VendaComercialItem { get; set; }

    public DateTime DataCriacaoUtc { get; set; } = DateTime.UtcNow;
    public DateTime? DataAtivacaoUtc { get; set; }
    public DateTime? DataAtualizacaoUtc { get; set; }
    public DateTime? UltimoAcessoUtc { get; set; }

    public int TotalAcessos { get; set; }
    public int TentativasSenha { get; set; }
    public DateTime? BloqueadoAteUtc { get; set; }

    public List<PlacaNFCCard> Cards { get; set; } = new();
}

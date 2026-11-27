using System.ComponentModel.DataAnnotations;

namespace TechToysDominio;

public enum StatusPagamento
{
    Pendente = 0,
    Aprovado = 1,
    Recusado = 2,
    Estornado = 3,
    Cancelado = 4
}

public class Pagamento
{
    public int Id { get; set; }
    public int VendaComercialId { get; set; }
    public VendaComercial VendaComercial { get; set; } = null!;
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public DateTime DataHora { get; set; } = DateTime.UtcNow;
    public decimal Valor { get; set; }
    public StatusPagamento Status { get; set; } = StatusPagamento.Pendente;
    [StringLength(40)] public string Metodo { get; set; } = "A definir";
    [StringLength(80)] public string Provedor { get; set; } = "Manual";
    [StringLength(150)] public string? TransacaoExternaId { get; set; }
    [StringLength(80)] public string? StatusProvedor { get; set; }
    [StringLength(12000)] public string? PixQrCodeBase64 { get; set; }
    [StringLength(2000)] public string? PixCopiaECola { get; set; }
    public DateTime? PixExpiracaoUtc { get; set; }
    [StringLength(2000)] public string? PayloadRetorno { get; set; }
    public DateTime? DataConfirmacaoUtc { get; set; }
}

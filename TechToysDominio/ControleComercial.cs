using System.ComponentModel.DataAnnotations;

namespace TechToysDominio;

public class ControleComercial
{
    // Mantido para compatibilidade com código legado que utilizava o enum aninhado.
    public enum StatusVenda
    {
        AGUARDANDO = 0,
        PAGO = 1,
        PRODUZINDO = 2,
        PRONTO = 3,
        ENVIADO = 4,
        RECEBIDO = 5
    }

    public int Id { get; set; }
    public int VendaComercialId { get; set; }
    public VendaComercial Venda { get; set; } = null!;
    public StatusVendaComercial Status { get; set; }
    public DateTime DataHoraUtc { get; set; } = DateTime.UtcNow;
    [StringLength(500)] public string? Observacao { get; set; }
    [StringLength(100)] public string? Responsavel { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace TechToysDominio;

public enum StatusVendaComercial
{
    AguardandoPagamento = 0,
    Pago = 1,
    EmProducao = 2,
    Pronto = 3,
    Enviado = 4,
    Recebido = 5,
    Cancelado = 6
}

public class VendaComercial
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public DateTime Data_Hora { get; set; } = DateTime.UtcNow;
    public StatusVendaComercial Status { get; set; } = StatusVendaComercial.AguardandoPagamento;
    [StringLength(200)] public string? EnderecoEntrega { get; set; }
    [StringLength(100)] public string? CidadeEntrega { get; set; }
    [StringLength(2)] public string? EstadoEntrega { get; set; }
    [StringLength(20)] public string? CEPEntrega { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Frete { get; set; }
    public decimal Desconto { get; set; }
    public decimal Total { get; set; }
    [StringLength(1000)] public string? Observacoes { get; set; }
    public List<VendaComercialItem> ItensVenda { get; set; } = new();
    public List<Pagamento> Pagamentos { get; set; } = new();
    public List<ControleComercial> Historico { get; set; } = new();

    public double Calcular_Lucro_Bruto() => Subtotal > 0 ? (double)Subtotal : (ItensVenda?.Sum(i => i.Calcular_Subtotal()) ?? 0);
    public double Calcular_Custo_Producao() => ItensVenda?.Sum(i => i.Calcular_Custo_Producao()) ?? 0;
    public double Calcular_Gasto_Total() => Calcular_Custo_Producao();
    public double Calcular_Lucro_Liquido() => Calcular_Lucro_Bruto() - Calcular_Gasto_Total();
}

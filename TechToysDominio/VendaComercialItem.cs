namespace TechToysDominio;

public class VendaComercialItem
{
    public int Id { get; set; }
    public int VendaComercialId { get; set; }
    public VendaComercial VendaComercial { get; set; } = null!;
    public int ProdutoComercialId { get; set; }
    public ProdutoComercial ProdutoComercial { get; set; } = null!;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public int? PagamentoId { get; set; }
    public Pagamento? Pagamento { get; set; }

    public double Calcular_Subtotal() => (double)(Subtotal > 0 ? Subtotal : PrecoUnitario * Quantidade);
    public double Calcular_Custo_Producao() => ProdutoComercial?.ProdutoBase == null ? 0 : ProdutoComercial.ProdutoBase.Calcular_Custo_Total() * Quantidade;
}

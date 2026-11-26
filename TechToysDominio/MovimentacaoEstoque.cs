namespace TechToysDominio;

/// <summary>Registro permanente de cada entrada ou saída de estoque.</summary>
public class MovimentacaoEstoque
{
    public int Id { get; set; }
    public string TipoItem { get; set; } = string.Empty;
    public int ItemId { get; set; }
    public string NomeItem { get; set; } = string.Empty;
    public string Operacao { get; set; } = string.Empty; // Entrada ou Saída
    public decimal Quantidade { get; set; }
    public string Unidade { get; set; } = string.Empty;
    public string Origem { get; set; } = string.Empty; // Produção, Venda, Ajuste manual, Cancelamento, Edição
    public int? ReferenciaId { get; set; }
    public DateTime DataHora { get; set; } = DateTime.UtcNow;
    public string Usuario { get; set; } = "Sistema";
}

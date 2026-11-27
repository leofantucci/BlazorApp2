namespace TechToysDominio;

public class ProdutoPeca
{
    public int Id { get; set; }
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;
    public int PecaId { get; set; }
    public Peca Peca { get; set; } = null!;
    public int Quantidade { get; set; }
}

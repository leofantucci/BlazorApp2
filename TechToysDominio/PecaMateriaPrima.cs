namespace TechToysDominio;

/// <summary>Quantidade, em gramas, de uma matéria-prima utilizada por uma peça.</summary>
public class PecaMateriaPrima
{
    public int Id { get; set; }
    public int PecaId { get; set; }
    public Peca Peca { get; set; } = null!;
    public int MateriaPrimaId { get; set; }
    public MateriaPrima MateriaPrima { get; set; } = null!;
    public double Peso { get; set; }
}

namespace TechToysDominio;

public class Estoque
{
    public int Id { get; set; }
    public string TipoItem { get; set; } = string.Empty;
    public int ItemId { get; set; }
    public decimal Quantidade { get; set; }
    public string Unidade_Medida { get; set; } = string.Empty;

    public bool Estoque_Adicionar(decimal quantidade, out string mensagem)
    {
        if (quantidade <= 0 || quantidade == decimal.MaxValue)
        {
            mensagem = "Informe uma quantidade válida e maior que zero.";
            return false;
        }
        Quantidade += quantidade;
        mensagem = "Entrada de estoque realizada com sucesso.";
        return true;
    }

    public bool Estoque_Remover(decimal quantidade, out string mensagem)
    {
        if (quantidade <= 0)
        {
            mensagem = "Informe uma quantidade válida e maior que zero.";
            return false;
        }
        if (Quantidade < quantidade)
        {
            mensagem = $"Estoque insuficiente. Disponível: {Quantidade}.";
            return false;
        }
        Quantidade -= quantidade;
        mensagem = "Saída de estoque realizada com sucesso.";
        return true;
    }
}

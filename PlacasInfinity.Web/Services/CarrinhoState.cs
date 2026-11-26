namespace PlacasInfinity.Web.Services;

/// <summary>
/// Carrinho em memória por sessão/circuito Blazor. O checkout sempre revalida
/// disponibilidade e preço diretamente no banco antes de registrar o pedido.
/// </summary>
public sealed class CarrinhoState
{
    private readonly Dictionary<int, int> _quantidades = new();

    public event Action? Alterado;
    public int TotalItens => _quantidades.Values.Sum();
    public IReadOnlyDictionary<int, int> Itens => _quantidades;

    public int Quantidade(int produtoId) =>
        _quantidades.TryGetValue(produtoId, out var quantidade) ? quantidade : 0;

    public void Adicionar(int produtoId)
    {
        if (produtoId <= 0) return;
        var atual = Quantidade(produtoId);
        if (atual >= 50) return;
        _quantidades[produtoId] = atual + 1;
        Alterado?.Invoke();
    }

    public void Remover(int produtoId)
    {
        if (!_quantidades.TryGetValue(produtoId, out var atual)) return;
        if (atual <= 1) _quantidades.Remove(produtoId);
        else _quantidades[produtoId] = atual - 1;
        Alterado?.Invoke();
    }

    public void Excluir(int produtoId)
    {
        if (_quantidades.Remove(produtoId)) Alterado?.Invoke();
    }

    public void Limpar()
    {
        if (_quantidades.Count == 0) return;
        _quantidades.Clear();
        Alterado?.Invoke();
    }
}

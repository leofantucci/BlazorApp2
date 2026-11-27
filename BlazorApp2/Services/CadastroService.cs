using System.ComponentModel.DataAnnotations;
using Repositorio;
using TechToysDominio;

namespace BlazorApp2.Services;

public class CadastroService
{
    private readonly RepositorioTechToys _repositorio;

    public CadastroService(RepositorioTechToys repositorio)
    {
        _repositorio = repositorio;
    }

    public Cliente CadastrarCliente(string nome, string? nomeFantasia, TipoCliente tipoCliente,
        string? cpf, string? cnpj, string? endereco, string? telefone, string? email)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Por favor, preencha o Nome do cliente.");
        if (nome.Trim().Length > 100)
            throw new ArgumentException("O nome do cliente deve ter até 100 caracteres.");
        if (!string.IsNullOrWhiteSpace(nomeFantasia) && nomeFantasia.Trim().Length > 100)
            throw new ArgumentException("O nome fantasia deve ter até 100 caracteres.");
        if (!Enum.IsDefined(typeof(TipoCliente), tipoCliente))
            throw new ArgumentException("Selecione um tipo de cliente válido.");
        if (!string.IsNullOrWhiteSpace(email) && !new EmailAddressAttribute().IsValid(email.Trim()))
            throw new ArgumentException("Informe um e-mail válido.");
        if (tipoCliente == TipoCliente.PF && !string.IsNullOrWhiteSpace(cpf) && cpf.Trim().Length > 14)
            throw new ArgumentException("O CPF deve ter até 14 caracteres.");
        if (tipoCliente == TipoCliente.PJ && !string.IsNullOrWhiteSpace(cnpj) && cnpj.Trim().Length > 18)
            throw new ArgumentException("O CNPJ deve ter até 18 caracteres.");
        if (!string.IsNullOrWhiteSpace(endereco) && endereco.Trim().Length > 200)
            throw new ArgumentException("O endereço deve ter até 200 caracteres.");
        if (!string.IsNullOrWhiteSpace(telefone) && telefone.Trim().Length > 20)
            throw new ArgumentException("O telefone deve ter até 20 caracteres.");
        if (!string.IsNullOrWhiteSpace(email) && email.Trim().Length > 150)
            throw new ArgumentException("O e-mail deve ter até 150 caracteres.");

        return _repositorio.CadastrarCliente(nome.Trim(), nomeFantasia, tipoCliente,
            tipoCliente == TipoCliente.PF ? cpf : null,
            tipoCliente == TipoCliente.PJ ? cnpj : null, endereco, telefone, email);
    }

    public Peca CadastrarPeca(string nome, List<MateriaPrimaPesoInput> materias,
        int idImpressora, double peso, double tempo, double largura, double comprimento,
        double altura, int estoqueInicial)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length > 120)
            throw new ArgumentException("Informe o nome da peça (até 120 caracteres).");
        if (!double.IsFinite(peso) || peso <= 0 || peso > 100000)
            throw new ArgumentException("Informe um peso total de impressão maior que zero.");
        if (!double.IsFinite(tempo) || tempo <= 0 || tempo > 1000000)
            throw new ArgumentException("Informe um tempo de impressão maior que zero.");
        if (materias == null || materias.Count == 0 || materias.Count > 50 ||
            materias.Any(x => x.MateriaPrimaId <= 0 || !double.IsFinite(x.Peso) || x.Peso <= 0 || x.Peso > 100000))
            throw new ArgumentException("Selecione matérias-primas válidas e informe o peso utilizado em cada linha.");
        if (materias.Select(x => x.MateriaPrimaId).Distinct().Count() != materias.Count)
            throw new ArgumentException("Não é permitido selecionar a mesma matéria-prima mais de uma vez. Some o peso em uma única linha.");
        if (Math.Abs(materias.Sum(x => x.Peso) - peso) > 0.001)
            throw new ArgumentException($"A soma dos pesos das matérias-primas ({materias.Sum(x => x.Peso):N3} g) deve ser igual ao peso total ({peso:N3} g).");
        if (!double.IsFinite(largura) || largura <= 0 || largura > 100000 ||
            !double.IsFinite(comprimento) || comprimento <= 0 || comprimento > 100000 ||
            !double.IsFinite(altura) || altura <= 0 || altura > 100000)
            throw new ArgumentException("Informe dimensões válidas para a peça.");
        if (estoqueInicial < 0 || estoqueInicial > 10000000)
            throw new ArgumentException("O estoque inicial deve estar entre 0 e 10.000.000 unidades.");

        var impressoras = _repositorio.ListarTodos<Impressora>();
        var impressora = impressoras.FirstOrDefault(x => x.Id == idImpressora)
            ?? throw new ArgumentException("Selecione uma impressora válida.");
        var idsMaterias = materias.Select(x => x.MateriaPrimaId).ToHashSet();
        var materiasExistentes = _repositorio.ListarTodos<MateriaPrima>().Where(x => idsMaterias.Contains(x.Id)).Select(x => x.Id).ToHashSet();
        if (!idsMaterias.SetEquals(materiasExistentes))
            throw new ArgumentException("Uma ou mais matérias-primas selecionadas são inválidas.");

        return _repositorio.CadastrarPeca(nome.Trim(), materias, impressora, peso, tempo,
            largura, comprimento, altura, estoqueInicial);
    }

    public Produto CadastrarProduto(string nome, double valorVenda,
        List<ItemQuantidadeInput> composicao, int estoqueInicial)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length > 100)
            throw new ArgumentException("Informe o nome do produto (até 100 caracteres).");
        if (!double.IsFinite(valorVenda) || valorVenda < 0)
            throw new ArgumentException("Informe um valor de venda válido e não negativo.");
        if (estoqueInicial < 0 || estoqueInicial > 10000000)
            throw new ArgumentException("O estoque inicial deve estar entre 0 e 10.000.000 unidades.");
        if (composicao == null || composicao.Count == 0 ||
            composicao.Any(x => x.ItemId <= 0 || x.Quantidade <= 0))
            throw new ArgumentException("Adicione pelo menos uma peça e informe uma quantidade válida em cada linha.");
        if (composicao.GroupBy(x => x.ItemId).Any(g => g.Count() > 1))
            throw new ArgumentException("A mesma peça foi selecionada mais de uma vez. Ajuste a quantidade na própria linha.");

        return _repositorio.CadastrarProduto(nome.Trim(), valorVenda, composicao, estoqueInicial);
    }
}

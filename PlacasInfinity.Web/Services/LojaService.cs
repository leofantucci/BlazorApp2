using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Repositorio;
using TechToysDominio;

namespace PlacasInfinity.Web.Services;

public sealed class ItemCarrinhoInput
{
    public int ProdutoComercialId { get; set; }
    public int Quantidade { get; set; } = 1;
}

public sealed class CheckoutInput
{
    [Required, StringLength(100)] public string Nome { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(150)] public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    [Required, StringLength(200)] public string Endereco { get; set; } = string.Empty;
    [Required, StringLength(100)] public string Cidade { get; set; } = string.Empty;
    [Required, StringLength(2, MinimumLength = 2)] public string Estado { get; set; } = string.Empty;
    [Required, StringLength(20)] public string CEP { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
}

public sealed class LojaService(IDbContextFactory<MyDBContext> contextFactory)
{
    public async Task<List<ProdutoComercial>> ListarProdutosAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.ProdutoComercial.AsNoTracking()
            .Where(p => p.Disponivel)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProdutoComercial?> ObterProdutoAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.ProdutoComercial.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.Disponivel, cancellationToken);
    }

    public async Task<CheckoutInput?> ObterCheckoutClienteAsync(int usuarioComercialId, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        var c = await db.Set<UsuarioComercial>().Where(u => u.Id == usuarioComercialId)
            .Select(u => u.Cliente).AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        if (c is null) return null;
        return new CheckoutInput
        {
            Nome = c.Nome, Email = c.Email ?? string.Empty, Telefone = c.Telefone ?? string.Empty,
            Endereco = string.Join(", ", new[] { c.Endereco, c.Numero, c.Complemento, c.Bairro }.Where(x => !string.IsNullOrWhiteSpace(x))),
            Cidade = c.Cidade ?? string.Empty, Estado = c.Estado ?? string.Empty, CEP = c.CEP ?? string.Empty
        };
    }

    public async Task<int> CriarVendaAsync(CheckoutInput dados, IReadOnlyCollection<ItemCarrinhoInput> itens,
        int? usuarioComercialId = null, CancellationToken cancellationToken = default)
    {
        if (itens.Count == 0) throw new InvalidOperationException("Adicione ao menos um produto.");
        if (string.IsNullOrWhiteSpace(dados.Nome) || string.IsNullOrWhiteSpace(dados.Email))
            throw new InvalidOperationException("Nome e e-mail são obrigatórios.");

        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var agrupados = itens.GroupBy(i => i.ProdutoComercialId)
            .Select(g => new ItemCarrinhoInput { ProdutoComercialId = g.Key, Quantidade = g.Sum(x => x.Quantidade) })
            .ToList();
        if (agrupados.Any(i => i.Quantidade is < 1 or > 50))
            throw new InvalidOperationException("A quantidade por produto deve estar entre 1 e 50.");

        var ids = agrupados.Select(i => i.ProdutoComercialId).ToList();
        var produtos = await db.ProdutoComercial
            .Where(p => ids.Contains(p.Id) && p.Disponivel)
            .ToDictionaryAsync(p => p.Id, cancellationToken);
        if (produtos.Count != ids.Count)
            throw new InvalidOperationException("Um ou mais produtos não estão mais disponíveis.");

        var email = dados.Email.Trim().ToLowerInvariant();
        Cliente? cliente = null;
        if (usuarioComercialId.HasValue)
        {
            var conta = await db.Set<UsuarioComercial>().Include(u => u.Cliente)
                .FirstOrDefaultAsync(u => u.Id == usuarioComercialId.Value, cancellationToken)
                ?? throw new InvalidOperationException("Sua sessão expirou. Entre novamente.");
            cliente = conta.Cliente;
            if (!string.Equals(cliente.Email, email, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("O e-mail do pedido deve corresponder ao e-mail da sua conta.");
            cliente.Nome = dados.Nome.Trim();
            cliente.Telefone = string.IsNullOrWhiteSpace(dados.Telefone) ? cliente.Telefone : dados.Telefone.Trim();
        }
        else
        {
            cliente = await db.Cliente.FirstOrDefaultAsync(c => c.Email != null && c.Email.ToLower() == email, cancellationToken);
            // Um checkout anônimo nunca deve anexar um pedido a uma conta já registrada.
            if (cliente is not null && await db.Set<UsuarioComercial>()
                    .AnyAsync(u => u.ClienteId == cliente.Id, cancellationToken))
                cliente = null;
            if (cliente is null)
            {
                cliente = new Cliente
                {
                    Nome = dados.Nome.Trim(), Email = email,
                    Telefone = string.IsNullOrWhiteSpace(dados.Telefone) ? null : dados.Telefone.Trim(),
                    TipoCliente = TipoCliente.PF
                };
                db.Cliente.Add(cliente);
            }
            else
            {
                cliente.Nome = dados.Nome.Trim();
                if (!string.IsNullOrWhiteSpace(dados.Telefone)) cliente.Telefone = dados.Telefone.Trim();
            }
        }

        var venda = new VendaComercial
        {
            Cliente = cliente,
            Data_Hora = DateTime.UtcNow,
            Status = StatusVendaComercial.AguardandoPagamento,
            EnderecoEntrega = dados.Endereco.Trim(),
            CidadeEntrega = dados.Cidade.Trim(),
            EstadoEntrega = dados.Estado.Trim().ToUpperInvariant(),
            CEPEntrega = dados.CEP.Trim(),
            Observacoes = string.IsNullOrWhiteSpace(dados.Observacoes) ? null : dados.Observacoes.Trim()
        };

        foreach (var item in agrupados)
        {
            var produto = produtos[item.ProdutoComercialId];
            if (produto.Preco <= 0) throw new InvalidOperationException($"O produto {produto.Nome} possui preço inválido.");
            var subtotal = produto.Preco * item.Quantidade;
            venda.ItensVenda.Add(new VendaComercialItem
            {
                ProdutoComercial = produto,
                Quantidade = item.Quantidade,
                PrecoUnitario = produto.Preco,
                Subtotal = subtotal
            });
            venda.Subtotal += subtotal;
        }

        venda.Total = venda.Subtotal + venda.Frete - venda.Desconto;
        venda.Historico.Add(new ControleComercial
        {
            Status = StatusVendaComercial.AguardandoPagamento,
            DataHoraUtc = DateTime.UtcNow,
            Observacao = "Pedido recebido pela loja online."
        });
        venda.Pagamentos.Add(new Pagamento
        {
            Cliente = cliente,
            Valor = venda.Total,
            Status = StatusPagamento.Pendente,
            Metodo = "A definir",
            Provedor = "Pendente de integração"
        });

        db.VendaComercial.Add(venda);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return venda.Id;
    }
}

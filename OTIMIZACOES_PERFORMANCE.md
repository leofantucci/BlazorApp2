# Otimizações de performance aplicadas

As alterações foram limitadas às telas de Visão Geral e Estoque e ao repositório, preservando as rotas, entidades, migrations, autenticação, regras de negócio e estrutura visual.

## Visão Geral
- As contagens agora usam `COUNT` no PostgreSQL por meio de `RepositorioTechToys.Contar<T>()`.
- Os resultados são carregados uma vez em `OnInitialized`, em vez de executar listagens completas durante a renderização dos cards.
- Isso evita carregar entidades e relacionamentos apenas para obter a quantidade de registros.

## Estoque
- A tela usa `ListarItensBasicos<T>()`, que projeta somente ID, nome e unidade de medida para os quatro tipos de item.
- Os saldos dos quatro tipos são carregados juntos por `ObterSaldosEstoque(...)`, em uma consulta à tabela Estoque.
- Isso elimina a consulta individual por item que existia na inicialização da página e evita os `Include` profundos desnecessários nessa tela.
- Após uma movimentação, o saldo selecionado continua sendo atualizado pelo método existente `ObterQuantidadeEstoque<T>()`.

## Validação
O ambiente usado para preparar este pacote não possui o SDK do .NET instalado. Portanto, não foi possível executar `dotnet restore`, `dotnet build` ou os testes automatizados aqui. Antes de publicar, execute na pasta que contém `BlazorApp2.sln`:

```bash
dotnet restore BlazorApp2.sln
dotnet build BlazorApp2.sln --no-restore
dotnet test BlazorApp2.sln --no-build
```

Não foram alteradas migrations nem é necessário executar uma migration nova para estas otimizações.

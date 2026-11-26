# Vista para o mar contando dinheiro na rede
## Gestão Infinity + Placas Infinity

A solução mantém o Gestão Infinity e adiciona a loja pública `PlacasInfinity.Web`, compartilhando `TechToysDominio`, `Repositorio`, `MyDBContext` e a tabela `Cliente`.

## Projetos

- `BlazorApp2`: gestão administrativa, produção, estoque, clientes e administração de placas NFC/QR Codes.
- `PlacasInfinity.Web`: catálogo público, formulário de compra e consulta de confirmação do pedido.
- `TechToysDominio`: entidades compartilhadas.
- `Repositorio`: persistência PostgreSQL e migrations compartilhadas.
- `BlazorApp2.Tests`: testes existentes.

A pasta antiga `PlacasInfinity/` foi preservada para não remover arquivos do ZIP original. O projeto que faz parte da solução e deve ser executado é `PlacasInfinity.Web/`.

## Configuração

Defina `DATABASE_URL` nos dois ambientes. São aceitos tanto connection strings do Npgsql (`Host=...;Database=...`) quanto URI PostgreSQL (`postgresql://...`). A normalização fica centralizada em `Repositorio/ConnectionStringResolver.cs`.

Exemplo:

```bash
export DATABASE_URL='Host=SEU_HOST;Port=5432;Database=neondb;Username=SEU_USUARIO;Password=SUA_SENHA;SSL Mode=Require'
```

## Migration comercial

Foi adicionada a migration `20261126010000_ComercialOnline`, que cria as tabelas de produto comercial, venda comercial, itens, pagamentos e histórico, além de adicionar o vínculo opcional da placa NFC ao item comercial. O vínculo `PlacaNFC.ProdutoId` com o produto de fabricação foi preservado.

Faça backup do banco antes de aplicar. Na raiz da solução:

```bash
dotnet ef database update \
  --project Repositorio \
  --startup-project BlazorApp2 \
  --context MyDBContext
```

A ferramenta EF Core deve estar instalada (`dotnet tool install --global dotnet-ef --version 10.0.12`, se necessário) e `DATABASE_URL` precisa estar definida no terminal.

## Fluxo atual da loja

1. Um usuário com papel `admin` ou `gerente` cadastra produtos em `/produtos-comerciais/visualizar` no Gestão Infinity.
2. Produtos marcados como disponíveis aparecem no catálogo público do `PlacasInfinity.Web`.
3. O cliente informa seus dados em `/comprar/{id}`.
4. O sistema reutiliza o cadastro existente em `Cliente` pelo e-mail ou cria um novo cliente, grava `VendaComercial`, `VendaComercialItem`, `Pagamento` pendente e o primeiro registro de histórico dentro de uma transação.
5. A equipe acompanha e atualiza os status em `/vendas-comerciais/visualizar` no Gestão Infinity.

## Limites que exigem configuração externa

- O pedido e o registro de pagamento pendente funcionam, mas não existe cobrança online ativa: é necessário escolher um gateway, configurar credenciais, implementar e validar webhooks antes de aceitar pagamentos reais.
- O catálogo usa a disponibilidade booleana do produto; reserva/baixa automática de estoque e cálculo de frete ainda precisam ser integrados às regras de produção e expedição.
- O checkout registra endereço de entrega na venda. O cadastro de Cliente compartilhado continua usando a tabela existente.
- O `PlacasInfinity.Web` oferece ativação pública de NFC em `/placas/nfc/{CodigoPublico}`, visualização pública dos cards e painel de edição protegido por senha em `/placas/nfc/{CodigoPublico}/editar`. A administração/produção de unidades NFC continua no Gestão Infinity. A URL gravada na tag física deve apontar para o domínio público do Placas Infinity.

## Execução

```bash
dotnet run --project BlazorApp2
dotnet run --project PlacasInfinity.Web
```

Não execute as duas aplicações contra um banco sem antes aplicar a migration compartilhada. As aplicações usam o mesmo banco; as migrations continuam centralizadas em `Repositorio`.

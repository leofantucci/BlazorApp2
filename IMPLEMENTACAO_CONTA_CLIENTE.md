# Placas Infinity — conta do cliente

## O que foi implementado
- Cadastro e login de clientes comerciais com senha protegida por `PasswordHasher`.
- `UsuarioComercial : Usuario`, vinculado a um `Cliente`.
- Autenticação por cookie, expiração deslizante e proteção antiforgery nos formulários.
- Área do cliente para editar dados pessoais, documento e endereço.
- Checkout pré-preenchido quando o cliente está autenticado; os dados do pedido continuam editáveis.
- Histórico de pedidos com status e totais.
- Navegação responsiva, menu de usuário e rodapé com contatos.
- Ícone do carrinho ampliado no desktop e somente ícone no celular.

## Imagem do ícone da marca
O header referencia `/Media/PlacaIcon.webp`. Adicione manualmente o arquivo em:
`PlacasInfinity.Web/wwwroot/Media/PlacaIcon.webp`

O ícone de usuário existente é referenciado em `/Media/userIcon.webp`.

## Aplicar a migration

Abra o terminal na pasta que contém `BlazorApp2.sln` e configure a conexão do PostgreSQL. Exemplo:

```bash
export DATABASE_URL='SUA_CONNECTION_STRING_DO_POSTGRESQL'
dotnet restore
dotnet build
dotnet ef migrations add CadastroUsuarioComercial --project Repositorio --startup-project PlacasInfinity.Web --context MyDBContext --output-dir Migrations
dotnet ef database update --project Repositorio --startup-project PlacasInfinity.Web --context MyDBContext
```

A migration adiciona o discriminador TPH para `Usuario`/`UsuarioComercial`, o relacionamento com `Cliente` e os campos adicionais de endereço. Faça backup do banco antes de aplicar em produção.

Se o comando `dotnet ef` não existir:

```bash
dotnet tool install --global dotnet-ef --version 10.0.12
```

Se já estiver instalado em outra versão:

```bash
dotnet tool update --global dotnet-ef --version 10.0.12
```

## Rodar localmente

```bash
export DATABASE_URL='SUA_CONNECTION_STRING_DO_POSTGRESQL'
dotnet run --project PlacasInfinity.Web
```

## Enviar ao GitHub

Execute na raiz do repositório:

```bash
git status
git add .
git commit -m "Adiciona conta de clientes na loja Placas Infinity"
git push -u origin HEAD
```

## Observações
- Clientes antigos/usuários internos recebem o discriminador `Interno`; novas contas da loja são `Comercial`.
- O checkout de visitante continua disponível. Pedidos de visitante não são associados automaticamente a uma conta comercial já cadastrada, mesmo que seja informado o mesmo e-mail.
- O projeto usa .NET 10. O build e a migration devem ser executados no seu ambiente com o SDK .NET 10 e a connection string válida.

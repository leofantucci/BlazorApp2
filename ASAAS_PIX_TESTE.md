# Integração PIX Asaas - Sandbox

## 1. Configuração local

Na raiz da solução, configure os User Secrets do projeto web (não coloque chaves no appsettings.json nem no Git):

```bash
dotnet user-secrets set 'Asaas:ApiKey' 'SUA_CHAVE_SANDBOX' --project PlacasInfinity.Web
dotnet user-secrets set 'Asaas:BaseUrl' 'https://api-sandbox.asaas.com/v3/' --project PlacasInfinity.Web
dotnet user-secrets set 'Asaas:WebhookToken' 'SEU_TOKEN_ALEATORIO_FORTE' --project PlacasInfinity.Web
```

O token do webhook deve ser diferente da API Key. Gere, por exemplo, com `openssl rand -hex 32`.

## 2. Banco de dados

Aplique a migration incluída no projeto antes de testar. Na pasta `BlazorApp2` (raiz da solução), execute:

```bash
dotnet ef database update --project Repositorio --startup-project PlacasInfinity.Web
```

Se o comando `dotnet ef` não existir, instale a ferramenta compatível com o EF Core 10:

```bash
dotnet tool install --global dotnet-ef --version 10.0.12
```

A migration adiciona o ID do cliente Asaas, os dados do PIX e a tabela de idempotência dos eventos. Faça backup antes de atualizar um banco com dados importantes.

## 3. URL pública do webhook

O Asaas não consegue acessar `localhost`. Exponha temporariamente a aplicação local por HTTPS (por exemplo, usando um túnel HTTPS) e cadastre no painel Sandbox do Asaas a URL pública terminada em `/webhooks/asaas`. Configure o token exatamente igual ao `Asaas:WebhookToken`. Habilite os eventos de cobrança, especialmente `PAYMENT_CONFIRMED`, `PAYMENT_RECEIVED`, `PAYMENT_DELETED` e `PAYMENT_REFUNDED`.

## 4. Teste

1. Execute a aplicação e acesse a loja.
2. Faça um pedido com nome, e-mail, CPF/CNPJ, telefone e endereço.
3. A página `/pedido/{id}` deverá mostrar o QR Code e o PIX Copia e Cola.
4. Para um teste básico de webhook, abra a cobrança no painel Sandbox e utilize a ação de confirmação manual, quando disponível.
5. Para testar o pagamento do QR Code de ponta a ponta, a documentação do Asaas exige duas contas Sandbox: a conta que gera o QR Code e outra conta pagadora com saldo disponível. A conta de destino deve ter uma chave Pix cadastrada. O saldo da conta pagadora pode ser gerado confirmando manualmente uma cobrança de teste.
6. Configure o webhook na conta que gerou a cobrança. O evento esperado para PIX recebido é `PAYMENT_RECEIVED`.
7. O webhook atualiza `Pagamento.Status`, `Pagamento.StatusProvedor`, `Pagamento.DataConfirmacaoUtc` e `VendaComercial.Status`.
8. A página atualiza o status automaticamente a cada 8 segundos.

## 5. Campos gravados

- `Cliente.AsaasCustomerId`: identifica o pagador na conta Asaas.
- `Pagamento.TransacaoExternaId`: ID da cobrança Asaas (`pay_...`).
- `Pagamento.Metodo`: `PIX`; `Pagamento.Provedor`: `Asaas`.
- `Pagamento.Status`: status interno do sistema.
- `Pagamento.StatusProvedor`: status original informado pelo Asaas.
- `Pagamento.PixQrCodeBase64`, `PixCopiaECola`, `PixExpiracaoUtc`: dados apresentados ao comprador.
- `AsaasWebhookEventos`: eventos já recebidos, com ID único para evitar processamento duplicado.

## Observações

- A resposta de criação da cobrança não significa que o PIX foi pago. Para PIX, o fluxo documentado é `PAYMENT_RECEIVED`; somente esse evento muda a venda para `Pago`. `PAYMENT_CONFIRMED` significa que o pagamento foi efetuado, mas o saldo ainda não está disponível.
- CPF/CNPJ é exigido pelo cadastro de clientes Asaas. O checkout agora solicita esse dado.
- O script SQL é fornecido separadamente porque este ambiente de edição não possui o SDK `dotnet` para gerar e validar uma migration EF Core.
- A rota `/dev/asaas-status` continua disponível somente em Development. Remova-a se não for mais necessária.

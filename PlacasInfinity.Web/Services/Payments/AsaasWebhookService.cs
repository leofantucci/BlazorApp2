using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Repositorio;
using TechToysDominio;

namespace PlacasInfinity.Web.Services.Payments;

public sealed class AsaasWebhookService(
    IDbContextFactory<MyDBContext> contextFactory,
    IOptions<AsaasOptions> options,
    ILogger<AsaasWebhookService> logger)
{
    public bool ValidarToken(string? recebido)
    {
        var esperado = options.Value.WebhookToken;
        if (string.IsNullOrWhiteSpace(esperado) || string.IsNullOrWhiteSpace(recebido)) return false;
        var a = Encoding.UTF8.GetBytes(esperado);
        var b = Encoding.UTF8.GetBytes(recebido);
        return a.Length == b.Length && CryptographicOperations.FixedTimeEquals(a, b);
    }

    public async Task ProcessarAsync(string json, CancellationToken ct = default)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var eventId = GetString(root, "id");
        var eventName = GetString(root, "event");
        if (string.IsNullOrWhiteSpace(eventId) || string.IsNullOrWhiteSpace(eventName))
            throw new InvalidOperationException("Webhook sem identificador ou tipo de evento.");
        if (!root.TryGetProperty("payment", out var payment) || payment.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("Webhook sem objeto payment.");

        var paymentId = GetString(payment, "id");
        var externalReference = GetString(payment, "externalReference");
        var status = GetString(payment, "status") ?? eventName;
        var value = payment.TryGetProperty("value", out var valueElement) && valueElement.TryGetDecimal(out var parsedValue)
            ? parsedValue : (decimal?)null;
        if (string.IsNullOrWhiteSpace(paymentId)) throw new InvalidOperationException("Webhook sem ID de cobrança.");

        await using var db = await contextFactory.CreateDbContextAsync(ct);
        if (await db.AsaasWebhookEventos.AnyAsync(e => e.EventoExternoId == eventId, ct)) return;

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        db.AsaasWebhookEventos.Add(new AsaasWebhookEvento
        {
            EventoExternoId = eventId,
            TipoEvento = eventName,
            PagamentoExternoId = paymentId,
            RecebidoEmUtc = DateTime.UtcNow,
            Processado = false
        });

        var registro = await db.Pagamento
            .Include(p => p.VendaComercial)
            .FirstOrDefaultAsync(p => p.TransacaoExternaId == paymentId, ct);

        if (registro is not null)
        {
            if (!string.IsNullOrWhiteSpace(externalReference) &&
                !string.Equals(externalReference, $"venda-{registro.VendaComercialId}", StringComparison.Ordinal))
                throw new InvalidOperationException("A referência externa do webhook não corresponde à venda.");
            if (value.HasValue && value.Value != registro.Valor)
                throw new InvalidOperationException("O valor informado pelo webhook não corresponde ao pagamento registrado.");

            registro.StatusProvedor = status;
            registro.PayloadRetorno = json.Length > 2000 ? json[..2000] : json;
            switch (eventName)
            {
                case "PAYMENT_CONFIRMED":
                    // Pagamento efetuado, mas o saldo ainda não está disponível no Asaas.
                    // Mantemos a venda aguardando até PAYMENT_RECEIVED.
                    break;
                case "PAYMENT_RECEIVED":
                    registro.Status = StatusPagamento.Aprovado;
                    registro.DataConfirmacaoUtc ??= DateTime.UtcNow;
                    if (registro.VendaComercial.Status == StatusVendaComercial.AguardandoPagamento)
                    {
                        registro.VendaComercial.Status = StatusVendaComercial.Pago;
                        registro.VendaComercial.Historico.Add(new ControleComercial
                        {
                            Status = StatusVendaComercial.Pago,
                            DataHoraUtc = DateTime.UtcNow,
                            Observacao = "Pagamento PIX recebido e disponibilizado pelo Asaas."
                        });
                    }
                    break;
                case "PAYMENT_DELETED":
                    if (registro.Status == StatusPagamento.Pendente)
                    {
                        registro.Status = StatusPagamento.Cancelado;
                        if (registro.VendaComercial.Status == StatusVendaComercial.AguardandoPagamento)
                            registro.VendaComercial.Status = StatusVendaComercial.Cancelado;
                    }
                    break;
                case "PAYMENT_REFUNDED":
                    registro.Status = StatusPagamento.Estornado;
                    break;
                case "PAYMENT_PARTIALLY_REFUNDED":
                    // O enum interno não possui estorno parcial; o status original fica em StatusProvedor.
                    break;
                case "PAYMENT_CREDIT_CARD_CAPTURE_REFUSED":
                    registro.Status = StatusPagamento.Recusado;
                    break;
            }
        }
        else
        {
            logger.LogWarning("Webhook Asaas {EventId}: cobrança {PaymentId} não encontrada no banco local.", eventId, paymentId);
            // Retorna erro ao Asaas para que ele tente entregar novamente após a conciliação.
            throw new InvalidOperationException("Cobrança do webhook ainda não localizada no banco local.");
        }

        var evento = db.AsaasWebhookEventos.Local.First(e => e.EventoExternoId == eventId);
        evento.Processado = true;
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }

    private static string? GetString(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
}

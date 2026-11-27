using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using TechToysDominio;

namespace PlacasInfinity.Web.Services.Payments;

public sealed class AsaasService
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public AsaasService(HttpClient http, IOptions<AsaasOptions> options)
    {
        var config = options.Value;
        if (!Uri.TryCreate(config.BaseUrl, UriKind.Absolute, out var baseUri) ||
            baseUri.Scheme != Uri.UriSchemeHttps ||
            !string.Equals(baseUri.Host, "api-sandbox.asaas.com", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Este build de testes aceita somente https://api-sandbox.asaas.com/v3/.");

        _http = http;
        _http.Timeout = TimeSpan.FromSeconds(30);
        _http.BaseAddress = new Uri(baseUri.ToString().TrimEnd('/') + "/");
        if (!string.IsNullOrWhiteSpace(config.ApiKey))
            _http.DefaultRequestHeaders.TryAddWithoutValidation("access_token", config.ApiKey);
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("PlacasInfinity/1.0");
    }

    private void GarantirConfiguracao()
    {
        if (!_http.DefaultRequestHeaders.Contains("access_token"))
            throw new InvalidOperationException("Configure Asaas:ApiKey com a chave da conta Sandbox.");
    }

    public async Task<(bool Sucesso, int StatusCode, string Resposta)> TestarConexaoAsync(CancellationToken ct = default)
    {
        GarantirConfiguracao();
        using var response = await _http.GetAsync("myAccount/status/", ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        return (response.IsSuccessStatusCode, (int)response.StatusCode, body);
    }

    public async Task<string> CriarClienteAsync(Cliente cliente, CancellationToken ct = default)
    {
        GarantirConfiguracao();
        var documento = SomenteDigitos(cliente.Documento);
        if (documento.Length is not (11 or 14))
            throw new InvalidOperationException("Informe um CPF ou CNPJ válido para gerar o PIX.");

        var body = new Dictionary<string, object?>
        {
            ["name"] = string.IsNullOrWhiteSpace(cliente.Nome) ? throw new InvalidOperationException("Nome do cliente obrigatório.") : cliente.Nome.Trim(),
            ["cpfCnpj"] = documento,
            ["email"] = cliente.Email,
            ["mobilePhone"] = string.IsNullOrWhiteSpace(cliente.Telefone) ? null : SomenteDigitos(cliente.Telefone),
            ["externalReference"] = $"cliente-{cliente.Id}",
            ["notificationDisabled"] = true
        };
        using var response = await _http.PostAsJsonAsync("customers", body, JsonOptions, ct);
        var json = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(ExtrairErro(json, (int)response.StatusCode));
        var result = JsonSerializer.Deserialize<AsaasCustomerResponse>(json, JsonOptions);
        return !string.IsNullOrWhiteSpace(result?.Id)
            ? result.Id
            : throw new InvalidOperationException("O Asaas não retornou o identificador do cliente.");
    }

    public async Task<AsaasPaymentResponse> CriarCobrancaPixAsync(string customerId, int vendaId, decimal valor, DateOnly vencimento, CancellationToken ct = default)
    {
        GarantirConfiguracao();
        if (string.IsNullOrWhiteSpace(customerId) || customerId.Length > 80)
            throw new InvalidOperationException("Identificador de cliente Asaas inválido.");
        if (vendaId <= 0 || valor <= 0 || valor > 1_000_000m)
            throw new InvalidOperationException("Pedido ou valor da cobrança inválido.");
        var body = new
        {
            customer = customerId,
            billingType = "PIX",
            value = valor,
            dueDate = vencimento.ToString("yyyy-MM-dd"),
            description = $"Pedido Placas Infinity #{vendaId}",
            externalReference = $"venda-{vendaId}"
        };
        using var response = await _http.PostAsJsonAsync("payments", body, JsonOptions, ct);
        var json = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(ExtrairErro(json, (int)response.StatusCode));
        return JsonSerializer.Deserialize<AsaasPaymentResponse>(json, JsonOptions)
            ?? throw new InvalidOperationException("Resposta inválida ao criar cobrança no Asaas.");
    }

    public async Task<AsaasPixQrCodeResponse> ObterQrCodePixAsync(string paymentId, CancellationToken ct = default)
    {
        GarantirConfiguracao();
        using var response = await _http.GetAsync($"payments/{Uri.EscapeDataString(paymentId)}/pixQrCode", ct);
        var json = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(ExtrairErro(json, (int)response.StatusCode));
        return JsonSerializer.Deserialize<AsaasPixQrCodeResponse>(json, JsonOptions)
            ?? throw new InvalidOperationException("O Asaas não retornou os dados do QR Code PIX.");
    }

    private static string ExtrairErro(string json, int status)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Array)
            {
                var messages = errors.EnumerateArray()
                    .Select(e => e.TryGetProperty("description", out var d) ? d.GetString() : null)
                    .Where(x => !string.IsNullOrWhiteSpace(x));
                var joined = string.Join("; ", messages);
                if (!string.IsNullOrWhiteSpace(joined)) return $"Asaas (HTTP {status}): {joined}";
            }
        }
        catch { }
        return $"Falha na API do Asaas (HTTP {status}). Verifique os dados do cliente e as configurações da conta Sandbox.";
    }

    private static string SomenteDigitos(string? valor) => new((valor ?? string.Empty).Where(char.IsDigit).ToArray());
}

public sealed class AsaasCustomerResponse { public string? Id { get; set; } }
public sealed class AsaasPaymentResponse
{
    public string? Id { get; set; }
    public string? Status { get; set; }
    public string? BillingType { get; set; }
    public decimal Value { get; set; }
}
public sealed class AsaasPixQrCodeResponse
{
    public string? EncodedImage { get; set; }
    public string? Payload { get; set; }
    public DateTimeOffset? ExpirationDate { get; set; }
}

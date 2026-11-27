namespace PlacasInfinity.Web.Services.Payments;

public sealed class AsaasOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api-sandbox.asaas.com/v3/";
    public string WebhookToken { get; set; } = string.Empty;
}

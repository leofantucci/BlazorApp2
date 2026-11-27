using System.ComponentModel.DataAnnotations;

namespace TechToysDominio;

public sealed class AsaasWebhookEvento
{
    public int Id { get; set; }
    [Required, StringLength(150)] public string EventoExternoId { get; set; } = string.Empty;
    [Required, StringLength(100)] public string TipoEvento { get; set; } = string.Empty;
    [StringLength(150)] public string? PagamentoExternoId { get; set; }
    public DateTime RecebidoEmUtc { get; set; } = DateTime.UtcNow;
    public bool Processado { get; set; }
}

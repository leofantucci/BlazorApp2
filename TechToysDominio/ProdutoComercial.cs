using System.ComponentModel.DataAnnotations;

namespace TechToysDominio;

public class ProdutoComercial
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500)]
    public string Foto { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Descricao { get; set; }

    public decimal Preco { get; set; }
    public bool Disponivel { get; set; } = true;
    public int? ProdutoBaseId { get; set; }
    public Produto? ProdutoBase { get; set; }
    public DateTime DataCriacaoUtc { get; set; } = DateTime.UtcNow;
    public DateTime? DataAtualizacaoUtc { get; set; }
}

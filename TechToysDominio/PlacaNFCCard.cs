namespace TechToysDominio;

public enum TipoCardNFC
{
    Personalizado = 0,
    Instagram = 1,
    Facebook = 2,
    WhatsApp = 3,
    YouTube = 4,
    AvaliacaoGoogle = 5,
    Site = 6
}

public class PlacaNFCCard
{
    public int Id { get; set; }

    public int PlacaNFCId { get; set; }
    public PlacaNFC PlacaNFC { get; set; } = null!;

    public TipoCardNFC Tipo { get; set; } = TipoCardNFC.Personalizado;

    // Caminho local (Media/Instagram.webp) ou URL externa.
    public string Icone { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string Link { get; set; } = string.Empty;

    public int Ordem { get; set; }
}

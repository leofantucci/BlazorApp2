using TechToysDominio;

namespace BlazorApp2.Components.Placas;

public record ModeloCardNFC(
    TipoCardNFC Tipo,
    string Nome,
    string Descricao,
    string Icone);

public static class PlacaNfcCardTemplates
{
    public static IReadOnlyList<ModeloCardNFC> Todos { get; } = new[]
    {
        new ModeloCardNFC(TipoCardNFC.Instagram, "Instagram", "Nos siga no Instagram", "Media/instagram.webp"),
        new ModeloCardNFC(TipoCardNFC.Facebook, "Facebook", "Curta nossa página no Facebook", "Media/facebook.webp"),
        new ModeloCardNFC(TipoCardNFC.WhatsApp, "WhatsApp", "Fale conosco no WhatsApp", "Media/whatsapp.webp"),
        new ModeloCardNFC(TipoCardNFC.YouTube, "YouTube", "Inscreva-se no nosso canal", "Media/youtube.webp"),
        new ModeloCardNFC(TipoCardNFC.AvaliacaoGoogle, "Avaliação Google", "Avalie nosso atendimento no Google", "Media/google.webp"),
        new ModeloCardNFC(TipoCardNFC.Site, "Site", "Visite nosso site", "Media/site.webp")
    };
}

using System.Text;
using QRCoder;

namespace BlazorApp2.Infrastructure;

/// <summary>
/// Geração de QR Codes usada pelas telas de placas. Mantém o mesmo formato
/// de data URL já utilizado pela página de QR Code.
/// </summary>
public static class QrCodeImage
{
    public static string ObterPngDataUrl(string url)
    {
        using var gerador = new QRCodeGenerator();
        using var dados = gerador.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(dados).GetGraphic(8);
        return "data:image/png;base64," + Convert.ToBase64String(png);
    }

    public static string ObterSvgDataUrl(string url)
    {
        using var gerador = new QRCodeGenerator();
        using var dados = gerador.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);

        var matriz = dados.ModuleMatrix;
        var tamanho = matriz.Count;
        const int margem = 4;
        var tamanhoTotal = tamanho + (margem * 2);
        var caminho = new StringBuilder();

        // Cada sequência horizontal de módulos escuros vira um retângulo vetorial.
        // Isso evita rastreamento de pixels e mantém todos os módulos perfeitamente quadrados.
        for (var y = 0; y < tamanho; y++)
        {
            var x = 0;
            while (x < tamanho)
            {
                if (!matriz[y][x])
                {
                    x++;
                    continue;
                }

                var inicio = x;
                while (x < tamanho && matriz[y][x]) x++;
                var largura = x - inicio;
                caminho.Append($"M{inicio + margem} {y + margem}h{largura}v1h-{largura}z");
            }
        }

        var svg = $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {tamanhoTotal} {tamanhoTotal}\" shape-rendering=\"crispEdges\"><path fill=\"#000000\" d=\"{caminho}\"/></svg>";
        return "data:image/svg+xml;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));
    }
}

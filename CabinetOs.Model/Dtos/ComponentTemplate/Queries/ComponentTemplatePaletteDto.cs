using CabinetOs.Core.Model;

namespace CabinetOs.Model.Dtos.ComponentTemplate.Queries;

/// <summary>
/// Palet (stencil kutuphanesi) kartı — pin semasiyla birlikte.
///
/// <b>Pinler neden burada.</b> Paletten birakilan cihazin pin ve kanal Id'lerini
/// ISTEMCI uretir; bunun icin sablonun pin semasini bilmesi sart. Sema olmadan
/// cihaz canvas'ta pinsiz dogar ve kaydedilene kadar kablolanamazdi.
///
/// Sunucu pinlerin ICERIGINI hala sablondan kopyalar (bkz.
/// <c>DiagramService.InstantiateTemplatePins</c>); istemciden gelen tek sey Guid.
///
/// Ayri sorgu anahtari + uzun staleTime ile cachelenir: sema her kabinette aynidir
/// ve yalnizca sablon yazarligi degistirir.
/// </summary>
public class ComponentTemplatePaletteDto : IDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public int DeviceTypeId { get; set; }
    public bool IsSystemTemplate { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    /// <summary>#RRGGBB renk dizesi.</summary>
    public string BackgroundColor { get; set; } = null!;
    public string? BackgroundImageUrl { get; set; }

    /// <summary>
    /// Bos olabilir: pano cercevesi gibi dekoratif bir sablonun pini olmayabilir,
    /// o zaman cihaz da pinsiz dogar.
    ///
    /// Ayri bir <c>PinCount</c> alani YOKTUR — <c>Pins.Count</c> varken ikinci bir
    /// sayac, sessizce ayrisabilecek ikinci bir dogruluk kaynagi olurdu.
    /// </summary>
    public List<ComponentTemplatePalettePinDto> Pins { get; set; } = [];
}

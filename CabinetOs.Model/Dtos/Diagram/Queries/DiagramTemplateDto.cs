using CabinetOs.Core.Model;

namespace CabinetOs.Model.Dtos.Diagram.Queries;

/// <summary>
/// Cihazin cozumlenmis sablon OZETI — yalnizca gorsel spec.
///
/// Neden her cihaza gomulu: node'un boyutu ve rengi sablondan gelir. Sablon
/// pasife alinsa bile kabin dogru render olmali, bu yuzden ozet cihazla birlikte
/// tasinir ve palet cagrisina bagimlilik olusmaz.
///
/// Sablonun PINLERI burada YOK: cihazin gercek pinleri zaten
/// <c>DiagramDeviceDto.Pins</c>'te cozumlenmis halde geliyor. Sablon pinlerini de
/// eklemek 30 cihazlik bir kabinde yuzlerce satir gereksiz yuk olurdu.
/// </summary>
public class DiagramTemplateDto : IDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public int DeviceTypeId { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    /// <summary>0xRRGGBB tamsayisi (renk dizesi degil) — bkz. 00-conventions.md.</summary>
    public int BackgroundColor { get; set; }
    public string? BackgroundImageUrl { get; set; }
}

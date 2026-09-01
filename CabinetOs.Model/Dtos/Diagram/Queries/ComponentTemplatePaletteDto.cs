using CabinetOs.Core.Model;

namespace CabinetOs.Model.Dtos.Diagram.Queries;

/// <summary> 
/// ComponentTemplate PINLERI burada YOK: paletten birakilan cihazin pinleri sunucuda
/// sablondan uretilir (D2, <c>instantiatePins</c>), istemcinin onlari bilmesine gerek kalmaz.
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
    public int PinCount { get; set; }
}

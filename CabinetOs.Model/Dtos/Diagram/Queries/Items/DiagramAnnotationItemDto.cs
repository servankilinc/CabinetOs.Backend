using CabinetOs.Core.Model;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Model.Dtos.Diagram.Queries.Items;

/// <summary>
/// Cihaz olmayan diyagram elemani (serbest metin, kutu, not).
///
/// Mevcut <c>Dtos.DiagramAnnotation.Queries.DiagramAnnotationDto</c>'dan ayri
/// olmasinin sebebi: o tip <c>CabinetId</c> + <c>CabinetName</c> tasiyor. Bu
/// aggregate zaten tek bir kabine ait oldugu icin ayni kabin adini her
/// annotation'da tekrarlamak gereksiz yuktur.
/// </summary>
public class DiagramAnnotationItemDto : IDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public double CoordinateX { get; set; }
    public double CoordinateY { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double Rotation { get; set; }
    public int ZIndex { get; set; }
    public bool IsLocked { get; set; }
    public bool IsVisible { get; set; }
    public string Text { get; set; } = null!;
    public AnnotationShape Shape { get; set; }
    /// <summary>CSS renk dizesi.</summary>
    public string BackgroundColor { get; set; } = null!;
    public string FontColor { get; set; } = null!;
    public double FontSize { get; set; }
    public bool IsBold { get; set; }
    public string BorderColor { get; set; } = null!;
}

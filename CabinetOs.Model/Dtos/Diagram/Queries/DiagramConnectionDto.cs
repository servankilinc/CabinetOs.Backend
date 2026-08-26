using CabinetOs.Core.Model;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Model.Dtos.Diagram.Queries;

/// <summary>
/// <see cref="SourceDeviceId"/> / <see cref="TargetDeviceId"/> DENORMALIZE'dir:
/// React Flow edge'in <c>source</c>/<c>target</c> alanlarinda NODE id'si ister,
/// pin id'si degil. Bunlar olmadan istemci tek bir kablo cizmeden once tum
/// pin -> cihaz indeksini kurmak zorunda kalirdi.
/// </summary>
public class DiagramConnectionDto : IDto
{
    public Guid Id { get; set; }
    public Guid CabinetId { get; set; }
    public Guid SourcePinId { get; set; }
    public Guid TargetPinId { get; set; }
    /// <summary>React Flow <c>source</c> — pinin bagli oldugu cihaz.</summary>
    public Guid SourceDeviceId { get; set; }
    /// <summary>React Flow <c>target</c> — pinin bagli oldugu cihaz.</summary>
    public Guid TargetDeviceId { get; set; }
    /// <summary>Nullable: draw-first UX'te yeni cizilen kablonun henuz etiketi yoktur.</summary>
    public string? Label { get; set; }
    public WireType WireType { get; set; }
    /// <summary>CSS renk dizesi (or. "#EF4444") — sablon renginden farkli olarak burada string.</summary>
    public string Color { get; set; } = null!;
    public LineStyle LineStyle { get; set; }
    public double StrokeWidth { get; set; }
    public EdgeRouting Routing { get; set; }
    /// <summary>Ara kirilma noktalari: kaynak -> hedef sirali, IKI UC NOKTA HARIC. Bos olabilir.</summary>
    public ICollection<PointDto> Waypoints { get; set; } = [];
    public int ZIndex { get; set; }
}

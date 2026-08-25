using CabinetOs.Core.Model;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Model.Entities;

public class Connection : IEntity, ISoftDeletableEntity, IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid CabinetId { get; set; }
    public Guid SourcePinId { get; set; }
    public Guid TargetPinId { get; set; }
    public string? Label { get; set; }
    public WireType WireType { get; set; }
    public string Color { get; set; } = null!;
    public LineStyle LineStyle { get; set; }
    public double StrokeWidth { get; set; }
    public EdgeRouting Routing { get; set; }
    /// <summary> Ara kirilma noktalari: kaynak -> hedef sirali, IKI UC NOKTA HARIC, Nullable, cunku yeni cizilen bir kablonun henuz kirilma noktasi yoktur. </summary>
    public string? WaypointsJson { get; set; }
    public int ZIndex { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? CreateDateUtc { get; set; }
    public DateTime? UpdateDateUtc { get; set; }
    public string? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedDateUtc { get; set; }
    public virtual Cabinet? Cabinet { get; set; }
    public virtual Pin? SourcePin { get; set; }
    public virtual Pin? TargetPin { get; set; }
}
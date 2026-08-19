using CabinetOs.Core.Model;

namespace CabinetOs.Model.Entities
{
    public class Connection : IEntity, ISoftDeletableEntity, IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid SourcePinId { get; set; }
        public Guid TargetPinId { get; set; }
        public string Label { get; set; } = null!;
        public int WireType { get; set; }
        public string Color { get; set; } = null!;
        public int LineStyle { get; set; }
        public double StrokeWidth { get; set; }
        public string WaypointsJson { get; set; } = null!;
        public int ZIndex { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreateDateUtc { get; set; }
        public DateTime? UpdateDateUtc { get; set; }
        public string? DeletedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDateUtc { get; set; }
        public virtual Pin? SourcePin { get; set; }
        public virtual Pin? TargetPin { get; set; }
    }
}
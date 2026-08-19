using CabinetOs.Core.Model;

namespace CabinetOs.Model.Entities
{
    public class ComponentTemplatePin : IEntity, IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid ComponentTemplateId { get; set; }
        public string Name { get; set; } = null!;
        public double RelativeX { get; set; }
        public double RelativeY { get; set; }
        public int? ChannelNumber { get; set; }
        public int Function { get; set; }
        public int Direction { get; set; }
        public int SignalLayer { get; set; }
        public int? VoltageLevel { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreateDateUtc { get; set; }
        public DateTime? UpdateDateUtc { get; set; }
        public virtual ComponentTemplate? ComponentTemplate { get; set; }
    }
}
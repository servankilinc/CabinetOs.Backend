using CabinetOs.Core.Model;

namespace CabinetOs.Model.Entities
{
    public class Pin : IEntity, ISoftDeletableEntity, IAuditableEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public double RelativeX { get; set; }
        public double RelativeY { get; set; }
        public Guid? IoChannelId { get; set; }
        public int Function { get; set; }
        public int SignalLayer { get; set; }
        public int? VoltageLevel { get; set; }
        public Guid DeviceId { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreateDateUtc { get; set; }
        public DateTime? UpdateDateUtc { get; set; }
        public string? DeletedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDateUtc { get; set; }
        public virtual Device? Device { get; set; }
        public virtual IoChannel? IoChanel { get; set; }
        public virtual ICollection<Connection>? SourcePinConnections { get; set; }
        public virtual ICollection<Connection>? TargetPinConnections { get; set; }
    }
}
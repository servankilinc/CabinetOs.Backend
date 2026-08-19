using CabinetOs.Core.Model;

namespace CabinetOs.Model.Entities
{
    public class IoChannel : IEntity, ISoftDeletableEntity, IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid DeviceId { get; set; }
        public int ChannelNumber { get; set; }
        public int Direction { get; set; }
        public bool IsEnabled { get; set; }
        public string? CurrentValue { get; set; }
        public string Name { get; set; } = null!;
        public DateTime? ValueUpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreateDateUtc { get; set; }
        public DateTime? UpdateDateUtc { get; set; }
        public string? DeletedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDateUtc { get; set; }
        public virtual Device? Device { get; set; }
        public virtual ICollection<Pin>? Pins { get; set; }
        public virtual ICollection<DeviceCommand>? DeviceCommands { get; set; }
    }
}
using CabinetOs.Core.Model;

namespace CabinetOs.Model.Entities
{
    public class Device : IEntity, IAuditableEntity, IActivatableEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public double CoordinateX { get; set; }
        public double CoordinateY { get; set; }
        public double Rotation { get; set; }
        public int ZIndex { get; set; }
        public bool IsLocked { get; set; }
        public bool IsVisible { get; set; }
        public Guid CabinetId { get; set; }
        public Guid ComponentTemplateId { get; set; }
        public int? DeviceStatusId { get; set; }
        public string? IpAddress { get; set; }
        public string? MacAddress { get; set; }
        public string? ExternalCode { get; set; }
        public DateTime? LastSeen { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreateDateUtc { get; set; }
        public DateTime? UpdateDateUtc { get; set; }
        public bool IsActive { get; set; }
        public virtual Cabinet? Cabinet { get; set; }
        public virtual ComponentTemplate? ComponentTemplate { get; set; }
        public virtual DeviceStatus? DeviceStatus { get; set; }
        public virtual ICollection<IoChannel>? IoChanels { get; set; }
        public virtual ICollection<Pin>? Pins { get; set; }
        public virtual ICollection<DeviceCommand>? DeviceCommands { get; set; }
    }
}
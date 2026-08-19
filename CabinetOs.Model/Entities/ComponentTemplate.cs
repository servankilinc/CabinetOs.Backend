using CabinetOs.Core.Model;

namespace CabinetOs.Model.Entities
{
    public class ComponentTemplate : IEntity, IAuditableEntity, IActivatableEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public int DeviceTypeId { get; set; }
        public bool IsSystemTemplate { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public int BackgroundColor { get; set; }
        public string? BackgroundImageUrl { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreateDateUtc { get; set; }
        public DateTime? UpdateDateUtc { get; set; }
        public bool IsActive { get; set; }
        public virtual DeviceType? DeviceType { get; set; }
        public virtual ICollection<ComponentTemplatePin>? ComponentTemplatePins { get; set; }
        public virtual ICollection<Device>? Devices { get; set; }
    }
}
using CabinetOs.Core.Model;

namespace CabinetOs.Model.Entities
{
    public class DeviceType : IEntity, IAuditableEntity, IImmutableEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreateDateUtc { get; set; }
        public DateTime? UpdateDateUtc { get; set; }
        public virtual ICollection<ComponentTemplate>? ComponentTemplates { get; set; }
    }
}
using CabinetOs.Core.Model;

namespace CabinetOs.Model.Entities
{
    public class Permission : IEntity, IAuditableEntity, IImmutableEntity
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreateDateUtc { get; set; }
        public DateTime? UpdateDateUtc { get; set; }
        public virtual ICollection<RolePermission>? RolePermissions { get; set; }
    }
}
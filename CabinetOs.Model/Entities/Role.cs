using CabinetOs.Core.Model;
using Microsoft.AspNetCore.Identity;

namespace CabinetOs.Model.Entities
{
    public class Role : IdentityRole<Guid>, IEntity, IAuditableEntity, IActivatableEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsImmutable { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreateDateUtc { get; set; }
        public DateTime? UpdateDateUtc { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<UserRole>? UserRoles { get; set; }
        public virtual ICollection<RolePermission>? RolePermissions { get; set; }
    }
}
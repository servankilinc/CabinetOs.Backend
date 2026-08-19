using CabinetOs.Core.Model;
using Microsoft.AspNetCore.Identity;

namespace CabinetOs.Model.Entities
{
    public class User : IdentityUser<Guid>, IEntity, IAuditableEntity, IActivatableEntity
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = null!;
        public string? Email { get; set; }
        public Guid CompanyId { get; set; }
        public string FullName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreateDateUtc { get; set; }
        public DateTime? UpdateDateUtc { get; set; }
        public bool IsActive { get; set; }
        public virtual Company? Comany { get; set; }
        public virtual ICollection<UserRole>? UserRoles { get; set; }
        public virtual ICollection<DeviceCommand>? DeviceCommands { get; set; }
        public virtual ICollection<RefreshToken>? RefreshTokens { get; set; }
    }
}
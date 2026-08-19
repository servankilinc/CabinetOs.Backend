using CabinetOs.Core.Model;

namespace CabinetOs.Model.Entities
{
    public class RolePermission : IEntity
    {
        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }
        public virtual Role? Role { get; set; }
        public virtual Permission? Permission { get; set; }
    }
}
using CabinetOs.Core.Model;

namespace CabinetOs.Model.Entities
{
    public class AuditLog : IEntity, IAuditableEntity, IImmutableEntity
    {
        public long Id { get; set; }
        public Guid UserId { get; set; }
        public int Action { get; set; }
        public int TargetType { get; set; }
        public int? TargetId { get; set; }
        public string? Details { get; set; }
        public bool IsSuccess { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreateDateUtc { get; set; }
        public DateTime? UpdateDateUtc { get; set; }
    }
}
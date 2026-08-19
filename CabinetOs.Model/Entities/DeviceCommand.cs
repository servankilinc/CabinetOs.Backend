using CabinetOs.Core.Model;

namespace CabinetOs.Model.Entities
{
    public class DeviceCommand : IEntity, ISoftDeletableEntity, IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid DeviceId { get; set; }
        public Guid? IoChannelId { get; set; }
        public int CommandType { get; set; }
        public string? PayloadJson { get; set; }
        public int Status { get; set; }
        public Guid? RequestedByUserId { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public string? ResultMessage { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreateDateUtc { get; set; }
        public DateTime? UpdateDateUtc { get; set; }
        public string? DeletedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDateUtc { get; set; }
        public virtual Device? Device { get; set; }
        public virtual IoChannel? IoChanel { get; set; }
        public virtual User? RequesterUser { get; set; }
    }
}
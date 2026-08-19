using CabinetOs.Core.Model;

namespace CabinetOs.Model.Entities
{
    public class Cabinet : IEntity, IAuditableEntity, IActivatableEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid CompanyId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? LocationDescription { get; set; }
        public string? GsmIp { get; set; }
        public string? NetworkIp { get; set; }
        public int? DeviceStatusId { get; set; }
        public DateTime? LastSeen { get; set; }
        public string ScadaBaseUrl { get; set; } = null!;
        public bool ScadaIsEnabled { get; set; }
        public int ScadaCommandTimeoutMs { get; set; }
        public DateTime? ScadaLastIngestAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreateDateUtc { get; set; }
        public DateTime? UpdateDateUtc { get; set; }
        public bool IsActive { get; set; }
        public virtual Company? Company { get; set; }
        public virtual DeviceStatus? DeviceStatus { get; set; }
        public virtual ICollection<Device>? Devices { get; set; }
        public virtual ICollection<DiagramAnnotation>? DiagramAnnotations { get; set; }
    }
}
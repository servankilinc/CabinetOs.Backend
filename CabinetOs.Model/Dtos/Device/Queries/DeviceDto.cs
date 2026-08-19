using CabinetOs.Core.Model;

namespace CabinetOs.Model.Dtos.Device.Queries
{
    public class DeviceDto : IDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsLocked { get; set; }
        public Guid CabinetId { get; set; }
        public int? DeviceStatusId { get; set; }
        public string? ExternalCode { get; set; }
        public DateTime? LastSeen { get; set; }
    }
}
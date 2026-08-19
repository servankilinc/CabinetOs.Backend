using CabinetOs.Core.Model;

namespace CabinetOs.Model.Dtos.Device.Queries
{
    public class DeviceDetailDto : IDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public double CoordinateX { get; set; }
        public double CoordinateY { get; set; }
        public double Rotation { get; set; }
        public int ZIndex { get; set; }
        public bool IsLocked { get; set; }
        public bool IsVisible { get; set; }
        public Guid CabinetId { get; set; }
        public Guid ComponentTemplateId { get; set; }
        public string ComponentTemplateName { get; set; } = null!;
        public int? DeviceStatusId { get; set; }
        public string DeviceStatusName { get; set; } = null!;
        public string? IpAddress { get; set; }
        public string? MacAddress { get; set; }
        public string? ExternalCode { get; set; }
        public DateTime? LastSeen { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreateDateUtc { get; set; }
        public DateTime? UpdateDateUtc { get; set; }
        public bool IsActive { get; set; }
    }
}
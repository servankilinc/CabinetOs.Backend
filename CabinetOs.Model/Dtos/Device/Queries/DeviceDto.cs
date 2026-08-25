using CabinetOs.Core.Model;

namespace CabinetOs.Model.Dtos.Device.Queries
{
    public class DeviceDto : IDto
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
        public int? DeviceStatusId { get; set; }
        public string? ExternalCode { get; set; }
        public DateTime? LastSeen { get; set; }
        public bool IsActive { get; set; }
    }
}
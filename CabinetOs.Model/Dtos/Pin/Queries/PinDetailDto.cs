using CabinetOs.Core.Model;

namespace CabinetOs.Model.Dtos.Pin.Queries
{
    public class PinDetailDto : IDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public double RelativeX { get; set; }
        public double RelativeY { get; set; }
        public Guid? IoChannelId { get; set; }
        public string IoChanelName { get; set; } = null!;
        public int Function { get; set; }
        public int SignalLayer { get; set; }
        public int? VoltageLevel { get; set; }
        public Guid DeviceId { get; set; }
        public string DeviceName { get; set; } = null!;
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreateDateUtc { get; set; }
        public DateTime? UpdateDateUtc { get; set; }
        public string? DeletedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDateUtc { get; set; }
    }
}
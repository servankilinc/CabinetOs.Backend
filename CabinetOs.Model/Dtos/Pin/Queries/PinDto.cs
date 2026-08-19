using CabinetOs.Core.Model;

namespace CabinetOs.Model.Dtos.Pin.Queries
{
    public class PinDto : IDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public double RelativeX { get; set; }
        public double RelativeY { get; set; }
        public int Function { get; set; }
        public int SignalLayer { get; set; }
        public int? VoltageLevel { get; set; }
        public Guid DeviceId { get; set; }
    }
}
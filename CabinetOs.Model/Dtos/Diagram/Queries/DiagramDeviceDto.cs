using CabinetOs.Core.Model;

namespace CabinetOs.Model.Dtos.Diagram.Queries;

public class DiagramDeviceDto : IDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public double CoordinateX { get; set; }
    public double CoordinateY { get; set; }
    public double Rotation { get; set; }
    public int ZIndex { get; set; }
    public bool IsLocked { get; set; }
    public bool IsVisible { get; set; }
    public bool IsActive { get; set; }
    public Guid ComponentTemplateId { get; set; }
    /// <summary>SCADA tarafindaki kimlik; ingest bu kodla cihaz cozumler.</summary>
    public string? ExternalCode { get; set; }
    public int? DeviceStatusId { get; set; }
    public string? DeviceStatusName { get; set; }
    public DateTime? LastSeen { get; set; }
    public DiagramTemplateDto Template { get; set; } = null!;
    public ICollection<DiagramPinDto> Pins { get; set; } = [];
    public ICollection<DiagramIoChannelDto> IoChannels { get; set; } = [];
}

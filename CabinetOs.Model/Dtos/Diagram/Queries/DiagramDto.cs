using CabinetOs.Core.Model;

namespace CabinetOs.Model.Dtos.Diagram.Queries;

/// <summary> <c>GET /api/Diagram/cabinet/{id}</c> yaniti — editorun acilisi icin gereken HER SEY tek istekte. </summary>
public class DiagramDto : IDto
{
    public DiagramCabinetDto Cabinet { get; set; } = null!;
    public ICollection<DiagramDeviceDto> Devices { get; set; } = [];
    public ICollection<DiagramConnectionDto> Connections { get; set; } = [];
    public ICollection<DiagramAnnotationItemDto> Annotations { get; set; } = [];
    /// <summary>Kabin bazlidir; kayitli satir yoksa VARSAYILAN doner ve satir olusturulmaz.</summary>
    public DiagramCanvasSettingsDto CanvasSettings { get; set; } = null!;
    /// <summary>Anlik goruntunun alindigi an; istemcinin tazelik gostergesi.</summary>
    public DateTime FetchedAtUtc { get; set; }
}

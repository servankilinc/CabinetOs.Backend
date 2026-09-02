using CabinetOs.Core.Model;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Model.Dtos.Diagram.Queries.Items;

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
    public DiagramComponentTemplateDto Template { get; set; } = null!;
    public ICollection<DiagramPinDto> Pins { get; set; } = [];
    public ICollection<DiagramIoChannelDto> IoChannels { get; set; } = [];
}

/// <summary>
/// Cihazin cozumlenmis sablon OZETI — yalnizca gorsel spec.
///
/// Neden her cihaza gomulu: node'un boyutu ve rengi sablondan gelir. Sablon
/// pasife alinsa bile kabin dogru render olmali, bu yuzden ozet cihazla birlikte
/// tasinir ve palet cagrisina bagimlilik olusmaz.
///
/// Sablonun PINLERI burada YOK: cihazin gercek pinleri zaten
/// <c>DiagramDeviceDto.Pins</c>'te cozumlenmis halde geliyor. Sablon pinlerini de
/// eklemek 30 cihazlik bir kabinde yuzlerce satir gereksiz yuk olurdu.
/// </summary>
public class DiagramComponentTemplateDto : IDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public int DeviceTypeId { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    /// <summary>#RRGGBB renk dizesi — bkz. 00-conventions.md.</summary>
    public string BackgroundColor { get; set; } = null!;
    public string? BackgroundImageUrl { get; set; }
}


/// <summary>
/// Canvas'ta bir React Flow <c>&lt;Handle&gt;</c> olarak render edilen pin. Handle id'si dogrudan <see cref="Id"/>'dir (1:1, sonek yok).
/// </summary>
public class DiagramPinDto : IDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    /// <summary>Sablonun genisligine gore 0..1 normalize kesir — CSS'te <c>left: X*100%</c>.</summary>
    public double RelativeX { get; set; }
    /// <summary>Sablonun yuksekligine gore 0..1 normalize kesir — CSS'te <c>top: Y*100%</c>.</summary>
    public double RelativeY { get; set; }
    /// <summary>Handle'in hangi kenarda duracagi. RelativeX/Y tek basina bunu belirleyemez.</summary>
    public HandleSide Side { get; set; }
    public PinFunction Function { get; set; }
    public PinDirection Direction { get; set; }
    /// <summary>Null = belirtilmemis. Baglanti dogrulamasi yalnizca IKI UC de doluysa seviye karsilastirir.</summary>
    public VoltageLevel? VoltageLevel { get; set; }
    public int? ChannelNumber { get; set; }
    /// <summary>Bu pinin turedigi sablon pini; sablon degistiginde neyin etkilendigi izlenebilsin diye.</summary>
    public Guid? ComponentTemplatePinId { get; set; }
    /// <summary>Bagli telemetri kanali. Canli DEGER burada degil, ayri kanaldan akar (bkz. K7).</summary>
    public Guid? IoChannelId { get; set; }
}


/// <summary>
/// Cihazin telemetri kanalinin STATIK tanimi.
///
/// <c>CurrentValue</c> ve <c>ValueUpdatedAt</c> BILEREK YOK. Canli deger ayri bir
/// kanaldan (SignalR) akar ve istemcide TanStack Query cache'ine degil harici bir store'a yazilir.
/// </summary>
public class DiagramIoChannelDto : IDto
{
    public Guid Id { get; set; }
    public int ChannelNumber { get; set; }
    public PinDirection Direction { get; set; }
    public bool IsEnabled { get; set; }
    public string Name { get; set; } = null!;
}

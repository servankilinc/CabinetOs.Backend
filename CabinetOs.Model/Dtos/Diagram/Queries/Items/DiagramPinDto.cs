using CabinetOs.Core.Model;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Model.Dtos.Diagram.Queries.Items;

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

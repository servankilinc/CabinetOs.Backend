using CabinetOs.Core.Model;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Model.Dtos.ComponentTemplate.Queries;

/// <summary>
/// Palet sablonunun pin semasindaki tek bir pin.
///
/// <c>ComponentTemplatePinDto</c> YENIDEN KULLANILMAZ: o denetim alanlarini
/// (<c>CreatedBy</c>, <c>UpdateDateUtc</c>...) da tasiyor ve bunlar paletin isi
/// degil. Buradaki alanlar tam olarak istemcinin cihazi canvas'ta cizmek ve
/// pinlerini urettikten sonra kablo dogrulamasi yapmak icin ihtiyac duyduklaridir
/// — <see cref="Diagram.Queries.DiagramPinDto"/> ile ayni kume.
/// </summary>
public class ComponentTemplatePalettePinDto : IDto
{
    /// <summary>
    /// Sablon pininin Id'si. Cihaz pininin Id'si DEGILDIR: istemci cihazi
    /// birakirken her pin icin yeni bir Guid uretir ve hangi sablon pininden
    /// turedigini bu alanla bildirir.
    /// </summary>
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    /// <summary>Sablonun genisligine gore 0..1 normalize kesir.</summary>
    public double RelativeX { get; set; }
    /// <summary>Sablonun yuksekligine gore 0..1 normalize kesir.</summary>
    public double RelativeY { get; set; }
    public HandleSide Side { get; set; }
    public PinFunction Function { get; set; }
    public PinDirection Direction { get; set; }
    /// <summary>Null = belirtilmemis. Kablo dogrulamasi yalnizca IKI UC de doluysa karsilastirir.</summary>
    public VoltageLevel? VoltageLevel { get; set; }
    /// <summary>Null = bu pinin telemetri kanali yok; dolu olanlar cihazda <c>IoChannel</c> uretir.</summary>
    public int? ChannelNumber { get; set; }
}

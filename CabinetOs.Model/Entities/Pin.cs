using CabinetOs.Core.Model;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Model.Entities;

public class Pin : IEntity, ISoftDeletableEntity, IAuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    /// <summary>Sablonun genisligine gore 0..1 normalize kesir (CHECK ile kisitli).</summary>
    public double RelativeX { get; set; }
    /// <summary>Sablonun yuksekligine gore 0..1 normalize kesir (CHECK ile kisitli).</summary>
    public double RelativeY { get; set; }
    /// <summary>React Flow Handle position karsiligi.</summary>
    public HandleSide Side { get; set; }
    public Guid? IoChannelId { get; set; }
    public PinFunction Function { get; set; }
    /// <summary>Yon: ComponentTemplatePin'de vardi, Pin'de yoktu. Kumanda dogrulamasi buna bakar.</summary>
    public PinDirection Direction { get; set; }
    public VoltageLevel? VoltageLevel { get; set; }
    /// <summary>Modul uzerindeki kanal numarasi; IoChannel eslesmesinin kaynagi.</summary>
    public int? ChannelNumber { get; set; }
    public Guid DeviceId { get; set; }
    /// <summary>Bu pinin uretildigi sablon pini — sablon degisince neyin turedigi izlenebilsin diye.</summary>
    public Guid? ComponentTemplatePinId { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? CreateDateUtc { get; set; }
    public DateTime? UpdateDateUtc { get; set; }
    public string? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedDateUtc { get; set; }
    public virtual Device? Device { get; set; }
    public virtual IoChannel? IoChannel { get; set; }
    public virtual ComponentTemplatePin? ComponentTemplatePin { get; set; }
    public virtual ICollection<Connection>? SourcePinConnections { get; set; }
    public virtual ICollection<Connection>? TargetPinConnections { get; set; }
}
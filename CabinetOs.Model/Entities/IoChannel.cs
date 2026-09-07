using CabinetOs.Core.Model;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Model.Entities;

public class IoChannel : IEntity, ISoftDeletableEntity, IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }

    /// <summary>
    /// Kanalın kabini — <b>denormalize</b>, <c>Device.CabinetId</c>'nin kopyası.
    ///
    /// <b>Neden var.</b> Kabin BİR kontrol kartıdır ve kartın adres uzayı düzdür:
    /// <c>IN1</c> kabinde tektir, cihazda değil. Benzersizlik bu yüzden
    /// <c>(CabinetId, Direction, ChannelNumber)</c> üzerinde kuruluyor ve bir
    /// kolon olmadan indeksle ifade edilemezdi.
    ///
    /// Aynı desen <see cref="ChannelEvent"/>'te de var. Yan kazanç: SCADA ingest'i
    /// kanalı <b>tek sorguda, join'siz</b> çözüyor — eskiden kabinin bütün
    /// cihazlarını yükleyip sözlük kurmak gerekiyordu.
    ///
    /// <b>Ayrışma yolu yok:</b> tek yazma yeri <c>InstantiateTemplatePins</c> ve
    /// cihaz başka kabine taşınamıyor (diyagram kaydetme çapraz-kabin düzenlemeyi
    /// 400 ile reddediyor).
    /// </summary>
    public Guid CabinetId { get; set; }

    public int ChannelNumber { get; set; }
    public PinDirection Direction { get; set; }
    public bool IsEnabled { get; set; }
    public string? CurrentValue { get; set; }
    public string Name { get; set; } = null!;
    public DateTime? ValueUpdatedAt { get; set; }

    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? CreateDateUtc { get; set; }
    public DateTime? UpdateDateUtc { get; set; }
    public string? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedDateUtc { get; set; }
    public virtual Device? Device { get; set; }
    public virtual Cabinet? Cabinet { get; set; }
    public virtual ICollection<Pin>? Pins { get; set; }
    public virtual ICollection<DeviceCommand>? DeviceCommands { get; set; }
    public virtual ICollection<ChannelEvent>? ChannelEvents { get; set; }
}
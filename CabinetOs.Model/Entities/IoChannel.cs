using CabinetOs.Core.Model;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Model.Entities;

public class IoChannel : IEntity, ISoftDeletableEntity, IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public int ChannelNumber { get; set; }
    public PinDirection Direction { get; set; }
    public bool IsEnabled { get; set; }
    public string? CurrentValue { get; set; }
    public string Name { get; set; } = null!;
    public DateTime? ValueUpdatedAt { get; set; }

    /// <summary>
    /// Bu kanalın değer değişimleri <see cref="ChannelEvent"/> olarak kalıcı
    /// kaydedilsin mi?
    ///
    /// <b>Varsayılan kapalıdır ve bu bilinçlidir.</b> Bir kabinde onlarca giriş
    /// pini vardır; kullanılmayan uçlar, yedek hatlar ve kurulum sırasında
    /// salınan kanallar da dahil hepsinin geçmişini tutmak, tabloyu okunmaz hale
    /// getirirdi. Hangi pinin anlamlı olduğunu diyagramı çizen operatör söyler —
    /// tıpkı <see cref="Name"/>'in "In7 = Kapı Sensörü" bilgisini vermesi gibi.
    ///
    /// Bayrak açık olsa bile <b>yalnızca <see cref="PinDirection.Input"/>
    /// kanallar</b> olay üretir: bir röleyi biz sürdüğümüzde dönen değer saha
    /// olayı değil, kendi komutumuzun yankısıdır ve kaydı zaten
    /// <see cref="DeviceCommand"/>'dadır.
    /// </summary>
    public bool IsEventLogged { get; set; }

    /// <summary>
    /// Doluysa olay YALNIZCA bu değere geçişte yazılır; <c>null</c> ise her
    /// değişim olaydır.
    ///
    /// Dijital bir girişte tam olarak şu işe yarar: hareket sensöründe
    /// <c>0→1</c> olaydır, <c>1→0</c> ("hareket bitti") çoğu senaryoda
    /// değildir. <c>EventTriggerValue = "1"</c> bunu tek alanla ifade eder.
    ///
    /// Karşılaştırma <c>StringComparison.Ordinal</c>'dır —
    /// <see cref="CurrentValue"/> karşılaştırmasının kullandığı kuralın aynısı.
    /// İkisinin ayrışması "değer değişti ama olay yazılmadı" gibi sessiz bir
    /// tutarsızlık üretirdi.
    /// </summary>
    public string? EventTriggerValue { get; set; }

    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? CreateDateUtc { get; set; }
    public DateTime? UpdateDateUtc { get; set; }
    public string? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedDateUtc { get; set; }
    public virtual Device? Device { get; set; }
    public virtual ICollection<Pin>? Pins { get; set; }
    public virtual ICollection<DeviceCommand>? DeviceCommands { get; set; }
    public virtual ICollection<ChannelEvent>? ChannelEvents { get; set; }
}
using CabinetOs.Core.Model;

namespace CabinetOs.Model.Entities;

/// <summary>
/// Bir giriş kanalında gerçekleşmiş, kalıcı olarak kaydedilmeye DEĞER bulunmuş
/// tek bir değer değişimi.
///
/// <b>Ham telemetri geçmişi DEĞİLDİR.</b> Bu sistemde <c>TelemetryRecord</c>
/// benzeri bir zaman serisi tablosu yoktur ve olmayacaktır; saklanan şey her
/// okuma değil, <i>anlamlı olay</i>tır. Hangi kanalın anlamlı olduğunu
/// <see cref="IoChannel.IsEventLogged"/>, hangi değere geçişin olay sayıldığını
/// <see cref="IoChannel.EventTriggerValue"/> söyler — ikisi de diyagramı çizen
/// operatörün kararıdır.
///
/// Olayın ANLAMI burada saklanmaz: "In7 = 1" satırının "dış kapı hareket
/// algılandı" demek olduğu bilgisi <see cref="IoChannel.Name"/>'den, yani
/// diyagramdan okunur. Adı buraya kopyalamak, kanal yeniden adlandırıldığında
/// sessizce ayrışan bir kopya alan üretirdi.
///
/// Yazan tek yer: <c>ScadaService.IngestAsync</c>.
/// Sözleşme: <c>docs/api-contract/12-channel-events.md</c>
/// </summary>
public class ChannelEvent : IEntity
{
    /// <summary>
    /// IDENTITY PK — sürekli SONA eklenen bir tablo.
    ///
    /// <c>DeviceCommand</c>'ın <c>Guid</c> PK'si düşük hacimli bir tabloda
    /// meşruydu; burada rastgele Guid kümelenmiş indeksi parçalar. Diyagram
    /// kayıtlarındaki "Id'yi istemci üretir" kuralı da geçerli değildir:
    /// bu satırları istemci değil sunucu üretir.
    /// </summary>
    public long Id { get; set; }

    /// <summary>Olayın gerçekleştiği kanal.</summary>
    public Guid IoChannelId { get; set; }

    /// <summary>
    /// Kanalın kabini — <b>kısayol FK</b>.
    ///
    /// <c>Connection.CabinetId</c> ile aynı istisnadır ve aynı üç ölçütü
    /// karşılar: (1) en sıcak sorgu "şu kabinin şu aralıktaki olayları" ve bu
    /// kolon olmadan her seferinde <c>IoChannel → Device</c> iki join gerekirdi,
    /// (2) ayrışması imkânsızdır — bir kanal cihaz değiştirmez, (3) tek yazma
    /// yolu vardır ve o yol kabini zaten elinde tutar.
    /// Ölçütlerden biri düşerse bu kolon da düşer.
    /// </summary>
    public Guid CabinetId { get; set; }

    /// <summary>
    /// Kanalın yeni değeri. Bugün yalnızca <c>"1"</c> / <c>"0"</c> gelir
    /// (kapsam: giriş pinleri).
    ///
    /// <b>Neden <c>bool</c> değil string:</b> <see cref="IoChannel.CurrentValue"/>
    /// string'dir ve analog sensörler kapsama girdiğinde <c>"23.5"</c> aynı
    /// kolona düşecektir. Tipi bugün daraltmak, o gün bir migration ve iki yerde
    /// ayrışan yorumlama demekti.
    /// </summary>
    public string Value { get; set; } = null!;

    /// <summary>
    /// Değişimden önceki değer; kanalın ilk okumasında <c>null</c>.
    /// Kopya alan DEĞİLDİR — başka bir satırın şu anki değerini tekrarlamaz,
    /// geçmiş bir anı dondurur.
    /// </summary>
    public string? PreviousValue { get; set; }

    /// <summary>
    /// Olayın SAHADA gerçekleştiği an — ingest gövdesindeki
    /// <c>timestampUtc</c>. Gönderilmediyse <see cref="ReceivedAtUtc"/> ile eşit
    /// yazılır.
    /// </summary>
    public DateTime OccurredAtUtc { get; set; }

    /// <summary>
    /// Bilginin BİZE ulaştığı an, bizim saatimizle.
    ///
    /// İkisi ayrı tutulur çünkü SCADA'nın saati kayabilir; tek bir zaman damgası
    /// saklansaydı "olay geç mi geldi yoksa saat mi yanlış" sorusu bir daha
    /// cevaplanamazdı.
    /// </summary>
    public DateTime ReceivedAtUtc { get; set; }

    // --- EF Core Navigation ---
    //
    // Yaşam döngüsü arayüzlerinin HİÇBİRİ uygulanmıyor (DiagramAnnotation gibi):
    // bir olay ne pasifleşir ne soft-delete olur. IImmutableEntity de BİLEREK
    // kullanılmıyor — o arayüz silmeyi de engeller ve ileride bir saklama süresi
    // temizliği yazmayı imkânsız kılardı.

    public virtual IoChannel? IoChannel { get; set; }
    public virtual Cabinet? Cabinet { get; set; }
}

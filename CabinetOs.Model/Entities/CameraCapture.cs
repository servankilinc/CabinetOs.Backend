using CabinetOs.Core.Model;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Model.Entities;

/// <summary>
/// Merkeze alınmış TEK bir görüntü — delil kaydı.
///
/// <b>7/24 kayıt DEĞİLDİR.</b> Kabinler ağırlıklı olarak GSM ile bağlanır;
/// sürekli akış kamera başına ~2-4 Mbps'tir ve o hatta sığmaz. Sürekli kayıt
/// kenarda (kameranın SD kartı / NVR) kalır, merkeze yalnızca olay anı çekilir.
///
/// İkili veri DB'de DURMAZ. Bu satır yalnızca indekstir; dosya
/// <see cref="StorageKey"/>'in gösterdiği yerdedir.
///
/// Sözleşme: <c>docs/api-contract/11-camera.md</c>
/// </summary>
public class CameraCapture : IEntity
{
    /// <summary>
    /// IDENTITY PK — <see cref="ChannelEvent.Id"/> ile aynı gerekçe: sürekli
    /// sona eklenen bir tabloda rastgele Guid kümelenmiş indeksi parçalar.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Görüntünün alındığı kamera. Kameranın adı, IP'si ve kabini bu FK
    /// üzerinden okunur — kopyalanmaz.
    ///
    /// İlişki <b>Restrict</b>'tir: delil, kaynağının silinmesini engeller.
    /// </summary>
    public Guid CameraId { get; set; }

    /// <summary>Anlık görüntü mü, klip mi?</summary>
    public CaptureType Type { get; set; }

    /// <summary>
    /// Çekimin akıbeti.
    ///
    /// Ara durum (<c>Pending</c>) kaydedilir çünkü çekim eşzamanlı olmayabilir.
    /// Bu, <c>DeviceCommand</c>'ın kuyruksuz senkron davranışından bilinçli
    /// olarak ayrılır — orada cevap milisaniyeler içinde gelir.
    /// </summary>
    public CaptureStatus Status { get; set; }

    /// <summary>
    /// Görüntünün ANI (UTC). Satırın oluşturulma zamanı değildir: klip olay
    /// öncesini de kapsayabildiği için çekim isteğinden ÖNCE bir an olabilir.
    /// </summary>
    public DateTime CapturedAtUtc { get; set; }

    /// <summary>
    /// Klip süresi (saniye); anlık görüntüde <c>null</c>.
    /// Bitiş zamanı için ayrı kolon yoktur — <c>CapturedAtUtc + DurationSec</c>'tir.
    /// </summary>
    public int? DurationSec { get; set; }

    /// <summary>
    /// Dosyanın depo anahtarı — bugün <c>wwwroot</c> altında göreli yol
    /// (örn: <c>uploads/captures/2026/08/27/{guid}.jpg</c>), şablon
    /// görselleriyle aynı desen.
    ///
    /// <b>Tam URL saklanmaz</b>: şema, host ve kök dizin yapılandırmadan gelir;
    /// her satıra yazılsaydı depo taşındığında binlerce satırın güncellenmesi
    /// gerekirdi. Kolon adının "path" değil <c>StorageKey</c> olması bilinçli —
    /// ileride bir obje deposuna (S3/MinIO) geçmek ŞEMA DEĞİŞTİRMEZ.
    ///
    /// <see cref="CaptureStatus.Pending"/> ve <see cref="CaptureStatus.Failed"/>
    /// iken <c>null</c>'dır.
    /// </summary>
    public string? StorageKey { get; set; }

    /// <summary>Dosya boyutu (byte) — yükleme tamamlanınca dolar. GSM kotası raporu bu kolondan çıkar.</summary>
    public long? SizeBytes { get; set; }

    /// <summary>
    /// <see cref="CaptureStatus.Failed"/> ise sebep: kameraya erişilemedi,
    /// kimlik doğrulama reddedildi, zaman aşımı vb.
    ///
    /// <b>Başarısız çekim de satır bırakır</b> — "o anda görüntü YOK"
    /// bilgisinin kendisi delildir; satırı hiç yazmamak o bilgiyi siler.
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Saklama süresinin sonu (UTC); <c>null</c> ise süresiz.
    ///
    /// Politikadan türetilebilir göründüğü için kopya alan sanılabilir;
    /// değildir: yazma anında sabitlenir ki politika sonradan kısaltıldığında
    /// MEVCUT delilin ömrü geriye dönük değişmesin.
    ///
    /// Bu turda bu kolona bakan bir temizlik işi YOKTUR — saklama politikası
    /// belirlenmeden yazılacak bir temizlik, delil silerdi.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Görüntüyü elle isteyen operatör.
    ///
    /// Referans tasarımda burada dört tetikleyici FK daha vardı
    /// (<c>AlarmId</c>, <c>DeviceCommandId</c>, <c>WorkflowNodeExecutionId</c>,
    /// <c>AccessSessionId</c>). <b>Hiçbiri eklenmedi</b>: üçünün tablosu henüz
    /// yok ve hiçbirini yazacak bir kod yolu bulunmuyor. Bugün tek meşru
    /// tetikleyici operatörün elle çekimidir; diğerleri kendi modülleriyle
    /// birlikte gelir.
    /// </summary>
    public Guid? RequestedByUserId { get; set; }

    // --- EF Core Navigation ---
    //
    // Denetim ve yaşam döngüsü arayüzleri UYGULANMIYOR: delil güncellenmez,
    // pasifleşmez ve soft-delete olmaz. "Kim istedi" bilgisi zaten
    // RequestedByUserId'de, "ne zaman" bilgisi CapturedAtUtc'de duruyor —
    // ayrıca bir CreatedBy/CreateDateUtc çifti onların kopyası olurdu.

    public virtual Camera? Camera { get; set; }
    public virtual User? RequestedByUser { get; set; }
}

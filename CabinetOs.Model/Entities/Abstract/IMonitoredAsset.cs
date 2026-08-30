using CabinetOs.Core.Model;

namespace CabinetOs.Model.Entities.Abstract;

/// <summary>
/// Ağ üzerinden yoklanarak (ping / TCP connect) ayakta olup olmadığı anlaşılan
/// varlıkların ortak sözleşmesi. Bugün tek uygulayıcısı <see cref="Camera"/>.
///
/// <b>Neden tek bir "MonitoredDevice" tablosu YOK.</b> Kamera, POS cihazı ve
/// benzerleri tek bir tabloda birleştirilseydi her yeni tipin alanları o tabloya
/// nullable olarak eklenirdi ve tablo, hiçbir satırın hepsini doldurmadığı bir
/// alan yığınına dönerdi. Bunun yerine <b>her tip kendi tablosunu</b> alır; bu
/// arayüz yalnızca ortak olanı garanti eder.
///
/// EF Core arayüzleri map ETMEZ — burada tanımlı her özelliği somut entity
/// kendi kolonu olarak taşır. Kalıtım (TPH) bilinçli olarak kullanılmıyor:
/// EF kalıtımı gördüğü anda tek tabloya indirger, ki bu tam olarak reddedilen
/// çözümdür.
///
/// Diyagramdaki <c>Device</c> ile karıştırılmamalı: <c>Device</c> pinli ve
/// kablolanabilirdir, verisi SCADA üzerinden gelir. Buradaki varlıklar
/// diyagramda yer almaz ve SCADA'nın dünyasında değildir — onları bu platform
/// kendisi yoklar.
/// </summary>
public interface IMonitoredAsset : IEntity
{
    Guid Id { get; }

    /// <summary>Fiziksel olarak bulunduğu kabin.</summary>
    Guid CabinetId { get; set; }

    /// <summary>Operatörün gördüğü ad (örn: "Ana Giriş Kamerası").</summary>
    string Name { get; set; }

    /// <summary>
    /// Kabin içi LAN adresi. Kabinin kendi dış erişim adresleri
    /// (<c>Cabinet.GsmIp</c> / <c>Cabinet.NetworkIp</c>) BU İŞ İÇİN
    /// KULLANILMAZ — onlar kabine ulaşmak içindir, kabin içindeki cihaza değil.
    /// </summary>
    string IpAddress { get; set; }

    /// <summary>
    /// Yoklama sondasının bağlanacağı TCP portu; saf ICMP ping kullanılacaksa
    /// <c>null</c>.
    ///
    /// Neden var: kamerada anlamlı sonda ICMP değil, servis portuna TCP
    /// connect'tir. Ağ kartı ayakta ama RTSP servisi ölmüşse ping "Online" der
    /// ve operatörü yanlış teşhise götürür.
    /// </summary>
    int? MonitoringPort { get; set; }

    /// <summary>
    /// Anlık durum — <c>DeviceStatus</c> lookup tablosuna FK.
    ///
    /// Kameraya özel AYRI BİR DURUM SÖZLÜĞÜ AÇILMAZ: Online/Offline/Warning/
    /// Critical/Maintenance aynı anlamları taşır ve kabin rozeti hesabı
    /// (cihazların en kötüsü) böylece tek sözlük üzerinden yürüyebilir.
    ///
    /// Hiç yoklanmamış varlıkta <c>null</c>'dır — "Offline" ile "bilgim yok"
    /// aynı şey değildir.
    /// </summary>
    int? DeviceStatusId { get; set; }

    /// <summary>Son BAŞARILI yoklamanın zamanı (UTC).</summary>
    DateTime? LastSeen { get; set; }

    /// <summary>
    /// İki yoklama arasındaki hedef aralık (saniye).
    ///
    /// Yoklamayı yapan servis bu turda YAZILMIYOR (kullanıcı kararı); alan,
    /// o servis geldiğinde zamanlamanın koda değil veriye bağlı olması için
    /// şimdiden burada duruyor — varlık başına farklı aralık istenebilir.
    /// </summary>
    int PingIntervalSec { get; set; }

    /// <summary>
    /// İzleme açık mı? Kapalıyken yoklayıcı bu satırı atlar ve durum olduğu
    /// gibi kalır. <c>IsActive</c>'den ayrıdır: kayıt aktif olabilir ama
    /// (bakımdayken) yoklanmıyor olabilir.
    /// </summary>
    bool IsMonitoringEnabled { get; set; }

    /// <summary>
    /// Son başarısız yoklamanın sebebi; başarılı yoklamada temizlenir.
    /// "Offline" tek başına teşhis ettirmez — sebep metni ettirir.
    /// </summary>
    string? LastConnectionError { get; set; }
}

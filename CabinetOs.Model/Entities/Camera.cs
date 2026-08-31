using CabinetOs.Core.Model;
using CabinetOs.Model.Entities.Abstract;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Model.Entities;

/// <summary>
/// Kabin içindeki IP kamera. Bugün hedeflenen donanım Hikvision
/// <c>DS-2CD1123G0-IUF</c>'tir, ama hiçbir satıcıya özel değer koda gömülü
/// değildir — hepsi bu tablodan gelir.
///
/// Diyagramdaki <c>Device</c>'tan AYRIDIR: pini ve kablosu yoktur, diyagramda
/// çizilmez ve verisi SCADA üzerinden gelmez. Bu platform onu kendisi yoklar.
/// Ortak izleme alanlarının sözleşmesi <see cref="IMonitoredAsset"/>'tedir.
///
/// <b>İki ayrı erişim yolu vardır ve ikisi de bu satırdan türetilir:</b>
/// <list type="bullet">
/// <item>Anlık görüntü — ISAPI:
/// <c>http://{IpAddress}:{HttpPort}/ISAPI/Streaming/channels/{SnapshotChannel}/picture</c></item>
/// <item>Canlı yayın — RTSP (yalnızca medya geçidine verilir, tarayıcıya ASLA):
/// <c>rtsp://{Username}:{Password}@{IpAddress}:{RtspPort}/Streaming/Channels/{kanal}</c></item>
/// </list>
///
/// <b>Bu URL'ler kolon DEĞİLDİR</b> ve saklanmaz. Tam URL saklamak, içindeki
/// host ve port parçalarını <see cref="IpAddress"/> / port kolonlarının kopyası
/// haline getirirdi: biri portu değiştirip URL'i güncellemeyi unuttuğunda ikisi
/// sessizce ayrışır ve hangisinin doğru olduğu belirsizleşirdi. Aynı sebeple
/// medya geçidindeki yol adı da saklanmaz, <c>Id</c>'den türetilir.
///
/// Sözleşme: <c>docs/api-contract/11-camera.md</c>
/// </summary>
public class Camera : IEntity, IAuditableEntity, IActivatableEntity, IMonitoredAsset
{
    public Guid Id { get; set; }

    // ─── Kimlik / metadata ───

    public Guid CabinetId { get; set; }

    public string Name { get; set; } = null!;

    /// <summary>Kurulum notu — "giriş kapısı, dışa bakan" gibi.</summary>
    public string? Description { get; set; }

    /// <summary>
    /// Üretici (örn: "Hikvision"). Kolon olarak duruyor çünkü ISAPI yolu ve
    /// RTSP yol şablonu üreticiye göre değişir; ikinci bir marka geldiğinde
    /// ayrım koda değil veriye bakarak yapılabilsin.
    /// </summary>
    public string? Manufacturer { get; set; }

    /// <summary>Model kodu (örn: "DS-2CD1123G0-IUF") — envanter ve destek için.</summary>
    public string? Model { get; set; }

    // ─── Ağ ───

    /// <inheritdoc/>
    public string IpAddress { get; set; } = null!;

    /// <summary>RTSP servis portu — fabrika varsayılanı 554.</summary>
    public int RtspPort { get; set; }

    /// <summary>ISAPI/web arayüzü portu — fabrika varsayılanı 80.</summary>
    public int HttpPort { get; set; }

    /// <summary>HTTPS portu; kamerada TLS kapalıysa <c>null</c>.</summary>
    public int? HttpsPort { get; set; }

    // ─── Erişim bilgileri ───

    /// <summary>Kamera web arayüzü kullanıcısı.</summary>
    public string? Username { get; set; }

    /// <summary>
    /// Kamera parolası.
    ///
    /// <b>Düz metin saklanır ve okuma DTO'sunda düz metin döner</b> (kullanıcı
    /// kararı): sistem kapalı ağda çalışıyor ve bu aşamada kamera kimlik
    /// bilgilerinin gizlenmesi istenmiyor. Araya bir koruma/şifreleme katmanı
    /// KONULMAMIŞTIR — atama doğrudandır.
    ///
    /// <b>Sonucu açıkça:</b> veritabanı yedeğini veya API cevabını eline geçiren,
    /// sahadaki tüm kameraların parolasını da alır. Geri almak istendiğinde
    /// değişecek yerler: bu alanın yazıldığı iki satır
    /// (<c>CameraService.CreateAsync</c> / <c>UpdateAsync</c>) ve
    /// <c>CameraService.Projection</c>.
    ///
    /// RTSP adresi yine tarayıcıya GİTMEZ — canlı izleme kısa ömürlü bir bilet
    /// üzerinden, medya geçidi aracılığıyla yapılır.
    /// </summary>
    public string? Password { get; set; }

    // ─── Akış ───

    /// <summary>
    /// Ana akım kanal numarası (Hikvision'da tipik olarak 101).
    /// Sabit varsayılmaz: NVR arkasındaki bir kamerada kanal numarası değişir.
    /// </summary>
    public int MainStreamChannel { get; set; }

    /// <summary>Tali akım kanal numarası (tipik olarak 102).</summary>
    public int SubStreamChannel { get; set; }

    /// <summary>Ana akım kullanılabilir mi?</summary>
    public bool MainStreamEnabled { get; set; }

    /// <summary>
    /// Tali akım kullanılabilir mi? Kapalıysa liste ekranı da ana akımı açmak
    /// zorunda kalır — bant genişliği açısından bilinçli bir tercih olmalıdır.
    /// </summary>
    public bool SubStreamEnabled { get; set; }

    // Kodek KOLON DEĞİLDİR. Kameralar sahada H.264'e ayarlanır ve transcoding
    // bilinçli olarak yapılmıyor; dolayısıyla saklanacak bir seçenek yok.
    // Kamera H.265 yayınlarsa tarayıcı çözemez ve görüntü siyah kalır — teşhis
    // kameranın kendi arayüzünden yapılır. Bir kolon tutmak, kod hiçbir yerde
    // ona bakmadığı için yalnızca yanlış olabilecek bir kopya üretirdi.

    // ─── Anlık görüntü ───

    /// <summary>
    /// ISAPI snapshot kanal numarası. Genellikle ana akımla aynıdır ama ayrı
    /// tutulur: bazı kurulumlarda anlık görüntü tali akımdan alınır (daha hızlı,
    /// daha küçük dosya).
    /// </summary>
    public int SnapshotChannel { get; set; }

    // ─── İzleme (IMonitoredAsset) ───

    /// <inheritdoc/>
    public int? MonitoringPort { get; set; }

    /// <inheritdoc/>
    public int? DeviceStatusId { get; set; }

    /// <inheritdoc/>
    public DateTime? LastSeen { get; set; }

    /// <inheritdoc/>
    public int PingIntervalSec { get; set; }

    /// <inheritdoc/>
    public bool IsMonitoringEnabled { get; set; }

    /// <inheritdoc/>
    public string? LastConnectionError { get; set; }

    // ─── Denetim / yaşam döngüsü ───
    //
    // IActivatableEntity: fiziksel silme EntityLifecycleInterceptor'da exception
    // atar. Zaten atmalı — CameraCapture bu satıra Restrict ile bağlı ve delil,
    // kaynağının silinmesini engellemelidir.

    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? CreateDateUtc { get; set; }
    public DateTime? UpdateDateUtc { get; set; }
    public bool IsActive { get; set; }

    // --- EF Core Navigation ---
    public virtual Cabinet? Cabinet { get; set; }
    public virtual DeviceStatus? DeviceStatus { get; set; }
    public virtual ICollection<CameraCapture>? Captures { get; set; }
}

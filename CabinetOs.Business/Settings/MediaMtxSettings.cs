namespace CabinetOs.Business.Settings;

/// <summary>
/// Medya gecidi (MediaMTX) yapilandirmasi — <c>appsettings.json</c>'daki
/// <c>Mediamtx</c> bolumu.
///
/// <b><c>IOptions&lt;T&gt;</c> KULLANILMIYOR.</b> Kod tabaninda hicbir yerde
/// yok; ayarlar <c>TokenSettings</c> / <c>CacheSettings</c> gibi baglanip
/// singleton olarak kaydediliyor. Tek bir yerde farkli desen acmak, "hangisi
/// dogru" sorusunu kalici hale getirirdi.
/// </summary>
public class MediaMtxSettings
{
    public const string SectionName = "Mediamtx";

    /// <summary>
    /// Control API adresi. Yalnizca SUNUCU kullanir; loopback olmasi beklenir —
    /// bu uc kimlik dogrulamasizdir ve disariya acilmamalidir.
    /// </summary>
    public string ApiBaseUrl { get; set; } = "http://127.0.0.1:9997";

    /// <summary>
    /// WebRTC/WHEP adresi. <b>TARAYICININ</b> ulasacagi adrestir, sunucunun
    /// degil: bilet cevabindaki <c>whepUrl</c> bundan uretilir ve istemci
    /// dogrudan buraya baglanir.
    ///
    /// Sunucu ile tarayici ayri makinedeyse <c>127.0.0.1</c> CALISMAZ —
    /// sunucunun LAN adresi yazilmalidir. Dagitimda ilk kirilan yer burasi.
    /// </summary>
    public string WebRtcPublicBaseUrl { get; set; } = "http://127.0.0.1:8889";

    /// <summary>
    /// Biletin omru (saniye). Kisa tutuluyor: bilet yalnizca el sikisma aninda
    /// kullanilir, uzun omur yalnizca calinma penceresini buyuturdu.
    /// </summary>
    public int TokenTtlSeconds { get; set; } = 60;

    /// <summary>
    /// Son izleyici ayrildiktan sonra MediaMTX'in kameraya olan RTSP oturumunu
    /// kapatmadan once bekledigi sure. Sekme yenilemede oturumun bastan
    /// kurulmasini engeller.
    /// </summary>
    public string SourceOnDemandCloseAfter { get; set; } = "10s";

    /// <summary>
    /// Kameraya baglanirken kullanilacak RTSP tasima katmani. <c>tcp</c>:
    /// UDP paket kaybi H.264 akisinda kalici bozulma birakir.
    /// </summary>
    public string RtspTransport { get; set; } = "tcp";

    /// <summary>
    /// MediaMTX'in klip segmentlerini yazdigi gecici kok dizin.
    ///
    /// <b>MediaMTX ile bu uygulama ayni dosya sistemini gormek ZORUNDA</b> —
    /// klip akisi, MediaMTX'in yazdigi dosyayi bu uygulamanin okumasina dayanir.
    /// Ikisi ayri makinede calisacaksa klip cekimi calismaz.
    /// </summary>
    public string RecordRoot { get; set; } = "";
}

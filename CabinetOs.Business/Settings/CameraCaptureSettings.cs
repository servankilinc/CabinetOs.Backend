namespace CabinetOs.Business.Settings;

/// <summary>
/// Anlik goruntu ve cekim yapilandirmasi — <c>appsettings.json</c>'daki
/// <c>Cameras</c> bolumu.
/// </summary>
public class CameraCaptureSettings
{
    public const string SectionName = "Cameras";

    /// <summary>
    /// Kameradan anlik goruntu beklerken uygulanan zaman asimi.
    /// Geciti kendi <c>CancellationTokenSource</c>'uyla uygular
    /// (bkz. <c>IsapiSnapshotGateway</c>) — <c>HttpClient.Timeout</c> ile
    /// "kamera yavas" ile "istek iptal edildi" ayirt edilemezdi.
    /// </summary>
    public int SnapshotTimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Anlik goruntunun onbellekte tutulma suresi. Kisa ama sifir degil: ayni
    /// kameranin goruntusunu es zamanli isteyen birden fazla istemci kameraya
    /// tek istek uretsin diye.
    /// </summary>
    public int SnapshotCacheSeconds { get; set; } = 3;

    /// <summary>
    /// Cekim dosyalarinin <c>wwwroot</c> altindaki koku — sablon gorselleriyle
    /// ayni desen (<c>TemplateImageStore</c>).
    /// </summary>
    public string CaptureRoot { get; set; } = "uploads/captures";

    /// <summary>
    /// <c>CameraCapture.ExpiresAt</c>'in yazma aninda hesaplanacagi saklama
    /// suresi. <c>0</c> ise sinirsiz (<c>ExpiresAt = null</c>).
    ///
    /// Deger yazma aninda SABITLENIR: politika sonradan kisaltildiginda mevcut
    /// delilin omru geriye donuk degismesin (bkz. <c>CameraCapture.ExpiresAt</c>).
    /// <b>Bu kolona bakan bir temizlik isi hala YOKTUR</b> — saklama politikasi
    /// netlesmeden yazilacak bir temizlik, delil silerdi.
    /// </summary>
    public int CaptureRetentionDays { get; set; } = 30;

    /// <summary>
    /// Klip suresinin ust siniri. Sinirsiz birakmak, tek bir istekle diski
    /// doldurmayi mumkun kilardi.
    /// </summary>
    public int MaxClipDurationSec { get; set; } = 120;

    /// <summary>
    /// Kayit suresinin uzerine eklenen pay: MediaMTX'in kameraya baglanmasi,
    /// ilk anahtar kareyi beklemesi ve segmenti kapatmasi zaman alir.
    /// </summary>
    public int ClipFinalizeGraceMs { get; set; } = 3000;

    // Grid'in es zamanli akim tavani BURADA DEGIL, istemcide
    // (Frontend/src/lib/camera/stream-budget.ts). Sunucunun onu okuyan bir
    // kod yolu yok; buraya konsaydi hicbir yerden servis edilmeyen bir ayar
    // olurdu ve "degistirdim ama bir sey olmadi" derdi.
}

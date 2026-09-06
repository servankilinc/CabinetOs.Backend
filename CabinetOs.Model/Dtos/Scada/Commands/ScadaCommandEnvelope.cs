using CabinetOs.Core.Model;
using CabinetOs.Model.Enums;

namespace CabinetOs.Model.Dtos.Scada.Commands;

// DIKKAT: `using static EntityEnums` bu AD ALANINDA riskli. `CabinetOs.Model.Dtos`
// altinda `DeviceStatus` adinda bir AD ALANI var ve kisa adlar once ona cozuluyor;
// komsu ScadaIngestRequest.cs tam bu yuzden enum'lari acikca niteliyor. Ayni
// kurala uyuluyor.

/// <summary>
/// CabinetOS'un SCADA'ya GONDERDIGI kumanda govdesi — ingest'in ters yonu.
///
/// <b>Kimlikler ingest ile ayni dilde.</b> Guid tasimayiz: SCADA bizim
/// Id'lerimizi bilmez. Kabin BIR kontrol kartidir ve kartin adres uzayi duzdur,
/// dolayisiyla <c>cabinetId</c> + <c>pin</c> hedefi tek basina belirler — tipki
/// ingest'in <c>"IN1"</c> gondermesi gibi, burada <c>"OUT5"</c> gider
/// (bkz. 07-scada-ingest.md).
///
/// <b>Eski <c>externalCode</c> ve <c>channelNumber</c> alanlari kalkti.</b>
/// Protokolde modul bayti yok; kart kimligi soketin kendisi. Buna bagli olarak
/// <c>DeviceCommandService</c>'teki "dis kodu olmayan cihaza kumanda
/// gonderilemez" on kosulu da kalkti — dayanagi kalmamisti.
///
/// Tek istisna <see cref="CommandId"/>'dir: bizim satirimizin Id'sidir ve SCADA
/// tarafinda TEKRAR TESPITI icin tasinir. Retry yapmiyoruz, ama sebeke seviyesinde
/// tekrarlanan bir paketin roleyi iki kez surmemesi SCADA'nin elindedir ve bunu
/// ancak degismeyen bir kimlikle yapabilir.
///
/// Sozlesme: <c>docs/api-contract/08-scada-command.md</c>
/// </summary>
public class ScadaCommandEnvelope : IDto
{
    /// <summary><c>DeviceCommand.Id</c> — SCADA tarafinda tekrar tespiti icin.</summary>
    public Guid CommandId { get; set; }

    public Guid CabinetId { get; set; }

    /// <summary>
    /// Hedef nokta — <c>"OUT5"</c>, <c>"OUT17"</c> (LED). Ingest'in <c>"IN1"</c>
    /// bicimiyle ayni dil; uretimi <c>ScadaPinAddress.Format</c>'ta.
    ///
    /// Kumanda tanim geregi bir CIKISI hedefler: giris yonlu kanal komut yolunda
    /// zaten 400 ile reddediliyor. Kartta role ve LED ayni duz cikis uzayini
    /// paylastigi icin (role 1-16, LED 17-24) numara tek basina yeterlidir.
    /// </summary>
    public string Pin { get; set; } = null!;

    public EntityEnums.DeviceCommandType CommandType { get; set; }

    /// <summary>
    /// Telemetriyle ayni sekilde STRING; kanal basina tip yoktur.
    ///
    /// <b>Bu deger istemciden GELMEZ, sunucu uretir.</b> Istemci niyeti bildirir
    /// (<c>turnOn</c>); NO/NC kutbuna gore telde gidecek <c>"1"</c>/<c>"0"</c>'i
    /// <c>DeviceCommandService</c> cozer. SCADA'nin NO/NC'den haberi olmasina
    /// gerek yok — ceviri bizde biter.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>Komutun SUNUCUDA olustugu an. SCADA'nin saatine guvenilmez.</summary>
    public DateTime IssuedAtUtc { get; set; }
}

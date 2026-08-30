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
/// Id'lerimizi bilmez, cihazi <c>externalCode</c> ve kanali <c>channelNumber</c>
/// ile tanir (bkz. 07-scada-ingest.md). Bunun dogrudan bir sonucu var ve
/// <c>DeviceCommandService</c> onu on kontrol olarak uyguluyor: <b>dis kodu
/// olmayan bir cihaza kumanda gonderilemez</b> — paletten yeni birakilmis, henuz
/// SCADA ile eslesmemis bir cihaz komut alamaz.
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

    /// <summary><c>Device.ExternalCode</c> — SCADA'nin cihazi tanidigi kod.</summary>
    public string ExternalCode { get; set; } = null!;

    /// <summary>
    /// Hedef kanal. Tek kumanda turu (<c>SetOutput</c>) her zaman bir kanali
    /// hedefledigi icin pratikte hep doludur; tipi <c>int?</c> birakildi, cunku
    /// daraltmak SCADA'ya giden tel sozlesmesini degistirirdi.
    /// </summary>
    public int? ChannelNumber { get; set; }

    public EntityEnums.DeviceCommandType CommandType { get; set; }

    /// <summary>Telemetriyle ayni sekilde STRING; kanal basina tip yoktur.</summary>
    public string? Value { get; set; }

    /// <summary>Komutun SUNUCUDA olustugu an. SCADA'nin saatine guvenilmez.</summary>
    public DateTime IssuedAtUtc { get; set; }
}

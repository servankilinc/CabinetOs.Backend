using CabinetOs.Core.Model;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Model.Dtos.DeviceCommand.Queries;

/// <summary>
/// Bir kumandanin SONUCU — hem <c>POST /api/Device/{id}/command</c> yaniti hem
/// <c>GET /api/Device/{id}/commands</c> satiri.
///
/// Iki ucun AYNI sekli dondurmesi bilerekdir: arayuzdeki komut gecmisi listesi,
/// az once gonderilen komutu yeniden sorgulamadan basina ekleyebilsin diye.
///
/// Sozlesme: <c>docs/api-contract/08-scada-command.md</c>
/// </summary>
public class DeviceCommandResultDto : IDto
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }

    /// <summary>
    /// Kumandanin hedefledigi kanal. Gonderim yolunda artik her zaman doludur;
    /// tipi <c>Guid?</c> kaldi cunku DB kolonu nullable ve daraltmak okuma
    /// sozlesmesini degistirirdi.
    /// </summary>
    public Guid? IoChannelId { get; set; }

    /// <summary>
    /// Kanalin numarasi. Id'ye ek olarak tasinir cunku gecmis listesi kanal
    /// nesnesini yeniden cozmeden "CH3" yazabilmeli.
    /// </summary>
    public int? ChannelNumber { get; set; }

    public DeviceCommandType CommandType { get; set; }

    /// <summary>Gonderilen payload (<c>{"value":"1"}</c>).</summary>
    public string? PayloadJson { get; set; }

    /// <summary>
    /// SCADA'nin cevabinin sonucu. <c>Sent</c> BU GOVDEDE GORULMEZ: satir ancak
    /// cevap islendikten sonra donuyor. Gecici <c>Sent</c> durumu yalnizca
    /// veritabaninda, istek ucusta iken var olur.
    /// </summary>
    public CommandStatus Status { get; set; }

    /// <summary>
    /// Basarisizlikta SCADA'nin govdesi (kirpilmis), zaman asiminda sure bilgisi.
    /// Basarida genellikle NULL. Operatore gosterilecek tek teshis metni budur.
    /// </summary>
    public string? ResultMessage { get; set; }

    public DateTime? SentAt { get; set; }
    public DateTime? RespondedAt { get; set; }

    /// <summary>
    /// SCADA'nin cevap suresi. <see cref="SentAt"/>/<see cref="RespondedAt"/>
    /// farkindan turetilir; istemcinin ayni hesabi yapmasina gerek kalmasin diye
    /// tasinir ve iki ucta AYNI sekilde hesaplanir.
    /// </summary>
    public int? ElapsedMs { get; set; }

    public Guid? RequestedByUserId { get; set; }

    /// <summary>Komutu isteyen kullanicinin gorunen adi — canli yayinda "kim yapti".</summary>
    public string? RequestedByName { get; set; }
}

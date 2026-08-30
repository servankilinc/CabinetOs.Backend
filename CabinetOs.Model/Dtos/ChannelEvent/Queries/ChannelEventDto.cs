using CabinetOs.Core.Model;

namespace CabinetOs.Model.Dtos.ChannelEvent.Queries;

/// <summary>
/// Tek bir kanal olayinin okuma sekli.
///
/// <b>Olayin ANLAMI diyagramdan gelir.</b> <see cref="ChannelName"/> saklanan bir
/// kopya degil, <c>IoChannel.Name</c>'in okuma anindaki halidir — operator kanali
/// "Kapi Sensoru" diye yeniden adlandirdiginda gecmis olaylar da yeni adla gorunur.
/// Adi olay satirina yazmak, kanal yeniden adlandirildiginda sessizce ayrisan bir
/// kopya alan uretirdi.
///
/// Sozlesme: <c>docs/api-contract/12-channel-events.md</c>
/// </summary>
public class ChannelEventDto : IDto
{
    public long Id { get; set; }
    public Guid IoChannelId { get; set; }
    public Guid CabinetId { get; set; }

    // Asagidaki dort alan TUREVDIR ve hepsi NULL OLABILIR.
    //
    // Sebep: IoChannel soft-delete tasiyor ve global query filter'i var. Kanal
    // silindiginde olay satiri DURUR — silinmis bir kanalin gecmisi de delildir —
    // ama adi cozulemez. Bu alanlari zorunlu yapmak, gecmisi okunamaz kilardi.
    // null gelmesi "kaynak kanal silinmis" demektir ve arayuz bunu boyle gosterir.

    /// <summary>Kanalin diyagramdaki adi — "In7" degil "Kapi Sensoru".</summary>
    public string? ChannelName { get; set; }

    /// <summary>Kanal numarasi — SCADA ile ortak dil.</summary>
    public int? ChannelNumber { get; set; }

    /// <summary>Kanalin bagli oldugu cihaz ve SCADA tarafindaki kodu.</summary>
    public Guid? DeviceId { get; set; }
    public string? DeviceName { get; set; }
    public string? DeviceExternalCode { get; set; }

    public string Value { get; set; } = null!;
    public string? PreviousValue { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public DateTime ReceivedAtUtc { get; set; }
}

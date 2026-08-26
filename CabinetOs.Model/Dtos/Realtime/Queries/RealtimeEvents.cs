using CabinetOs.Core.Model;
using CabinetOs.Model.Enums;

namespace CabinetOs.Model.Dtos.Realtime.Queries;

// DIKKAT: `using static EntityEnums` BURADA CALISMAZ. `CabinetOs.Model.Dtos`
// altinda `DeviceStatus` adinda bir AD ALANI var (lookup entity'sinin DTO'lari);
// kisa ad once ona cozuluyor. Enum bu yuzden acikca nitelenir.

/// <summary>
/// <c>/hubs/diagram</c> uzerinden SUNUCUDAN ISTEMCIYE giden olaylarin govdeleri.
///
/// Bu tipler bir sozlesmedir: alan adlari SignalR uzerinden aynen gider ve
/// frontend'de <c>src/models/realtime/**</c> aynasi tarafindan okunur.
/// Degistirmek, derleyicinin uyarmadigi bir kirilma uretir.
///
/// <b>Yalnizca DEGISENLER yayinlanir.</b> Ingest'te degeri ayni kalan kanal bu
/// olaylardan hicbirini uretmez — aksi halde 500 kanalli bir kabinde saniyede bir
/// ingest, hicbir sey degismese bile saniyede 500 guncelleme yayardi.
///
/// Sozlesme: <c>docs/api-contract/09-realtime.md</c>
/// </summary>
public class ChannelValueChange : IDto
{
    public Guid IoChannelId { get; set; }
    /// <summary>Kanalin bagli oldugu cihaz — istemci node bazinda yeniden cizebilsin diye.</summary>
    public Guid DeviceId { get; set; }
    public int ChannelNumber { get; set; }
    public string? Value { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class DeviceStatusChange : IDto
{
    public Guid DeviceId { get; set; }
    /// <summary>Null = hic telemetri alinmadi. <c>Offline</c> ile AYNI SEY DEGIL.</summary>
    public EntityEnums.DeviceStatus? StatusId { get; set; }
    public DateTime? LastSeen { get; set; }
}

public class CabinetStatusChange : IDto
{
    public Guid CabinetId { get; set; }
    public EntityEnums.DeviceStatus? StatusId { get; set; }
    public DateTime? LastSeen { get; set; }
    public DateTime? ScadaLastIngestAt { get; set; }
}

/// <summary>
/// Bir kumandanin sonuclandigini bildirir (S2).
///
/// Ustteki uc olaydan FARKLI bir sebeple var: onlar sahadan gelen degisimi
/// tasir, bu ise BIR KULLANICININ yaptigi isi tasir. Komutu gonderen zaten
/// HTTP yanitinda sonucu aliyor; bu yayin ayni kabini izleyen DIGER
/// kullanicilar icin. Yayinlanmasaydi, iki operatorden biri roleyi surerken
/// otekinin ekraninda hicbir sey olmazdi ve ikisi de ayni cikisi surmeye
/// calisirdi.
///
/// Kanal DEGERI bu olayla gelmez: SCADA komutu uyguladiginda degisen deger
/// normal ingest yoluyla <see cref="ChannelValueChange"/> olarak zaten gelir.
/// Ikisini birlestirmek, SCADA'nin "kabul ettim" demesini "uyguladim" saymak
/// olurdu — kabul, uygulama degildir.
/// </summary>
public class CommandCompleted : IDto
{
    public Guid CommandId { get; set; }
    public Guid DeviceId { get; set; }
    public Guid? IoChannelId { get; set; }
    public int? ChannelNumber { get; set; }
    public EntityEnums.DeviceCommandType CommandType { get; set; }
    public EntityEnums.CommandStatus Status { get; set; }
    public string? ResultMessage { get; set; }
    public DateTime? RespondedAt { get; set; }
    /// <summary>Komutu isteyen kullanicinin gorunen adi — Guid degil.</summary>
    public string? RequestedByName { get; set; }
}

using CabinetOs.Core.Model;
using FluentValidation;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Model.Dtos.DeviceCommand.Commands;

/// <summary>
/// <c>POST /api/Device/{deviceId}/command</c> govdesi — kumanda istegi.
///
/// <b>Payload TIPLIDIR, ham JSON string DEGIL.</b> Entity'deki
/// <c>DeviceCommand.PayloadJson</c> bir string kolonudur ve istemcinin dogrudan o
/// stringi gondermesi daha az kod olurdu; yapilmadi cunku o sekilde govdenin
/// SAHAYA NE GONDERDIGI dogrulanamaz — istemcinin yazdigi metin oldugu gibi
/// role suren bir sisteme gecerdi. Sunucu payload'i bu alanlardan KENDISI kurar,
/// boylece veritabanindaki metin ile tel uzerindeki metin ayni ve dogrulanmis olur.
///
/// Sozlesme: <c>docs/api-contract/08-scada-command.md</c>
/// </summary>
public class DeviceCommandSendRequest : IDto
{
    public DeviceCommandType CommandType { get; set; }

    /// <summary>
    /// Hedef kanal — ZORUNLU. Tek kumanda turu <see cref="DeviceCommandType.SetOutput"/>
    /// oldugu ve o da her zaman bir cikis kanalini hedefledigi icin modul geneline
    /// giden, kanalsiz bir kumanda artik yoktur.
    /// </summary>
    public Guid? IoChannelId { get; set; }

    /// <summary>
    /// NIYET — ZORUNLU. <c>true</c> = "yuku ver", <c>false</c> = "yuku kes".
    ///
    /// <b>Neden ham deger degil.</b> Karta giden bayt rolenin BOBININI surer;
    /// yukun devresinin kapanip kapanmadigi ise yukun <b>NO</b> mu <b>NC</b>
    /// kontaga kablolandigina baglidir. Yani "ac" ile telde giden
    /// <c>"1"</c>/<c>"0"</c> arasindaki esleme sabit DEGILDIR. Eskiden bu alan
    /// ham bir string'di ve karari operatore yikiyordu: NC kabloli bir rolede
    /// "ac" demek icin <c>"0"</c> gondermesi gerektigini bilmek zorundaydi.
    ///
    /// Alan <b>yukun</b> durumunu ifade eder, bobinin degil — operatorun
    /// diyagramda gordugu sey yuktur. Telde gidecek degeri sunucu
    /// <c>Pin.Function</c>'daki NO/NC'den cozer; bu, bu DTO'nun zaten uyguladigi
    /// "payload'i sunucu kurar" ilkesinin dogal devamidir.
    /// </summary>
    public bool? TurnOn { get; set; }
}

public class DeviceCommandSendRequestValidator : AbstractValidator<DeviceCommandSendRequest>
{
    public DeviceCommandSendRequestValidator()
    {
        RuleFor(v => v.CommandType).IsInEnum().WithMessage("Geçersiz komut türü");

        // Tek kumanda turu kaldigi icin hedef ve niyet KOSULSUZ zorunlu. Once
        // komut turune bakan bir ayrim vardi; artik ayrilacak bir sey yok.
        RuleFor(v => v.IoChannelId).NotEmpty()
            .WithMessage("Kumanda için hedef kanal zorunlu");

        // NotEmpty KULLANILAMAZ: bool? uzerinde false'u da bos sayar ve "kapat"
        // komutunu reddederdi.
        RuleFor(v => v.TurnOn).NotNull()
            .WithMessage("Kumanda için aç/kapat bilgisi zorunlu");
    }
}

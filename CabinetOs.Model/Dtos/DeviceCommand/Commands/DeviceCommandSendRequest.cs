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
    /// Yazilacak deger — ZORUNLU. Telemetriyle ayni sekilde STRING tasinir; kanal
    /// basina tip yoktur: role icin <c>"1"</c> / <c>"0"</c>.
    /// </summary>
    public string? Value { get; set; }
}

public class DeviceCommandSendRequestValidator : AbstractValidator<DeviceCommandSendRequest>
{
    private const int MaxValueLength = 64;

    public DeviceCommandSendRequestValidator()
    {
        RuleFor(v => v.CommandType).IsInEnum().WithMessage("Geçersiz komut türü");

        // Tek kumanda turu kaldigi icin hedef ve deger KOSULSUZ zorunlu. Once
        // komut turune bakan bir ayrim vardi; artik ayrilacak bir sey yok.
        RuleFor(v => v.IoChannelId).NotEmpty()
            .WithMessage("Kumanda için hedef kanal zorunlu");

        RuleFor(v => v.Value).NotEmpty()
            .WithMessage("Kumanda için değer zorunlu");

        RuleFor(v => v.Value).MaximumLength(MaxValueLength)
            .WithMessage($"Değer en fazla {MaxValueLength} karakter olabilir");
    }
}

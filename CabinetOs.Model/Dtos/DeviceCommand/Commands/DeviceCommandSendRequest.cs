using CabinetOs.Core.Model;
using FluentValidation;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Model.Dtos.DeviceCommand.Commands;

/// <summary>
/// <c>POST /api/Device/{deviceId}/command</c> govdesi — kumanda istegi.
///
/// <b>Payload TIPLIDIR, ham JSON string DEGIL.</b> Entity'deki
/// <c>DeviceCommand.PayloadJson</c> bir string kolonudur ve istemcinin dogrudan o
/// stringi gondermesi daha az kod olurdu; yapilmadi cunku o sekilde
/// <c>PulseOutput</c>'un sure tasidigi DOGRULANAMAZ. Suresiz bir darbe komutu ya
/// hicbir sey yapmaz ya da roleyi kalici olarak cekili birakir — ikincisi bir
/// guvenlik sorunudur. Sunucu payload'i bu alanlardan KENDISI kurar.
///
/// Sozlesme: <c>docs/api-contract/08-scada-command.md</c>
/// </summary>
public class DeviceCommandSendRequest : IDto
{
    public DeviceCommandType CommandType { get; set; }

    /// <summary>
    /// Hedef kanal. <c>SetOutput</c>/<c>PulseOutput</c>/<c>SetValue</c> icin
    /// ZORUNLU; <c>Reset</c>/<c>Sync</c> modulun tamamina gider ve bu alan NULL
    /// olmak zorundadir. Doluysa sessizce yok sayilmaz, reddedilir: yok saymak,
    /// "tek kanali reset'ledim" saniyor olan bir istemciyi yaniltirdi.
    /// </summary>
    public Guid? IoChannelId { get; set; }

    /// <summary>
    /// Yazilacak deger — telemetriyle ayni sekilde STRING tasinir. Kanal basina tip
    /// yoktur: role icin <c>"1"</c>, ayar noktasi icin <c>"250"</c>.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// <c>PulseOutput</c> icin darbe suresi. Sureyi SCADA uygular; bizde bekleyen
    /// bir is yoktur (bkz. <see cref="DeviceCommandType.PulseOutput"/>).
    /// </summary>
    public int? DurationMs { get; set; }
}

public class DeviceCommandSendRequestValidator : AbstractValidator<DeviceCommandSendRequest>
{
    /// <summary>
    /// Darbe suresi sinirlari. Alt sinir, SCADA'nin ayirt edemeyecegi kadar kisa
    /// darbeleri eler; ust sinir, "darbe" olmaktan cikmis bir sureyi. On dakikadan
    /// uzun surmesi gereken bir cikis <c>SetOutput</c> ile surulur ve elle birakilir —
    /// oyle bir cikisi darbe olarak gondermek, SCADA'nin sayaci kaybetmesi halinde
    /// roleyi kalici olarak cekili birakir.
    /// </summary>
    private const int MinDurationMs = 50;
    private const int MaxDurationMs = 600_000;

    private const int MaxValueLength = 64;

    public DeviceCommandSendRequestValidator()
    {
        RuleFor(v => v.CommandType).IsInEnum().WithMessage("Geçersiz komut türü");

        // Kanal hedefi olan ve olmayan komutlar. Ayrimi burada bir kez yapip
        // asagida iki yonlu olarak zorunlu kiliyoruz: eksik hedef kadar FAZLA hedef
        // de bir istemci hatasidir.
        RuleFor(v => v.IoChannelId).NotEmpty()
            .When(v => RequiresChannel(v.CommandType))
            .WithMessage("Bu komut için hedef kanal zorunlu");

        RuleFor(v => v.IoChannelId).Null()
            .When(v => !RequiresChannel(v.CommandType))
            .WithMessage("Bu komut modülün tamamına gider, kanal hedefi alamaz");

        RuleFor(v => v.Value).NotEmpty()
            .When(v => RequiresValue(v.CommandType))
            .WithMessage("Bu komut için değer zorunlu");

        RuleFor(v => v.Value).MaximumLength(MaxValueLength)
            .WithMessage($"Değer en fazla {MaxValueLength} karakter olabilir");

        // Sure YALNIZCA darbede anlamli. Zorunlulugu bu sinifin varlik sebebidir:
        // suresiz bir darbe roleyi kalici olarak cekili birakabilir.
        RuleFor(v => v.DurationMs).NotNull()
            .When(v => v.CommandType == DeviceCommandType.PulseOutput)
            .WithMessage("Darbe komutu için süre zorunlu");

        RuleFor(v => v.DurationMs).InclusiveBetween(MinDurationMs, MaxDurationMs)
            .When(v => v.DurationMs.HasValue)
            .WithMessage($"Süre {MinDurationMs}-{MaxDurationMs} ms aralığında olmalı");

        RuleFor(v => v.DurationMs).Null()
            .When(v => v.CommandType != DeviceCommandType.PulseOutput)
            .WithMessage("Süre yalnızca darbe komutunda kullanılır");
    }

    private static bool RequiresChannel(DeviceCommandType type) =>
        type is DeviceCommandType.SetOutput or DeviceCommandType.PulseOutput or DeviceCommandType.SetValue;

    private static bool RequiresValue(DeviceCommandType type) =>
        type is DeviceCommandType.SetOutput or DeviceCommandType.PulseOutput or DeviceCommandType.SetValue;
}

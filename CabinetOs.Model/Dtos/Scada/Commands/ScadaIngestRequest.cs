using CabinetOs.Core.Model;
using CabinetOs.Model.Enums;
using FluentValidation;

namespace CabinetOs.Model.Dtos.Scada.Commands;

// DIKKAT: `using static EntityEnums` BURADA CALISMAZ. `CabinetOs.Model.Dtos`
// altinda `DeviceStatus` adinda bir AD ALANI var (lookup entity'sinin DTO'lari);
// kisa ad once ona cozuluyor. Enum bu yuzden acikca nitelenir.

/// <summary>
/// <c>POST /api/Scada/ingest</c> govdesi — SCADA'dan BIZE push edilen telemetri.
///
/// Yon onemli: bu sistem sahayla Modbus konusmaz, SCADA ile HTTP uzerinden
/// {modul, kanal, deger} alisverisi yapar (bkz. EntityEnums.cs). Deger cekmeyiz,
/// SCADA gonderir.
///
/// <b>Kimlik govdedeki <see cref="CabinetId"/>.</b> Uc <c>[AllowAnonymous]</c>'tur
/// cunku SCADA'nin JWT'si yoktur. Bu bir sir DEGILDIR — kabin Id'si her diyagram
/// URL'inde gorunur — dolayisiyla kabini bir kez gormus herkes o kabin adina sahte
/// telemetri yazabilir. Sertlestirme (serialize edilmeyen ikinci bir
/// <c>Cabinet.IngestKey</c> kolonu) planda OPSIYONEL isaretli ve yapilmadi;
/// ingest ucu guvenilmeyen bir aga acilmadan once yapilmalidir.
///
/// Sozlesme: <c>docs/api-contract/07-scada-ingest.md</c>
/// </summary>
public class ScadaIngestRequest : IDto
{
    public Guid CabinetId { get; set; }

    /// <summary>
    /// Olcumun SCADA tarafindaki zamani. Bilgi amaclidir: yazilan
    /// <c>ValueUpdatedAt</c> / <c>LastSeen</c> alanlari SUNUCU saatinden gelir,
    /// cunku SCADA'nin saati kaymis olabilir ve bayat cihaz supurucusu
    /// (<c>StaleDeviceSweeper</c>) o alanlara gore karar veriyor — kaymis bir saat
    /// canli bir kabini kalicı olarak Offline gosterebilirdi.
    /// </summary>
    public DateTime? TimestampUtc { get; set; }

    public List<ScadaDeviceReading> Devices { get; set; } = [];
}

/// <summary>Tek bir modulun okumasi.</summary>
public class ScadaDeviceReading
{
    /// <summary>
    /// <c>Device.ExternalCode</c> — cihazin SCADA tarafindaki kimligi. Guid degil,
    /// cunku SCADA bizim Id'lerimizi bilmez; eslesme kabin icinde benzersiz olan
    /// bu kod uzerinden yapilir (IX_Device_CabinetId_ExternalCode).
    /// </summary>
    public string ExternalCode { get; set; } = null!;

    /// <summary>Null = "dokunma". Cihazin mevcut durumu korunur.</summary>
    public EntityEnums.DeviceStatus? StatusId { get; set; }

    public List<ScadaChannelReading> Channels { get; set; } = [];
}

/// <summary>Tek bir kanalin degeri.</summary>
public class ScadaChannelReading
{
    public int ChannelNumber { get; set; }

    /// <summary>
    /// Deger STRING olarak tasinir ve string olarak saklanir
    /// (<c>IoChannel.CurrentValue</c>). Kanal basina tip yoktur: ayni ingest
    /// govdesinde bir role icin <c>"1"</c>, bir sicaklik icin <c>"23.5"</c>
    /// gelebilir. Yorumlama gosterim katmaninin isidir.
    /// </summary>
    public string? Value { get; set; }
}

public class ScadaIngestRequestValidator : AbstractValidator<ScadaIngestRequest>
{
    /// <summary>
    /// Tek govdede kabul edilen en fazla cihaz / kanal. Sinir yoksa tek bir istek
    /// sinirsiz bellek ayirtabilir; uc kimlik dogrulamasiz oldugu icin bu bir
    /// hizmet disi birakma yoludur.
    /// </summary>
    private const int MaxDevices = 500;
    private const int MaxChannelsPerDevice = 512;

    public ScadaIngestRequestValidator()
    {
        RuleFor(v => v.CabinetId).NotEmpty().WithMessage("cabinetId zorunlu");

        RuleFor(v => v.Devices).NotNull().WithMessage("devices zorunlu");
        RuleFor(v => v.Devices).Must(d => d.Count <= MaxDevices)
            .WithMessage($"Tek gonderide en fazla {MaxDevices} cihaz olabilir");

        RuleForEach(v => v.Devices).ChildRules(device =>
        {
            device.RuleFor(d => d.ExternalCode).NotEmpty().WithMessage("externalCode zorunlu");
            device.RuleFor(d => d.ExternalCode).MaximumLength(64).WithMessage("externalCode en fazla 64 karakter olabilir");
            // Null gecerli ("dokunma"); dolu ise tanimli bir deger olmali.
            device.RuleFor(d => d.StatusId).IsInEnum().When(d => d.StatusId.HasValue)
                .WithMessage("Gecersiz cihaz durumu");
            device.RuleFor(d => d.Channels).NotNull().WithMessage("channels zorunlu");
            device.RuleFor(d => d.Channels).Must(c => c.Count <= MaxChannelsPerDevice)
                .WithMessage($"Cihaz basina en fazla {MaxChannelsPerDevice} kanal olabilir");

            device.RuleForEach(d => d.Channels).ChildRules(channel =>
            {
                channel.RuleFor(c => c.ChannelNumber).GreaterThan(0)
                    .WithMessage("channelNumber sifirdan buyuk olmali");
                // Deger NULL olabilir: "kanal var ama okunamadi" mesru bir durum.
                channel.RuleFor(c => c.Value).MaximumLength(256)
                    .WithMessage("value en fazla 256 karakter olabilir");
            });
        });
    }
}

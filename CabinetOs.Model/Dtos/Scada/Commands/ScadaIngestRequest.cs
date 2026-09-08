using CabinetOs.Core.Model;
using CabinetOs.Model.Enums;
using FluentValidation;

namespace CabinetOs.Model.Dtos.Scada.Commands;

/// <summary>
/// <c>POST /api/Scada/ingest</c> govdesi — SCADA'dan BIZE push edilen TEK okuma.
///
/// <b>Neden tek okuma, neden toplu degil.</b> Kart olay guduml&#252;d&#252;r: bir pin
/// degistiginde 5 baytlik tek bir cerceve gonderir
/// (<c>[baslik][pin no][deger][zaman][bitis]</c>). Toplu bir paket diye bir sey
/// hicbir zaman gelmez, dolayisiyla <c>devices[] -&gt; channels[]</c> seklindeki
/// eski govde sahada karsiligi olmayan bir yapiydi.
///
/// <b>Kimlik yalnizca <see cref="CabinetId"/>.</b> Kabin BIR kontrol kartidir;
/// kartin adres uzayi duzdur, dolayisiyla kabin + pin bir noktayi tek basina
/// belirler. Eski <c>externalCode</c> alani kalkti: protokolde modul bayti yok,
/// kart kimligi soketin kendisidir.
///
/// Uc <c>[AllowAnonymous]</c>'tur cunku SCADA'nin JWT'si yoktur. Bu bir sir
/// DEGILDIR — kabin Id'si her diyagram URL'inde gorunur — dolayisiyla kabini bir
/// kez gormus herkes o kabin adina sahte telemetri yazabilir. Sertlestirme
/// (serialize edilmeyen ikinci bir <c>Cabinet.IngestKey</c> kolonu) yapilmadi;
/// ingest ucu guvenilmeyen bir aga acilmadan once yapilmalidir.
///
/// Sozlesme: <c>docs/api-contract/07-scada-ingest.md</c>
/// </summary>
public class ScadaIngestRequest : IDto
{
    public Guid CabinetId { get; set; }

    /// <summary>
    /// Okumanin geldigi nokta — <c>"IN1"</c>, <c>"IN7"</c>. Ayristirma
    /// <see cref="ScadaPinAddress"/>'te.
    ///
    /// <b>Bu turda YALNIZCA giris kabul edilir.</b> Cikis pinlerinden bilgi
    /// gelmez, onlara yalnizca kumanda gider — bir roleyi biz surdugumuzde donen
    /// deger saha olayi degil kendi komutumuzun yankisidir ve kaydi zaten
    /// <c>DeviceCommand</c>'dadir. <c>"OUT5"</c> gibi bir adres sozdizimsel olarak
    /// TANINIR ama 400 ile reddedilir: bu "tanimadigim referans" degil, anlamca
    /// gecersiz bir istektir ve sessizce atlamak yapilandirma hatasini gorunmez
    /// kilardi.
    /// </summary>
    public string Pin { get; set; } = null!;

    /// <summary>
    /// Deger STRING olarak tasinir ve string olarak saklanir
    /// (<c>IoChannel.CurrentValue</c>). Kanal basina tip yoktur: bir role icin
    /// <c>"1"</c>, bir sicaklik icin <c>"23.5"</c> gelebilir. Yorumlama gosterim
    /// katmaninin isidir.
    ///
    /// <c>null</c> gecerlidir ve "kanal var ama okunamadi" demektir; <c>"0"</c>
    /// ile ayni sey DEGILDIR.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Olcumun SCADA tarafindaki zamani. Bilgi amaclidir: yazilan
    /// <c>ValueUpdatedAt</c> / <c>LastSeen</c> alanlari SUNUCU saatinden gelir,
    /// cunku SCADA'nin saati kaymis olabilir ve bayat cihaz supurucusu
    /// (<c>OfflineDeviceChecker</c>) o alanlara gore karar veriyor — kaymis bir saat
    /// canli bir kabini kalici olarak Offline gosterebilirdi.
    /// </summary>
    public DateTime? TimestampUtc { get; set; }
}

public class ScadaIngestRequestValidator : AbstractValidator<ScadaIngestRequest>
{
    /// <summary>
    /// Govde 256 karaktere kadar deger kabul eder. Olay kolonu (<c>ChannelEvent.Value</c>)
    /// 32'dir; asan deger anlik degeri yine gunceller, kalici olay uretmez.
    /// </summary>
    private const int MaxValueLength = 256;

    public ScadaIngestRequestValidator()
    {
        RuleFor(v => v.CabinetId).NotEmpty().WithMessage("cabinetId zorunlu");

        RuleFor(v => v.Pin).NotEmpty().WithMessage("pin zorunlu");

        // Ayristirilabilirlik ve YON ayri ayri raporlanir: "IN0" ile "OUT5"
        // ikisi de 400 dondurur ama sebepleri farklidir ve entegrasyon yapan
        // kisinin hangisiyle karsilastigini bilmesi gerekir.
        RuleFor(v => v.Pin)
            .Must(pin => ScadaPinAddress.TryParse(pin, out _))
            .When(v => !string.IsNullOrWhiteSpace(v.Pin))
            .WithMessage("Gecersiz pin adresi. Beklenen bicim: IN1, IN2, ...");

        RuleFor(v => v.Pin)
            .Must(pin => !ScadaPinAddress.TryParse(pin, out var address)
                         || address.Direction == EntityEnums.PinDirection.Input)
            .When(v => !string.IsNullOrWhiteSpace(v.Pin))
            .WithMessage("Cikis pininden telemetri kabul edilmiyor; yalnizca giris pinleri (IN...) veri gonderir");

        // Deger NULL olabilir: "kanal var ama okunamadi" mesru bir durum.
        RuleFor(v => v.Value).MaximumLength(MaxValueLength)
            .WithMessage($"value en fazla {MaxValueLength} karakter olabilir");
    }
}

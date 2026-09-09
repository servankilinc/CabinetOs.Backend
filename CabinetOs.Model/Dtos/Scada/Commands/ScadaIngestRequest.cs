using CabinetOs.Core.Model;
using FluentValidation;

namespace CabinetOs.Model.Dtos.Scada.Commands;

/// <summary>
/// <c>POST /api/Scada/ingest</c> govdesi — SCADA'dan BIZE push edilen TEK okuma.
///
/// <b>Neden tek okuma, neden toplu degil.</b> Kart olay gudumludur: bir giris
/// degistiginde 3 baytlik tek bir cerceve gonderir —
/// <c>[baslik][kod][deger]</c>. SCADA akisi bu cercevelere ayirir ve HER CERCEVE
/// ICIN bir istek atar; dolayisiyla <c>devices[] -&gt; channels[]</c> seklindeki
/// eski govde sahada karsiligi olmayan bir yapiydi.
///
/// <b>Neden cerceve alanlarina bolunmus.</b> Govde cercevenin kendi yapisini
/// tasir: baslik <see cref="Type"/>, orta bayt <see cref="ChannelNumber"/>,
/// son bayt <see cref="Value"/>. Eskiden ikisi <c>pin: "IN7"</c> diye tek metinde
/// birlesikti ve ayristirilmasi gerekiyordu.
///
/// <b>Kimlik yalnizca <see cref="CabinetId"/>.</b> Kabin BIR kontrol kartidir;
/// kartin adres uzayi duzdur, dolayisiyla kabin + tip + kanal bir noktayi tek
/// basina belirler. Eski <c>externalCode</c> alani kalkti: protokolde modul bayti
/// yok, kart kimligi soketin kendisidir.
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
    /// Cerceve basligi: <c>"I"</c> dijital giris, <c>"A"</c> analog giris.
    /// Yone cevrimi <see cref="ScadaPinAddress.TryParseType"/>'ta.
    ///
    /// <b>Bu turda YALNIZCA giris kabul edilir</b> ve bu, alanin tipiyle zorlanir:
    /// <c>"O"</c> taninmaz, dolayisiyla cikis telemetrisi govdede IFADE EDILEMEZ.
    /// (Birlesik <c>"OUT5"</c> adresi kullanildigi donemde bu, ayrica 400 ile
    /// reddedilmesi gereken bir durumdu.)
    ///
    /// C# enum'u DEGIL duz string: <c>ApiJsonOptions</c> enum'lari SAYI olarak
    /// serilestiriyor, dolayisiyla bir enum <c>"I"</c> kabul etmezdi ve tek alan
    /// icin ayri bir converter takmak gereksiz karmasiklik olurdu.
    /// </summary>
    public string Type { get; set; } = null!;

    /// <summary>
    /// Cercevenin orta bayti — referans projedeki cihaz <c>Code</c>'u, bizde
    /// <c>IoChannel.ChannelNumber</c>.
    ///
    /// <b><c>Type</c> ile birlikte anlamlidir, tek basina degil:</b> <c>"I"</c> ve
    /// <c>"A"</c> BAGIMSIZ kod uzaylaridir — ayni kartta hem <c>A/1</c> (sicaklik)
    /// hem <c>I/1</c> (darbe sensoru) bulunur ve bunlar AYRI noktalardir.
    /// </summary>
    public int ChannelNumber { get; set; }

    /// <summary>
    /// Deger STRING olarak tasinir ve string olarak saklanir
    /// (<c>IoChannel.CurrentValue</c>). Kanal basina tip yoktur: bir role icin
    /// <c>"1"</c>, bir sicaklik icin <c>"235"</c> gelebilir. Yorumlama gosterim
    /// katmaninin isidir.
    ///
    /// <c>null</c> gecerlidir ve "kanal var ama okunamadi" demektir; <c>"0"</c>
    /// ile ayni sey DEGILDIR.
    ///
    /// <b>Olcekleme bizde YAPILMAZ.</b> Cercevenin deger bayti tek bayttir (0-255),
    /// yani ondalik ve negatif tasiyamaz; bir olcegin gerekmesi halinde donusumu
    /// SCADA yapar ve <c>"23.5"</c> gonderir — alan tipi degismeden ayni yere duser.
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

    /// <summary>
    /// Kanal numarasinin ust siniri. Kartin kendi kod bayti zaten 0-255 ile
    /// sinirli; buradaki daha genis sinir, adresi biz uretmedigimiz icin
    /// savunmacidir ve <see cref="ScadaPinAddress.Format"/> ciktisinin makul
    /// uzunlukta kalmasini garanti eder.
    /// </summary>
    private const int MaxChannelNumber = 9999;

    public ScadaIngestRequestValidator()
    {
        RuleFor(v => v.CabinetId).NotEmpty().WithMessage("cabinetId zorunlu");

        RuleFor(v => v.Type).NotEmpty().WithMessage("type zorunlu");

        RuleFor(v => v.Type)
            .Must(type => ScadaPinAddress.TryParseType(type, out _))
            .When(v => !string.IsNullOrWhiteSpace(v.Type))
            .WithMessage("Gecersiz tip. Beklenen: \"I\" (dijital giris) veya \"A\" (analog giris)");

        // Kartta 0 diye bir nokta yok; 0 bir yazim hatasidir, gecerli bir adres degil.
        RuleFor(v => v.ChannelNumber)
            .InclusiveBetween(1, MaxChannelNumber)
            .WithMessage($"channelNumber 1 ile {MaxChannelNumber} arasinda olmalidir");

        // Deger NULL olabilir: "kanal var ama okunamadi" mesru bir durum.
        RuleFor(v => v.Value).MaximumLength(MaxValueLength)
            .WithMessage($"value en fazla {MaxValueLength} karakter olabilir");
    }
}

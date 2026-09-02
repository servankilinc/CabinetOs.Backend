using CabinetOs.Core.Model;
using CabinetOs.Model.Dtos.Diagram.Commands.Abstract;
using FluentValidation;

namespace CabinetOs.Model.Dtos.Diagram.Commands.Items;

/// <summary>
/// Canvas'taki bir cihazin TAM durumu — yeni de olabilir, mevcut da.
///
/// Burada OLMAYAN alanlar sunucuda dokunulmadan kalir: <c>DeviceStatusId</c> /
/// <c>LastSeen</c> (telemetri) ve <c>IpAddress</c> / <c>MacAddress</c> (cihaz
/// yonetimi) bilerek disarida — diyagram kaydetmek SCADA'nin yazdigini ezmemeli.
/// Koruma yorumla degil TIPLE saglanir: kaynak tipte o alanlar yok.
///
/// Cihazi pasife almak buradan degil, <c>deleted</c> listesinden yapilir.
/// </summary>
public class DeviceDraft : IDto, IIdentifiableDraft
{
    public Guid Id { get; set; }

    /// <summary>
    /// Yalnizca OLUSTURMADA kullanilir. Mevcut bir cihazin sablonu degistirilemez:
    /// pinler sablondan turedigi icin sablon degistirmek cihazi bastan yaratmak
    /// demektir (bkz. <c>DiagramService.SaveInternals</c>).
    /// </summary>
    public Guid ComponentTemplateId { get; set; }

    public string Name { get; set; } = null!;
    public double CoordinateX { get; set; }
    public double CoordinateY { get; set; }
    public double Rotation { get; set; }
    public int ZIndex { get; set; }
    public bool IsLocked { get; set; }
    public bool IsVisible { get; set; } = true;

    /// <summary> SCADA tarafindaki kimlik. Editorde bos birakilabilir, sonra atanir.</summary>
    public string? ExternalCode { get; set; }

    /// <summary>
    /// Olusacak pinlerin KIMLIKLERI — yalnizca OLUSTURMADA doldurulur.
    ///
    /// Sablonun pin kumesine birebir karsilik gelmelidir. Mevcut bir cihazda dolu
    /// gonderilirse 400: pinleri zaten var ve sablonu degistirilemiyor.
    ///
    /// Pin VERISI tasimaz; sunucu her alani sablondan kopyalar (bkz.
    /// <see cref="DevicePinDraft"/>).
    /// </summary>
    public List<DevicePinDraft> Pins { get; set; } = [];

    /// <summary>
    /// Olusacak telemetri kanallarinin KIMLIKLERI — yalnizca OLUSTURMADA.
    /// Ad, okuma yolundaki <c>DiagramDeviceDto.IoChannels</c> ile AYNI tutuluyor:
    /// istemci ayni listeyi okuyup geri gonderiyor, arada bir ad cevirisi olmamali.
    /// Sablon pinlerinin null olmayan farkli kanal numaralarina birebir karsilik
    /// gelmelidir (bkz. <see cref="DeviceIoChannelDraft"/>).
    /// </summary>
    public List<DeviceIoChannelDraft> IoChannels { get; set; } = [];
}

public class DeviceDraftValidator : AbstractValidator<DeviceDraft>
{
    public DeviceDraftValidator()
    {
        RuleFor(v => v.Id).NotEqual(Guid.Empty).WithMessage("Cihaz kimligi zorunlu");
        RuleFor(v => v.ComponentTemplateId).NotEqual(Guid.Empty).WithMessage("Sablon secilmeli");
        RuleFor(v => v.Name).NotEmpty().WithMessage("Cihaz adi zorunlu");
        RuleFor(v => v.Name).MaximumLength(128).WithMessage("Cihaz adi en fazla 128 karakter olabilir");
        RuleFor(v => v.ExternalCode).MaximumLength(64).WithMessage("Dis kod en fazla 64 karakter olabilir");

        // Yalnizca tekil taslak sagligi. Kumelerin sablonla ORTUSMESI burada
        // dogrulanamaz — sablonun pin semasi DB'den okunmadan bilinmiyor; o kontrol
        // DiagramService.ValidateDevices'ta.
        RuleForEach(v => v.Pins).SetValidator(new DevicePinDraftValidator());
        RuleForEach(v => v.IoChannels).SetValidator(new DeviceIoChannelDraftValidator());
    }
}






#region DevicePinDraft

/// <summary>
/// Yeni bir cihazin TEK bir pini icin istemcinin urettigi kimlik.
///
/// <b>Burada pin VERISI yoktur, yalnizca kimlik vardir.</b> Ad, konum, fonksiyon,
/// yon ve gerilim sunucuda <c>ComponentTemplatePin</c>'den kopyalanmaya devam eder;
/// istemciden gelen tek sey Guid ve o Guid'in hangi sablon pinine karsilik geldigi.
/// Pin semasinin tek yazari hala sablon ekranidir (ROADMAP R2) — bu tip o kurali
/// delmez, sadece kimlik uretimini istemciye tasir.
///
/// Sunucu <c>ComponentTemplatePinId</c> kumesinin sablonun pin kumesine BIREBIR
/// esit oldugunu dogrular; eksik, fazla veya tekrarli gonderim 400'dur.
/// </summary>
public class DevicePinDraft : IDto
{
    /// <summary> Olusacak <c>Pin</c> satirinin birincil anahtari. </summary>
    public Guid Id { get; set; }

    /// <summary> Bu pinin turedigi sablon pini. </summary>
    public Guid ComponentTemplatePinId { get; set; }
}

public class DevicePinDraftValidator : AbstractValidator<DevicePinDraft>
{
    public DevicePinDraftValidator()
    {
        RuleFor(v => v.Id).NotEqual(Guid.Empty).WithMessage("Pin kimligi zorunlu");
        RuleFor(v => v.ComponentTemplatePinId).NotEqual(Guid.Empty).WithMessage("Sablon pini zorunlu");
    }
}
#endregion









#region DeviceIoChannelDraft

/// <summary>
/// Yeni bir cihazin TEK bir telemetri kanali icin istemcinin urettigi kimlik.
///
/// <b>Neden pinin icine gomulu degil.</b> "Ayni cihazda ayni kanal numarasi TEK
/// bir <c>IoChannel</c>'dir" kurali (<c>IX_IoChannel_DeviceId_ChannelNumber</c>)
/// boyle YAPISAL olarak tutarsiz ifade edilemez hale gelir. Her pin kendi
/// <c>IoChannelId</c>'sini tasisaydi ayni kanali gosteren iki pinin ayni Id'yi
/// tasidigi her gonderide ayrica dogrulanmak zorunda kalirdi.
///
/// Sunucu kanal numarasi kumesinin, sablon pinlerinin null olmayan farkli kanal
/// numaralarina BIREBIR esit oldugunu dogrular.
/// </summary>
public class DeviceIoChannelDraft : IDto
{
    /// <summary> Olusacak <c>IoChannel</c> satirinin birincil anahtari. </summary>
    public Guid Id { get; set; }

    /// <summary> SCADA'nin kanali cozdugu numara; cihaz icinde benzersiz. </summary>
    public int ChannelNumber { get; set; }
}

public class DeviceIoChannelDraftValidator : AbstractValidator<DeviceIoChannelDraft>
{
    public DeviceIoChannelDraftValidator()
    {
        RuleFor(v => v.Id).NotEqual(Guid.Empty).WithMessage("Kanal kimligi zorunlu");
    }
} 
#endregion

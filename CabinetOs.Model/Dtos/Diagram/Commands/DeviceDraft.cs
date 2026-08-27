using CabinetOs.Core.Model;
using FluentValidation;

namespace CabinetOs.Model.Dtos.Diagram.Commands;

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
    }
}

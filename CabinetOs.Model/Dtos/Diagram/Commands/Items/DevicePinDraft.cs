using CabinetOs.Core.Model;
using FluentValidation;

namespace CabinetOs.Model.Dtos.Diagram.Commands.Items;

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

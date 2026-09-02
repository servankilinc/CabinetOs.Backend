using CabinetOs.Core.Model;
using CabinetOs.Model.Dtos.Common;
using CabinetOs.Model.Dtos.Diagram.Commands.Draft.Abstract;
using FluentValidation;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Model.Dtos.Diagram.Commands.Draft;

/// <summary> Canvas'taki bir kablonun TAM durumu — yeni de olabilir, mevcut da. </summary>
public class ConnectionDraft : IDto, IIdentifiableDraft
{
    public Guid Id { get; set; }

    /// <summary>
    /// Uclar her zaman KALICI pin kimligidir: pinleri sunucu, cihazla birlikte
    /// sablondan uretir.
    ///
    /// Mevcut bir kabloda uc DEGISTIRILEMEZ — farkli gonderilirse 400 doner.
    /// Bir kablonun ucunu tasimak sil + olustur'dur; aksi halde filtreli unique
    /// index (SourcePinId, TargetPinId) ile cakisan sessiz durumlar dogar.
    /// </summary>
    public Guid SourcePinId { get; set; }
    public Guid TargetPinId { get; set; }

    public string? Label { get; set; }
    public WireType WireType { get; set; }
    public string Color { get; set; } = null!;
    public LineStyle LineStyle { get; set; }
    public double StrokeWidth { get; set; }
    public EdgeRouting Routing { get; set; }

    /// <summary>Ara kirilma noktalari, kaynak -> hedef sirali, IKI UC NOKTA HARIC. Bos olabilir.</summary>
    public List<PointDto> Waypoints { get; set; } = [];
    public int ZIndex { get; set; }
}

public class ConnectionDraftValidator : AbstractValidator<ConnectionDraft>
{
    public ConnectionDraftValidator()
    {
        RuleFor(v => v.Id).NotEqual(Guid.Empty).WithMessage("Kablo kimligi zorunlu");

        RuleFor(v => v.SourcePinId).NotEqual(Guid.Empty).WithMessage("Kaynak pin kimligi zorunlu");
        RuleFor(v => v.TargetPinId).NotEqual(Guid.Empty).WithMessage("Hedef pin kimligi zorunlu");
        RuleFor(v => v.TargetPinId)
            .NotEqual(v => v.SourcePinId)
            .WithMessage("Bir pin kendisine baglanamaz");

        RuleFor(v => v.Label).MaximumLength(128).WithMessage("Etiket en fazla 128 karakter olabilir");
        RuleFor(v => v.WireType).IsInEnum().WithMessage("Gecersiz kablo tipi");
        RuleFor(v => v.Color).NotEmpty().WithMessage("Kablo rengi zorunlu");
        RuleFor(v => v.Color).MaximumLength(32).WithMessage("Kablo rengi en fazla 32 karakter olabilir");
        RuleFor(v => v.LineStyle).IsInEnum().WithMessage("Gecersiz cizgi stili");
        RuleFor(v => v.Routing).IsInEnum().WithMessage("Gecersiz cizim sekli");
        RuleFor(v => v.StrokeWidth).GreaterThan(0).WithMessage("Kalinlik sifirdan buyuk olmali");
        RuleFor(v => v.Waypoints).Must(w => w.Count <= 64).WithMessage("Bir kabloda en fazla 64 kirilma noktasi olabilir");
    }
}

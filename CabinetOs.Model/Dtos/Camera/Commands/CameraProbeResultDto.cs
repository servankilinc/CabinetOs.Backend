using CabinetOs.Core.Model;
using FluentValidation;

namespace CabinetOs.Model.Dtos.Camera.Commands;

/// <summary>
/// Bir yoklama denemesinin sonucu.
///
/// Bu gövdeyi gönderen taraf bu turda YAZILMADI — yoklayıcıyı kullanıcı kendisi
/// yazacak. Uç, o servisin in-process mi harici mi olduğundan bağımsız hedefidir:
/// içeriden çağrılırsa <c>ICameraService.RecordProbeResultAsync</c>, dışarıdan
/// çağrılırsa <c>POST /api/Camera/{id}/probe-result</c>.
///
/// Sözleşme: <c>docs/api-contract/11-camera.md</c>
/// </summary>
public class CameraProbeResultDto : IDto
{
    /// <summary>Kameraya ulaşıldı mı?</summary>
    public bool Reachable { get; set; }

    /// <summary>
    /// Gidiş-dönüş süresi (ms) — bilgi amaçlı, saklanmaz.
    ///
    /// Kolon olarak tutulmuyor: her yoklamada değişen bir sayıyı yazmak,
    /// "durum değişmediyse yazma yok" kuralını anlamsız kılardı — kamera ayakta
    /// dursa bile her 5 dakikada bir UPDATE üretirdi.
    /// </summary>
    public int? RttMs { get; set; }

    /// <summary>Ulaşılamadıysa sebep. Başarılı yoklamada yok sayılır ve temizlenir.</summary>
    public string? Error { get; set; }
}

public class CameraProbeResultDtoValidator : AbstractValidator<CameraProbeResultDto>
{
    public CameraProbeResultDtoValidator()
    {
        RuleFor(v => v.RttMs!.Value).GreaterThanOrEqualTo(0).When(v => v.RttMs.HasValue)
            .WithMessage("Gecikme negatif olamaz");
        RuleFor(v => v.Error).MaximumLength(512).WithMessage("Hata metni en fazla 512 karakter olabilir");
    }
}

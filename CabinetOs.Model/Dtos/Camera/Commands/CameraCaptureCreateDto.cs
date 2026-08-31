using CabinetOs.Core.Model;
using FluentValidation;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Model.Dtos.Camera.Commands;

/// <summary>
/// Cekim istegi. Sozlesme: <c>docs/api-contract/11-camera.md</c>
///
/// Kamera rotadan gelir, govdede YOKTUR — cekim her zaman tek bir kameranin
/// alt kaynagidir.
/// </summary>
public class CameraCaptureCreateDto : IDto
{
    /// <summary>Anlik goruntu mu, klip mi?</summary>
    public CaptureType Type { get; set; } = CaptureType.Snapshot;

    /// <summary>
    /// Klip suresi (saniye). <see cref="CaptureType.Snapshot"/> icin
    /// <c>null</c> OLMALIDIR — tek karenin suresi yoktur.
    ///
    /// <b>Klip ileri yonludur:</b> kayit istek anindan itibaren baslar, olay
    /// oncesini KAPSAMAZ. Oncesini yakalamak surekli donen bir kayit tamponu
    /// gerektirirdi ve bu, "7/24 kayit yapilmaz" karariyla cakisirdi.
    /// </summary>
    public int? DurationSec { get; set; }
}

public class CameraCaptureCreateDtoValidator : AbstractValidator<CameraCaptureCreateDto>
{
    /// <summary>
    /// Ust sinir burada SABIT: dogrulayici yapilandirmayi okumuyor. Ayarlardaki
    /// <c>MaxClipDurationSec</c> daha dusuk olabilir ve servis onu ayrica
    /// uygular; buradaki sinir yalnizca acikca sacma degerleri (bir saatlik
    /// klip) sozlesme duzeyinde keser.
    /// </summary>
    private const int AbsoluteMaxClipSeconds = 600;

    public CameraCaptureCreateDtoValidator()
    {
        RuleFor(v => v.Type).IsInEnum().WithMessage("Geçersiz çekim tipi");

        RuleFor(v => v.DurationSec)
            .NotNull().WithMessage("Klip süresi girilmeli")
            .When(v => v.Type == CaptureType.Clip);

        RuleFor(v => v.DurationSec!.Value)
            .InclusiveBetween(1, AbsoluteMaxClipSeconds)
            .WithMessage($"Klip süresi 1-{AbsoluteMaxClipSeconds} saniye arasında olmalı")
            .When(v => v.Type == CaptureType.Clip && v.DurationSec.HasValue);

        RuleFor(v => v.DurationSec)
            .Null().WithMessage("Anlık görüntüde süre belirtilemez")
            .When(v => v.Type == CaptureType.Snapshot);
    }
}

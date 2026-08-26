using FluentValidation;

namespace CabinetOs.Model.Dtos.Diagram.Commands;

/// <summary> 
/// Yeni olusturulan bir taslagin, sunucu Id'sini almadan once tasidigi gecici kimlik. 
/// Ayri bir arayuz olmasinin sebebi: <see cref="DiagramSaveRequest"/> tum gecici kimlikleri TEK bir kumede toplayip benzersizligini dogruluyor.
/// </summary>
public interface ITempIdDraft
{
    string TempId { get; set; }
}

/// <summary>
/// Var olan bir satiri hedefleyen taslak. Guncelleme listelerinde ayni Id'nin iki kez gecmesi ya da hem guncellenip hem silinmesi gibi celiskileri
/// <see cref="DiagramSaveRequest"/> bu arayuz uzerinden tek yerde yakalar.
/// </summary>
public interface IIdentifiableDraft
{
    Guid Id { get; set; }
}

/// <summary>
/// Taslak DTO'larinin paylastigi dogrulama kurallari.
/// </summary>
public static class DraftRules
{
    /// <summary>
    /// Gecici kimliklerin zorunlu oneki.
    ///
    /// Sunucunun bunu dayatmasinin sebebi istemcinin ayirt edicisi olmasi:
    /// React Flow'da node/edge id'leri tek bir string uzayinda yasar ve istemci
    /// "bu daha kaydedilmemis mi" sorusunu <c>id.startsWith('tmp_')</c> ile
    /// cevaplar. Sunucu keyfi string kabul etseydi, bu varsayim sessizce
    /// bozulurdu ve hata ancak kaydetmeden SONRA fark edilirdi.
    /// </summary>
    public const string TempIdPrefix = "tmp_";

    public const int TempIdMaxLength = 64;

    public static void AddTempIdRules<T>(this AbstractValidator<T> validator) where T : ITempIdDraft
    {
        validator.RuleFor(v => v.TempId).NotEmpty()
            .WithMessage("Gecici kimlik zorunlu");
        validator.RuleFor(v => v.TempId).MaximumLength(TempIdMaxLength)
            .WithMessage($"Gecici kimlik en fazla {TempIdMaxLength} karakter olabilir");

        validator.RuleFor(v => v.TempId).Must(t => t.StartsWith(TempIdPrefix, StringComparison.Ordinal))
            .When(v => !string.IsNullOrEmpty(v.TempId))
            .WithMessage($"Gecici kimlik '{TempIdPrefix}' ile baslamali");
    }
}

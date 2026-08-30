using CabinetOs.Core.Model;
using FluentValidation;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Model.Dtos.IoChannel.Commands;

public class IoChannelUpdateDto : IDto
{
    public Guid Id { get; set; }
    public int ChannelNumber { get; set; }
    public PinDirection Direction { get; set; }
    public bool IsEnabled { get; set; }
    public string Name { get; set; } = null!;

    /// <summary>Bu kanalın değişimleri kalıcı olay olarak kaydedilsin mi?</summary>
    public bool IsEventLogged { get; set; }

    /// <summary>Doluysa olay yalnızca bu değere geçişte yazılır; null ise her değişim olaydır.</summary>
    public string? EventTriggerValue { get; set; }
}

public class IoChannelUpdateDtoValidator : AbstractValidator<IoChannelUpdateDto>
{
    public IoChannelUpdateDtoValidator()
    {
        RuleFor(v => v.Id).NotNull().WithMessage("Field cannot be null");
        RuleFor(v => v.Id).NotEqual(Guid.Empty).WithMessage("Field mus be a valid guid value");
        RuleFor(v => v.ChannelNumber).NotEmpty().WithMessage("Kanal numarası girilmeli");
        RuleFor(v => v.Direction).IsInEnum().WithMessage("Geçersiz bağlantı yönü");
        RuleFor(v => v.IsEnabled).NotNull().WithMessage("Geçersiz bilgi");
        RuleFor(v => v.Name).NotEmpty().WithMessage("İsim bilgisi girilmeli");
        // Kolon nvarchar(32); DB'deki kısıtı 500 yerine 400'e çevirir.
        RuleFor(v => v.EventTriggerValue).MaximumLength(32)
            .WithMessage("Tetikleyici değer en fazla 32 karakter olabilir");
        // Yalnızca giriş kanalları olay üretir; çıkışta bayrağı açmak sessizce
        // hiçbir şey yapmazdı ve kullanıcı sebebini anlayamazdı.
        RuleFor(v => v.IsEventLogged).Equal(false)
            .When(v => v.Direction != PinDirection.Input)
            .WithMessage("Olay kaydı yalnızca giriş kanallarında açılabilir");
    }
}
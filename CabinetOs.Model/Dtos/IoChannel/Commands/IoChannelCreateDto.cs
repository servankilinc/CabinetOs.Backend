using CabinetOs.Core.Model;
using FluentValidation;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Model.Dtos.IoChannel.Commands;

public class IoChannelCreateDto : IDto
{
    public Guid DeviceId { get; set; }
    public int ChannelNumber { get; set; }
    public PinDirection Direction { get; set; }
    public bool IsEnabled { get; set; }
    public string Name { get; set; } = null!;

    /// <summary>Bu kanalın değişimleri kalıcı olay olarak kaydedilsin mi?</summary>
    public bool IsEventLogged { get; set; }

    /// <summary>Doluysa olay yalnızca bu değere geçişte yazılır; null ise her değişim olaydır.</summary>
    public string? EventTriggerValue { get; set; }
}

public class IoChannelCreateDtoValidator : AbstractValidator<IoChannelCreateDto>
{
    public IoChannelCreateDtoValidator()
    {
        RuleFor(v => v.DeviceId).NotNull().WithMessage("Geçersiz cihaz bilgisi");
        RuleFor(v => v.DeviceId).NotEqual(Guid.Empty).WithMessage("Geçersiz cihaz bilgisi");
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
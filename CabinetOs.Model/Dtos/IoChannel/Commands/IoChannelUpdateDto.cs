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
    }
}
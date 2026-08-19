using CabinetOs.Core.Model;
using FluentValidation;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Model.Dtos.Pin.Commands;

public class PinUpdateDto : IDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public double RelativeX { get; set; }
    public double RelativeY { get; set; }
    public PinFunction Function { get; set; }
    public SignalLayer SignalLayer { get; set; }
    public VoltageLevel? VoltageLevel { get; set; }
}

public class PinUpdateDtoValidator : AbstractValidator<PinUpdateDto>
{
    public PinUpdateDtoValidator()
    {
        RuleFor(v => v.Id).NotNull().WithMessage("Field cannot be null");
        RuleFor(v => v.Id).NotEqual(Guid.Empty).WithMessage("Field mus be a valid guid value");
        RuleFor(v => v.Name).NotEmpty().WithMessage("İsim bilgisi zorunlu");
        RuleFor(v => v.RelativeX).NotNull().WithMessage("Geçersiz kordinat x bilgisi");
        RuleFor(v => v.RelativeY).NotNull().WithMessage("Geçersiz kordinat y bilgisi");
        RuleFor(v => v.Function).IsInEnum().WithMessage("Geçersiz fonksiyon ataması");
        RuleFor(v => v.SignalLayer).IsInEnum().WithMessage("Geçersiz sinyal ataması");
    }
}
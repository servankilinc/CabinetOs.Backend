using CabinetOs.Core.Model;
using static CabinetOs.Model.Enums.EntityEnums;
using FluentValidation;

namespace CabinetOs.Model.Dtos.ComponentTemplatePin.Commands;

public class ComponentTemplatePinCreateDto : IDto
{
    public Guid ComponentTemplateId { get; set; }
    public string Name { get; set; } = null!;
    public double RelativeX { get; set; }
    public double RelativeY { get; set; }
    public int? ChannelNumber { get; set; }
    public PinFunction Function { get; set; }
    public PinDirection Direction { get; set; }
    public SignalLayer SignalLayer { get; set; }
    public VoltageLevel? VoltageLevel { get; set; }
}

public class ComponentTemplatePinCreateDtoValidator : AbstractValidator<ComponentTemplatePinCreateDto>
{
    public ComponentTemplatePinCreateDtoValidator()
    {
        RuleFor(v => v.ComponentTemplateId).NotNull().WithMessage("Field cannot be null");
        RuleFor(v => v.ComponentTemplateId).NotEqual(Guid.Empty).WithMessage("Field mus be a valid guid value");
        RuleFor(v => v.Name).MinimumLength(1).WithMessage("Pin ismi en az 1 karakter içermeli");
        RuleFor(v => v.RelativeX).NotNull().WithMessage("Konum x bilgisi geçersiz");
        RuleFor(v => v.RelativeY).NotNull().WithMessage("Konum y bilgisi geçersiz");
        RuleFor(v => v.Function).IsInEnum().WithMessage("Fonksiyon bilgisi geçersiz");
        RuleFor(v => v.Direction).IsInEnum().WithMessage("Yön bilgisi geçersiz");
        RuleFor(v => v.SignalLayer).IsInEnum().WithMessage("Sinyal bilgisi geçersiz");
    }
}
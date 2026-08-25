using CabinetOs.Core.Model;
using static CabinetOs.Model.Enums.EntityEnums;
using FluentValidation;

namespace CabinetOs.Model.Dtos.ComponentTemplatePin.Commands;

public class ComponentTemplatePinUpdateDto : IDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public double RelativeX { get; set; }
    public double RelativeY { get; set; }
    public HandleSide Side { get; set; }
    public int? ChannelNumber { get; set; }
    public PinFunction Function { get; set; }
    public PinDirection Direction { get; set; }
    public VoltageLevel? VoltageLevel { get; set; }
}

public class ComponentTemplatePinUpdateDtoValidator : AbstractValidator<ComponentTemplatePinUpdateDto>
{
    public ComponentTemplatePinUpdateDtoValidator()
    {
        RuleFor(v => v.Id).NotNull().WithMessage("Field cannot be null");
        RuleFor(v => v.Id).NotEqual(Guid.Empty).WithMessage("Field mus be a valid guid value");
        RuleFor(v => v.Name).MinimumLength(1).WithMessage("Pin ismi en az 1 karakter içermeli");
        RuleFor(v => v.RelativeX).InclusiveBetween(0, 1).WithMessage("Konum x 0 ile 1 arasinda olmali");
        RuleFor(v => v.RelativeY).InclusiveBetween(0, 1).WithMessage("Konum y 0 ile 1 arasinda olmali");
        RuleFor(v => v.Side).IsInEnum().WithMessage("Kenar bilgisi geçersiz");
        RuleFor(v => v.Function).IsInEnum().WithMessage("Fonksiyon bilgisi geçersiz");
        RuleFor(v => v.Direction).IsInEnum().WithMessage("Yön bilgisi geçersiz");
    }
}

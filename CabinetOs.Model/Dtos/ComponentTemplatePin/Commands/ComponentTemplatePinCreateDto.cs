using CabinetOs.Core.Model;
using FluentValidation;

namespace CabinetOs.Model.Dtos.ComponentTemplatePin.Commands
{
    public class ComponentTemplatePinCreateDto : IDto
    {
        public Guid ComponentTemplateId { get; set; }
        public string Name { get; set; } = null!;
        public double RelativeX { get; set; }
        public double RelativeY { get; set; }
        public int? ChannelNumber { get; set; }
        public int Function { get; set; }
        public int Direction { get; set; }
        public int SignalLayer { get; set; }
        public int? VoltageLevel { get; set; }
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
            RuleFor(v => v.Function).NotNull().WithMessage("Fonksiyon bilgisi geçersiz");
            RuleFor(v => v.Direction).NotNull().WithMessage("Yön bilgisi geçersiz");
            RuleFor(v => v.SignalLayer).NotNull().WithMessage("Sinyal bilgisi geçersiz");
        }
    }
}
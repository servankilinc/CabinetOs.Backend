using CabinetOs.Core.Model;
using FluentValidation;

namespace CabinetOs.Model.Dtos.ComponentTemplate.Commands
{
    public class ComponentTemplateCreateDto : IDto
    {
        public string Name { get; set; } = null!;
        public int DeviceTypeId { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public int BackgroundColor { get; set; }
        public string? BackgroundImageUrl { get; set; }
    }

    public class ComponentTemplateCreateDtoValidator : AbstractValidator<ComponentTemplateCreateDto>
    {
        public ComponentTemplateCreateDtoValidator()
        {
            RuleFor(v => v.Name).MinimumLength(2).WithMessage("En az 2 karakter içermeli");
            RuleFor(v => v.DeviceTypeId).NotNull().WithMessage("Tip bilgisi zorunlu lütfen kontrol ediniz");
            RuleFor(v => v.Width).GreaterThan(0).WithMessage("Genişlik bilgisi boş geçilemez");
            RuleFor(v => v.Height).GreaterThan(0).WithMessage("Yükseklik bilgisi boş geçilemez");
            RuleFor(v => v.BackgroundColor).InclusiveBetween(0, 0xFFFFFF).WithMessage("Arka plan rengi 0x000000 - 0xFFFFFF araliginda olmali");
        }
    }
}
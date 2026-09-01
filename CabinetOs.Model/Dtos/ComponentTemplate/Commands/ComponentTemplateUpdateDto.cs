using CabinetOs.Core.Model;
using FluentValidation;

namespace CabinetOs.Model.Dtos.ComponentTemplate.Commands
{
    public class ComponentTemplateUpdateDto : IDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public int DeviceTypeId { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string BackgroundColor { get; set; } = null!;
        public string? BackgroundImageUrl { get; set; }
        public bool IsActive { get; set; }
    }

    public class ComponentTemplateUpdateDtoValidator : AbstractValidator<ComponentTemplateUpdateDto>
    {
        public ComponentTemplateUpdateDtoValidator()
        {
            RuleFor(v => v.Id).NotNull().WithMessage("Field cannot be null");
            RuleFor(v => v.Id).NotEqual(Guid.Empty).WithMessage("Field mus be a valid guid value");
            RuleFor(v => v.Name).MinimumLength(2).WithMessage("En az 2 karakter içermeli");
            RuleFor(v => v.DeviceTypeId).NotNull().WithMessage("Tip bilgisi zorunlu lütfen kontrol ediniz");
            RuleFor(v => v.Width).GreaterThan(0).WithMessage("Genişlik bilgisi boş geçilemez");
            RuleFor(v => v.Height).GreaterThan(0).WithMessage("Yükseklik bilgisi boş geçilemez");
            RuleFor(v => v.BackgroundColor).Matches("^#[0-9A-Fa-f]{6}$").WithMessage("Arka plan rengi #RRGGBB biçiminde olmalı");
        }
    }
}
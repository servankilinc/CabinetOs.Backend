using CabinetOs.Core.Model;
using FluentValidation;

namespace CabinetOs.Model.Dtos.Device.Commands
{
    public class DeviceCreateDto : IDto
    {
        public string Name { get; set; } = null!;
        public double CoordinateX { get; set; }
        public double CoordinateY { get; set; }
        public double Rotation { get; set; }
        public int ZIndex { get; set; }
        public bool IsLocked { get; set; }
        public bool? IsVisible { get; set; } = true;
        public Guid CabinetId { get; set; }
        public Guid ComponentTemplateId { get; set; }
        public string? IpAddress { get; set; }
        public string? MacAddress { get; set; }
        public string? ExternalCode { get; set; }
    }

    public class DeviceCreateDtoValidator : AbstractValidator<DeviceCreateDto>
    {
        public DeviceCreateDtoValidator()
        {
            RuleFor(v => v.Name).MinimumLength(2).WithMessage("En az 2 karakter içermeli");
            RuleFor(v => v.CoordinateX).NotNull().WithMessage("Kordinat x geçersiz");
            RuleFor(v => v.CoordinateY).NotNull().WithMessage("Kordinat y geçersiz");
            RuleFor(v => v.Rotation).NotNull().WithMessage("Rotasyon bilgisi geçersiz");
            RuleFor(v => v.ZIndex).NotNull().WithMessage("Z index bilgisi geçersiz");
            RuleFor(v => v.IsLocked).NotNull().WithMessage("Geçersiz bilgi");
            RuleFor(v => v.CabinetId).NotNull().WithMessage("Field cannot be null");
            RuleFor(v => v.CabinetId).NotEqual(Guid.Empty).WithMessage("Field mus be a valid guid value");
            RuleFor(v => v.ComponentTemplateId).NotNull().WithMessage("Field cannot be null");
            RuleFor(v => v.ComponentTemplateId).NotEqual(Guid.Empty).WithMessage("Field mus be a valid guid value");
        }
    }
}
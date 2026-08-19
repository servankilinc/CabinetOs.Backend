using CabinetOs.Core.Model;
using FluentValidation;

namespace CabinetOs.Model.Dtos.Device.Commands
{
    public class DeviceUpdateDto : IDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public double CoordinateX { get; set; }
        public double CoordinateY { get; set; }
        public double Rotation { get; set; }
        public int ZIndex { get; set; }
        public bool IsLocked { get; set; }
        public bool IsVisible { get; set; }
        public string? IpAddress { get; set; }
        public string? MacAddress { get; set; }
        public string? ExternalCode { get; set; }
        public bool IsActive { get; set; }
    }

    public class DeviceUpdateDtoValidator : AbstractValidator<DeviceUpdateDto>
    {
        public DeviceUpdateDtoValidator()
        {
            RuleFor(v => v.Id).NotNull().WithMessage("Field cannot be null");
            RuleFor(v => v.Id).NotEqual(Guid.Empty).WithMessage("Field mus be a valid guid value");
            RuleFor(v => v.Name).MinimumLength(2).WithMessage("En az 2 karakter içermeli");
            RuleFor(v => v.CoordinateX).NotNull().WithMessage("Kordinat x geçersiz");
            RuleFor(v => v.CoordinateY).NotNull().WithMessage("Kordinat y geçersiz");
            RuleFor(v => v.Rotation).NotNull().WithMessage("Rotasyon bilgisi geçersiz");
            RuleFor(v => v.ZIndex).NotNull().WithMessage("Z index bilgisi geçersiz");
            RuleFor(v => v.IsLocked).NotNull().WithMessage("Geçersiz bilgi");
            RuleFor(v => v.IsVisible).NotNull().WithMessage("Geçersiz bilgi");
        }
    }
}
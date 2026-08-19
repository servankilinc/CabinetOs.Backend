using CabinetOs.Core.Model;
using FluentValidation;

namespace CabinetOs.Model.Dtos.Pin.Commands
{
    public class PinCreateDto : IDto
    {
        public string Name { get; set; } = null!;
        public double RelativeX { get; set; }
        public double RelativeY { get; set; }
        public Guid? IoChannelId { get; set; }
        public int Function { get; set; }
        public int SignalLayer { get; set; }
        public int? VoltageLevel { get; set; }
        public Guid DeviceId { get; set; }
    }

    public class PinCreateDtoValidator : AbstractValidator<PinCreateDto>
    {
        public PinCreateDtoValidator()
        {
            RuleFor(v => v.Name).NotEmpty().WithMessage("İsim bilgisi zorunlu");
            RuleFor(v => v.RelativeX).NotNull().WithMessage("Geçersiz kordinat x bilgisi");
            RuleFor(v => v.RelativeY).NotNull().WithMessage("Geçersiz kordinat y bilgisi");
            RuleFor(v => v.Function).NotNull().WithMessage("Geçersiz fonksiyon ataması");
            RuleFor(v => v.SignalLayer).NotNull().WithMessage("Geçersiz sinyal ataması");
            RuleFor(v => v.DeviceId).NotNull().WithMessage("Geçersiz cihaz bilgisi");
            RuleFor(v => v.DeviceId).NotEqual(Guid.Empty).WithMessage("Geçersiz cihaz bilgisi");
        }
    }
}
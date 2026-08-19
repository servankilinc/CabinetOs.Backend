using CabinetOs.Core.Model;
using FluentValidation;

namespace CabinetOs.Model.Dtos.IoChannel.Commands
{
    public class IoChannelCreateDto : IDto
    {
        public Guid DeviceId { get; set; }
        public int ChannelNumber { get; set; }
        public int Direction { get; set; }
        public bool IsEnabled { get; set; }
        public string Name { get; set; } = null!;
    }

    public class IoChannelCreateDtoValidator : AbstractValidator<IoChannelCreateDto>
    {
        public IoChannelCreateDtoValidator()
        {
            RuleFor(v => v.DeviceId).NotNull().WithMessage("Geçersiz cihaz bilgisi");
            RuleFor(v => v.DeviceId).NotEqual(Guid.Empty).WithMessage("Geçersiz cihaz bilgisi");
            RuleFor(v => v.ChannelNumber).NotEmpty().WithMessage("Kanal numarası girilmeli");
            RuleFor(v => v.Direction).NotNull().WithMessage("Geçersiz bağlantı yönü");
            RuleFor(v => v.IsEnabled).NotNull().WithMessage("Geçersiz bilgi");
            RuleFor(v => v.Name).NotEmpty().WithMessage("İsim bilgisi girilmeli");
        }
    }
}
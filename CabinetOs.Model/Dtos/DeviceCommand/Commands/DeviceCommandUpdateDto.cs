using CabinetOs.Core.Model;
using FluentValidation;

namespace CabinetOs.Model.Dtos.DeviceCommand.Commands
{
    public class DeviceCommandUpdateDto : IDto
    {
        public Guid Id { get; set; }
        public string? PayloadJson { get; set; }
        public int Status { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public string? ResultMessage { get; set; }
    }

    public class DeviceCommandUpdateDtoValidator : AbstractValidator<DeviceCommandUpdateDto>
    {
        public DeviceCommandUpdateDtoValidator()
        {
            RuleFor(v => v.Id).NotNull().WithMessage("Field cannot be null");
            RuleFor(v => v.Id).NotEqual(Guid.Empty).WithMessage("Field mus be a valid guid value");
            RuleFor(v => v.Status).NotNull().WithMessage("Komut tipi girilmeli");
        }
    }
}
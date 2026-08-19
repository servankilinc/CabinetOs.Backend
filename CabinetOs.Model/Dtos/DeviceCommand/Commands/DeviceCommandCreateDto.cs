using CabinetOs.Core.Model;
using FluentValidation;

namespace CabinetOs.Model.Dtos.DeviceCommand.Commands
{
    public class DeviceCommandCreateDto : IDto
    {
        public Guid DeviceId { get; set; }
        public Guid? IoChannelId { get; set; }
        public int CommandType { get; set; }
        public string? PayloadJson { get; set; }
        public int Status { get; set; }
        public Guid? RequestedByUserId { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public string? ResultMessage { get; set; }
    }

    public class DeviceCommandCreateDtoValidator : AbstractValidator<DeviceCommandCreateDto>
    {
        public DeviceCommandCreateDtoValidator()
        {
            RuleFor(v => v.DeviceId).NotNull().WithMessage("Cihaz bilgisi girilmeli");
            RuleFor(v => v.DeviceId).NotEqual(Guid.Empty).WithMessage("Cihaz bilgisi girilmeli");
            RuleFor(v => v.CommandType).NotNull().WithMessage("Komut tipi girilmeli");
            RuleFor(v => v.Status).NotNull().WithMessage("Durum bilgisi eksik olamaz");
        }
    }
}
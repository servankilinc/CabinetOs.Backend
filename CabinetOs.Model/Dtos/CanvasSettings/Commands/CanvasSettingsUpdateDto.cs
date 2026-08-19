using CabinetOs.Core.Model;
using FluentValidation;

namespace CabinetOs.Model.Dtos.CanvasSettings.Commands
{
    public class CanvasSettingsUpdateDto : IDto
    {
        public Guid Id { get; set; }
        public int GridSize { get; set; }
        public bool SnapToGrid { get; set; }
        public int BackgroundVariant { get; set; }
        public string GridColor { get; set; } = null!;
        public string BackgroundColor { get; set; } = null!;
        public double MinZoom { get; set; }
        public double MaxZoom { get; set; }
    }

    public class CanvasSettingsUpdateDtoValidator : AbstractValidator<CanvasSettingsUpdateDto>
    {
        public CanvasSettingsUpdateDtoValidator()
        {
            RuleFor(v => v.Id).NotNull().WithMessage("Field cannot be null");
            RuleFor(v => v.Id).NotEqual(Guid.Empty).WithMessage("Field mus be a valid guid value");
            RuleFor(v => v.GridSize).NotNull().WithMessage("Field cannot be null");
            RuleFor(v => v.SnapToGrid).NotNull().WithMessage("Field cannot be null");
            RuleFor(v => v.BackgroundVariant).NotNull().WithMessage("Field cannot be null");
            RuleFor(v => v.GridColor).NotEmpty().WithMessage("Field cannot be empty");
            RuleFor(v => v.BackgroundColor).NotEmpty().WithMessage("Field cannot be empty");
            RuleFor(v => v.MinZoom).NotNull().WithMessage("Field cannot be null");
            RuleFor(v => v.MaxZoom).NotNull().WithMessage("Field cannot be null");
        }
    }
}
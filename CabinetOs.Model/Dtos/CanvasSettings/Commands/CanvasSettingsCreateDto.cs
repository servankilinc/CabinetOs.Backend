using CabinetOs.Core.Model;
using static CabinetOs.Model.Enums.EntityEnums;
using FluentValidation;

namespace CabinetOs.Model.Dtos.CanvasSettings.Commands
{
    public class CanvasSettingsCreateDto : IDto
    {
        public int GridSize { get; set; }
        public bool SnapToGrid { get; set; }
        public BackgroundVariant BackgroundVariant { get; set; }
        public string GridColor { get; set; } = null!;
        public string BackgroundColor { get; set; } = null!;
        public double MinZoom { get; set; }
        public double MaxZoom { get; set; }
    }

    public class CanvasSettingsCreateDtoValidator : AbstractValidator<CanvasSettingsCreateDto>
    {
        public CanvasSettingsCreateDtoValidator()
        {
            RuleFor(v => v.GridSize).NotNull().WithMessage("Field cannot be null");
            RuleFor(v => v.SnapToGrid).NotNull().WithMessage("Field cannot be null");
            RuleFor(v => v.BackgroundVariant).IsInEnum().WithMessage("Field cannot be null");
            RuleFor(v => v.GridColor).NotEmpty().WithMessage("Field cannot be empty");
            RuleFor(v => v.BackgroundColor).NotEmpty().WithMessage("Field cannot be empty");
            RuleFor(v => v.MinZoom).NotNull().WithMessage("Field cannot be null");
            RuleFor(v => v.MaxZoom).NotNull().WithMessage("Field cannot be null");
        }
    }
}
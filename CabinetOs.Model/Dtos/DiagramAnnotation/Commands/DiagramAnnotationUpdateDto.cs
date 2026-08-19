using CabinetOs.Core.Model;
using FluentValidation;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Model.Dtos.DiagramAnnotation.Commands;

public class DiagramAnnotationUpdateDto : IDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public double CoordinateX { get; set; }
    public double CoordinateY { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double Rotation { get; set; }
    public int ZIndex { get; set; }
    public bool IsLocked { get; set; }
    public bool IsVisible { get; set; }
    public string BackgroundColor { get; set; } = null!;
    public string Text { get; set; } = null!;
    public AnnotationShape Shape { get; set; }
    public string FontColor { get; set; } = null!;
    public double FontSize { get; set; }
    public bool IsBold { get; set; }
    public string BorderColor { get; set; } = null!;
}

public class DiagramAnnotationUpdateDtoValidator : AbstractValidator<DiagramAnnotationUpdateDto>
{
    public DiagramAnnotationUpdateDtoValidator()
    {
        RuleFor(v => v.Id).NotNull().WithMessage("Field cannot be null");
        RuleFor(v => v.Id).NotEqual(Guid.Empty).WithMessage("Field mus be a valid guid value");
        RuleFor(v => v.Name).MinimumLength(2).WithMessage("En az 2 karakter içermeli");
        RuleFor(v => v.CoordinateX).NotNull().WithMessage("Field cannot be null");
        RuleFor(v => v.CoordinateY).NotNull().WithMessage("Field cannot be null");
        RuleFor(v => v.Width).NotNull().WithMessage("Field cannot be null");
        RuleFor(v => v.Height).NotNull().WithMessage("Field cannot be null");
        RuleFor(v => v.Rotation).NotNull().WithMessage("Field cannot be null");
        RuleFor(v => v.ZIndex).NotNull().WithMessage("Field cannot be null");
        RuleFor(v => v.IsLocked).NotNull().WithMessage("Field cannot be null");
        RuleFor(v => v.IsVisible).NotNull().WithMessage("Field cannot be null");
        RuleFor(v => v.BackgroundColor).NotEmpty().WithMessage("Arka plan rengi girmelisiniz");
        RuleFor(v => v.Text).NotEmpty().WithMessage("İçerik metni girilmesi gerekmekte");
        RuleFor(v => v.Shape).IsInEnum().WithMessage("Tip seçimi yapılmalı");
        RuleFor(v => v.FontColor).NotNull().WithMessage("Metin rengi seçilmeli");
        RuleFor(v => v.FontSize).NotNull().WithMessage("Metin boyutu girilmeli");
        RuleFor(v => v.IsBold).NotNull().WithMessage("Field cannot be null");
        RuleFor(v => v.BorderColor).NotNull().WithMessage("Kenar rengi seçilmeli");
    }
}
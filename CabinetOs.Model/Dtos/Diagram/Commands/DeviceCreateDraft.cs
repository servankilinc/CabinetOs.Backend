using CabinetOs.Core.Model;
using FluentValidation;

namespace CabinetOs.Model.Dtos.Diagram.Commands;

/// <summary> Paletten canvas'a birakilan yeni cihaz </summary>
public class DeviceCreateDraft : IDto, ITempIdDraft
{
    public string TempId { get; set; } = null!;
    public Guid ComponentTemplateId { get; set; }
    public string Name { get; set; } = null!;
    public double CoordinateX { get; set; }
    public double CoordinateY { get; set; }
    public double Rotation { get; set; }
    public int ZIndex { get; set; }
    public bool IsLocked { get; set; }
    public bool IsVisible { get; set; } = true;

    /// <summary> SCADA tarafindaki kimlik. Editorde bos birakilabilir, sonra atanir.</summary>
    public string? ExternalCode { get; set; }
}

public class DeviceCreateDraftValidator : AbstractValidator<DeviceCreateDraft>
{
    public DeviceCreateDraftValidator()
    {
        this.AddTempIdRules();

        RuleFor(v => v.ComponentTemplateId).NotEqual(Guid.Empty).WithMessage("Sablon secilmeli");
        RuleFor(v => v.Name).NotEmpty().WithMessage("Cihaz adi zorunlu");
        RuleFor(v => v.Name).MaximumLength(128).WithMessage("Cihaz adi en fazla 128 karakter olabilir");
        RuleFor(v => v.ExternalCode).MaximumLength(64).WithMessage("Dis kod en fazla 64 karakter olabilir");
    }
}

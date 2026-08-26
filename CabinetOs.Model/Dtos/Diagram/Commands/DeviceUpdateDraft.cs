using CabinetOs.Core.Model;
using FluentValidation;

namespace CabinetOs.Model.Dtos.Diagram.Commands;

/// <summary> Cihazi pasife almak buradan degil, <c>deleted</c> listesinden yapilir. </summary>
public class DeviceUpdateDraft : IDto, IIdentifiableDraft
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public double CoordinateX { get; set; }
    public double CoordinateY { get; set; }
    public double Rotation { get; set; }
    public int ZIndex { get; set; }
    public bool IsLocked { get; set; }
    public bool IsVisible { get; set; }
    public string? ExternalCode { get; set; }
}

public class DeviceUpdateDraftValidator : AbstractValidator<DeviceUpdateDraft>
{
    public DeviceUpdateDraftValidator()
    {
        RuleFor(v => v.Id).NotEqual(Guid.Empty).WithMessage("Cihaz kimligi zorunlu");
        RuleFor(v => v.Name).NotEmpty().WithMessage("Cihaz adi zorunlu");
        RuleFor(v => v.Name).MaximumLength(128).WithMessage("Cihaz adi en fazla 128 karakter olabilir");
        RuleFor(v => v.ExternalCode).MaximumLength(64).WithMessage("Dis kod en fazla 64 karakter olabilir");
    }
}

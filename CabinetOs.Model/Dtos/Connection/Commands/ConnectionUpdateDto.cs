using CabinetOs.Core.Model;
using static CabinetOs.Model.Enums.EntityEnums;
using FluentValidation;

namespace CabinetOs.Model.Dtos.Connection.Commands;

public class ConnectionUpdateDto : IDto
{
    public Guid Id { get; set; }
    public Guid SourcePinId { get; set; }
    public Guid TargetPinId { get; set; }
    public string? Label { get; set; }
    public WireType WireType { get; set; }
    public string Color { get; set; } = null!;
    public LineStyle LineStyle { get; set; }
    public double StrokeWidth { get; set; }
    public EdgeRouting Routing { get; set; }
    public string? WaypointsJson { get; set; }
    public int ZIndex { get; set; }
}

public class ConnectionUpdateDtoValidator : AbstractValidator<ConnectionUpdateDto>
{
    public ConnectionUpdateDtoValidator()
    {
        RuleFor(v => v.Id).NotNull().WithMessage("Field cannot be null");
        RuleFor(v => v.Id).NotEqual(Guid.Empty).WithMessage("Field mus be a valid guid value");
        RuleFor(v => v.SourcePinId).NotEqual(Guid.Empty).WithMessage("Kaynak pin bilgisi seçilmeli");
        RuleFor(v => v.TargetPinId).NotEqual(Guid.Empty).WithMessage("Hedef pin bilgisi seçilmeli");
        RuleFor(v => v.TargetPinId).NotEqual(v => v.SourcePinId).WithMessage("Bir pin kendisine bağlanamaz");
        RuleFor(v => v.WireType).IsInEnum().WithMessage("Kablo tipi seçilmeli");
        RuleFor(v => v.Color).NotEmpty().WithMessage("Renk bilgisi girilmeli");
        RuleFor(v => v.LineStyle).IsInEnum().WithMessage("Stil bilgisi girilmeli");
        RuleFor(v => v.Routing).IsInEnum().WithMessage("Çizim şekli geçersiz");
        RuleFor(v => v.StrokeWidth).GreaterThan(0).WithMessage("Kalınlık sıfırdan büyük olmalı");
    }
}
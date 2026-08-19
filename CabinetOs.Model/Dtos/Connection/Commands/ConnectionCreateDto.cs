using CabinetOs.Core.Model;
using FluentValidation;

namespace CabinetOs.Model.Dtos.Connection.Commands
{
    public class ConnectionCreateDto : IDto
    {
        public Guid SourcePinId { get; set; }
        public Guid TargetPinId { get; set; }
        public string Label { get; set; } = null!;
        public int WireType { get; set; }
        public string Color { get; set; } = null!;
        public int LineStyle { get; set; }
        public double StrokeWidth { get; set; }
        public string WaypointsJson { get; set; } = null!;
        public int ZIndex { get; set; }
    }

    public class ConnectionCreateDtoValidator : AbstractValidator<ConnectionCreateDto>
    {
        public ConnectionCreateDtoValidator()
        {
            RuleFor(v => v.SourcePinId).NotNull().WithMessage("Kaynak pin bilgisi seçilmeli");
            RuleFor(v => v.SourcePinId).NotEqual(Guid.Empty).WithMessage("Kaynak pin bilgisi seçilmeli");
            RuleFor(v => v.TargetPinId).NotNull().WithMessage("Hedef pin bilgisi seçilmeli");
            RuleFor(v => v.TargetPinId).NotEqual(Guid.Empty).WithMessage("Hedef pin bilgisi seçilmeli");
            RuleFor(v => v.Label).NotEmpty().WithMessage("Etiket bilgisi girilmeli");
            RuleFor(v => v.WireType).NotNull().WithMessage("Kablo tipi seçilmeli");
            RuleFor(v => v.Color).NotNull().WithMessage("Renk bilgisi girilmeli");
            RuleFor(v => v.LineStyle).NotNull().WithMessage("Stil bilgisi girilmeli");
            RuleFor(v => v.StrokeWidth).NotNull().WithMessage("Kalınlık seçilmeli");
            RuleFor(v => v.WaypointsJson).NotEmpty().WithMessage("Bağlantı verisi geçersiz");
            RuleFor(v => v.ZIndex).NotNull().WithMessage("Z index bilgisi geçersiz");
        }
    }
}
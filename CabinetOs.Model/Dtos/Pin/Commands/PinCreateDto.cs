using CabinetOs.Core.Model;
using FluentValidation;
using static CabinetOs.Model.Enums.EntityEnums;

namespace CabinetOs.Model.Dtos.Pin.Commands;

public class PinCreateDto : IDto
{
    public string Name { get; set; } = null!;
    public double RelativeX { get; set; }
    public double RelativeY { get; set; }
    public HandleSide Side { get; set; }
    public Guid? IoChannelId { get; set; }
    public PinFunction Function { get; set; }
    public PinDirection Direction { get; set; }
    public VoltageLevel? VoltageLevel { get; set; }
    /// <summary>Modul uzerindeki kanal numarasi; IoChannel eslesmesinin kaynagi.</summary>
    public int? ChannelNumber { get; set; }
    public Guid? ComponentTemplatePinId { get; set; }
    public Guid DeviceId { get; set; }
}

public class PinCreateDtoValidator : AbstractValidator<PinCreateDto>
{
    public PinCreateDtoValidator()
    {
        RuleFor(v => v.Name).NotEmpty().WithMessage("İsim bilgisi zorunlu");
        RuleFor(v => v.RelativeX).NotNull().WithMessage("Geçersiz kordinat x bilgisi");
        RuleFor(v => v.RelativeY).NotNull().WithMessage("Geçersiz kordinat y bilgisi");
        // DB'de CHECK (0..1) var; burada yakalamak 400 dondurur, yoksa insert sirasinda kisit ihlali status code 500'e cevrilir.
        RuleFor(v => v.RelativeX).InclusiveBetween(0, 1).WithMessage("Kordinat x 0 ile 1 arasinda olmali");
        RuleFor(v => v.RelativeY).InclusiveBetween(0, 1).WithMessage("Kordinat y 0 ile 1 arasinda olmali");
        RuleFor(v => v.Side).IsInEnum().WithMessage("Geçersiz kenar bilgisi");
        RuleFor(v => v.Function).IsInEnum().WithMessage("Geçersiz fonksiyon ataması");
        RuleFor(v => v.Direction).IsInEnum().WithMessage("Geçersiz yön ataması");
        RuleFor(v => v.DeviceId).NotEqual(Guid.Empty).WithMessage("Geçersiz cihaz bilgisi");
    }
}
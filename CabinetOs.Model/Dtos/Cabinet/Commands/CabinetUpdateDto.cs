using CabinetOs.Core.Model;
using FluentValidation;

namespace CabinetOs.Model.Dtos.Cabinet.Commands
{
    public class CabinetUpdateDto : IDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? LocationDescription { get; set; }
        public string? GsmIp { get; set; }
        public string? NetworkIp { get; set; }
        public string ScadaBaseUrl { get; set; } = null!;
        public int ScadaCommandTimeoutMs { get; set; }
        public bool ScadaIsEnabled { get; set; }
        public bool IsActive { get; set; }
    }

    public class CabinetUpdateDtoValidator : AbstractValidator<CabinetUpdateDto>
    {
        public CabinetUpdateDtoValidator()
        {
            RuleFor(v => v.Id).NotNull().WithMessage("Field cannot be null");
            RuleFor(v => v.Id).NotEqual(Guid.Empty).WithMessage("Field mus be a valid guid value");
            RuleFor(v => v.Name).NotNull().WithMessage("İsim bilgisi zorunlu lütfen kontrol ediniz");
            RuleFor(v => v.Name).MinimumLength(2).WithMessage("İsim bilgisi en az 2 karakter içermeli");
            RuleFor(v => v.ScadaBaseUrl).NotEmpty().WithMessage("Adres bilgisi zorunlu");
            RuleFor(v => v.ScadaCommandTimeoutMs).Must(amount => decimal.TryParse(amount.ToString(), out _)).WithMessage("Lütfen geçrli bir bilgi giriniz");
            RuleFor(v => v.ScadaCommandTimeoutMs).GreaterThan(10000).WithMessage("Zaman aşımı en az 10.000ms olabilir");
            RuleFor(v => v.ScadaIsEnabled).NotNull().WithMessage("Bilgi zorunlu lütfen kontorl ediniz");
        }
    }
}
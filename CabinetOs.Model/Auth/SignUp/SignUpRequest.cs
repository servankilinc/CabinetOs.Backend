using FluentValidation;
using CabinetOs.Core.Utils.CriticalData;

namespace CabinetOs.Model.Auth.SignUp
{
    public class SignUpRequest
    {
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public Guid CompanyId { get; set; }
        public string? PhoneNumber { get; set; }

        [CriticalData]
        public string Password { get; set; } = null!;
        public Guid? DeviceId { get; set; }
        public string ClientType { get; set; } = null!;
    }

    public class SignUpRequestValidator : AbstractValidator<SignUpRequest>
    {
        public SignUpRequestValidator()
        {
            RuleFor(b => b.Email).NotNull().NotEmpty().EmailAddress();
            RuleFor(b => b.UserName).NotNull().NotEmpty().MinimumLength(6);
            RuleFor(b => b.Password).NotNull().NotEmpty().MinimumLength(6);
            RuleFor(b => b.ClientType).NotNull().NotEmpty();
        }
    }
}
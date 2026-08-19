using CabinetOs.Core.Model;
using FluentValidation;

namespace CabinetOs.Model.Dtos.User.Commands
{
    public class UserCreateDto : IDto
    {
        public string UserName { get; set; } = null!;
        public string? Email { get; set; }
        public Guid CompanyId { get; set; }
        public string FullName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
    }

    public class UserCreateDtoValidator : AbstractValidator<UserCreateDto>
    {
        public UserCreateDtoValidator()
        {
            RuleFor(v => v.UserName).MinimumLength(4).WithMessage("Kullanıcı adı en az 4 karakter olmalı");
            RuleFor(v => v.CompanyId).NotNull().WithMessage("Firma bilgisi zorunlu kontrol ediniz");
            RuleFor(v => v.FullName).MinimumLength(4).WithMessage("Lütfen geçerli bir kullanıcı ism soyismi giriniz");
        }
    }
}
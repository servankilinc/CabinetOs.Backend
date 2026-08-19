using CabinetOs.Core.Model;
using FluentValidation;

namespace CabinetOs.Model.Dtos.Company.Commands
{
    public class CompanyCreateDto : IDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class CompanyCreateDtoValidator : AbstractValidator<CompanyCreateDto>
    {
        public CompanyCreateDtoValidator()
        {
            RuleFor(v => v.Name).MinimumLength(2).WithMessage("Firma ismi en az 2 karakter olmalı");
        }
    }
}
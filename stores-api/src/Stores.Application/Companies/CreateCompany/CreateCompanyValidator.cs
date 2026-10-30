using FluentValidation;
using Stores.Domain.Companies;
using Stores.Domain.ValueObjects;

namespace Stores.Application.Companies.CreateCompany;

public sealed class CreateCompanyValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Company.NameMaxLength);
        RuleFor(x => x.CountryCode).Must(CountryCode.IsValid).WithMessage("Must be an ISO 3166-1 alpha-2 country code.");
    }
}

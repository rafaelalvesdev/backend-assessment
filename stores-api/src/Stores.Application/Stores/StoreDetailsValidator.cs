using FluentValidation;
using Stores.Domain.Stores;
using Stores.Domain.ValueObjects;

namespace Stores.Application.Stores;

public abstract class StoreDetailsValidator<T> : AbstractValidator<T> where T : IStoreDetails
{
    protected StoreDetailsValidator()
    {
        RuleFor(x => x.Code)
            .Must(Store.IsValidCode)
            .WithMessage($"Must be 1-{Store.CodeMaxLength} characters of letters, digits, '-' or '_'.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Store.NameMaxLength);
        RuleFor(x => x.Currency).Must(CurrencyCode.IsValid).WithMessage("Must be an ISO 4217 currency code.");
        RuleFor(x => x.TimeZone).Must(TimeZoneId.IsValid).WithMessage("Must be an IANA time zone, e.g. 'Europe/Lisbon'.");
        RuleFor(x => x.Culture).Must(CultureCode.IsValid).WithMessage("Must be a specific culture name, e.g. 'pt-BR'.");
        RuleFor(x => x.Address).NotNull().SetValidator(new AddressValidator());
    }

    private sealed class AddressValidator : AbstractValidator<AddressInput>
    {
        public AddressValidator()
        {
            RuleFor(x => x.Street).NotEmpty().MaximumLength(Address.StreetMaxLength);
            RuleFor(x => x.City).NotEmpty().MaximumLength(Address.CityMaxLength);
            RuleFor(x => x.State).MaximumLength(Address.StateMaxLength);
            RuleFor(x => x.PostalCode).MaximumLength(Address.PostalCodeMaxLength);
            RuleFor(x => x.CountryCode).Must(CountryCode.IsValid).WithMessage("Must be an ISO 3166-1 alpha-2 country code.");
        }
    }
}

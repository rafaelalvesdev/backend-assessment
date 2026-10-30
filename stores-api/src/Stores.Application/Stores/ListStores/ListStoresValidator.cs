using FluentValidation;
using Stores.Application.Common;
using Stores.Domain.ValueObjects;

namespace Stores.Application.Stores.ListStores;

public sealed class ListStoresValidator : AbstractValidator<ListStoresQuery>
{
    public ListStoresValidator()
    {
        this.PageRules(x => x.Page, x => x.PageSize);
        RuleFor(x => x.CountryCode)
            .Must(CountryCode.IsValid)
            .When(x => x.CountryCode is not null)
            .WithMessage("Must be an ISO 3166-1 alpha-2 country code.");
    }
}

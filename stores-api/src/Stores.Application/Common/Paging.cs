using FluentValidation;

namespace Stores.Application.Common;

public static class Paging
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static void PageRules<T>(this AbstractValidator<T> validator, Func<T, int> page, Func<T, int> pageSize)
    {
        validator.RuleFor(x => page(x)).GreaterThanOrEqualTo(1).OverridePropertyName("Page");
        validator.RuleFor(x => pageSize(x)).InclusiveBetween(1, MaxPageSize).OverridePropertyName("PageSize");
    }
}

using FluentValidation;

namespace Products.Functions.Products;

public sealed class ProductQueryValidator : AbstractValidator<ProductQuery>
{
    public ProductQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, ProductQuery.MaxPageSize);
    }
}

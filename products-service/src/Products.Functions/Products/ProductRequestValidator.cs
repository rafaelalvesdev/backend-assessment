using FluentValidation;

namespace Products.Functions.Products;

public sealed class ProductRequestValidator : AbstractValidator<ProductRequest>
{
    public ProductRequestValidator()
    {
        RuleFor(x => x.StoreId).NotEmpty();
        RuleFor(x => x.Sku)
            .NotEmpty()
            .Matches("^[A-Za-z0-9._-]{1,50}$")
            .WithMessage("Must be 1-50 characters of letters, digits, '.', '-' or '_'.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).PrecisionScale(19, 4, ignoreTrailingZeros: true);
        RuleFor(x => x.Currency).Must(CurrencyCodes.IsValid).WithMessage("Must be an ISO 4217 currency code.");
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
    }
}

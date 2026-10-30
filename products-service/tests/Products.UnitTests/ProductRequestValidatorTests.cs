using System.Globalization;
using Products.Functions.Products;

namespace Products.UnitTests;

public class ProductRequestValidatorTests
{
    private readonly ProductRequestValidator _validator = new();

    [Fact]
    public void Valid_request_passes()
    {
        Assert.True(_validator.Validate(TestData.Request()).IsValid);
    }

    [Fact]
    public void Reports_each_invalid_field()
    {
        var request = new ProductRequest(Guid.Empty, "bad sku!", "", new string('x', 2001), -1m, "REAIS", -5, true);

        var fields = _validator.Validate(request).Errors.Select(e => e.PropertyName).Distinct().Order();

        Assert.Equal(["Currency", "Description", "Name", "Price", "Sku", "StockQuantity", "StoreId"], fields);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("0.0001")]
    [InlineData("999999999999999.9999")]
    public void Accepts_prices_that_fit_the_column(string price)
    {
        var request = TestData.Request(price: decimal.Parse(price, CultureInfo.InvariantCulture));

        Assert.True(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Rejects_prices_that_overflow_the_column()
    {
        Assert.False(_validator.Validate(TestData.Request(price: 1_000_000_000_000_000m)).IsValid);
    }

    [Fact]
    public void Rejects_more_than_four_decimal_places()
    {
        Assert.False(_validator.Validate(TestData.Request(price: 1.00001m)).IsValid);
    }

    [Fact]
    public void Rejects_unknown_currency()
    {
        Assert.False(_validator.Validate(TestData.Request(currency: "XYZ")).IsValid);
    }
}

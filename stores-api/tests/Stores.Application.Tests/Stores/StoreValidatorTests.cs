using Stores.Application.Stores;
using Stores.Application.Stores.CreateStore;

namespace Stores.Application.Tests.Stores;

public class StoreValidatorTests
{
    private readonly CreateStoreValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var result = _validator.Validate(TestData.CreateCommand(Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Reports_every_invalid_field()
    {
        var command = new CreateStoreCommand(
            Guid.NewGuid(),
            "bad code!",
            "",
            new AddressInput("", "Lisboa", null, null, "XX"),
            "EURO",
            "Europe/Atlantis",
            "xx-YY");

        var result = _validator.Validate(command);

        var fields = result.Errors.Select(e => e.PropertyName).Distinct().Order().ToArray();
        Assert.Equal(
            ["Address.CountryCode", "Address.Street", "Code", "Culture", "Currency", "Name", "TimeZone"],
            fields);
    }

    [Fact]
    public void Address_is_required()
    {
        var command = TestData.CreateCommand(Guid.NewGuid()) with { Address = null! };

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == "Address");
    }
}

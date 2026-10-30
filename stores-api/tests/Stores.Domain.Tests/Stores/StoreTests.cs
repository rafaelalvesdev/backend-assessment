using Stores.Domain.Common;
using Stores.Domain.Stores;
using Stores.Domain.ValueObjects;

namespace Stores.Domain.Tests.Stores;

public class StoreTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid CompanyId = Guid.NewGuid();

    private static Address SaoPauloAddress() =>
        new("Av. Paulista, 1000", "São Paulo", "SP", "01310-100", CountryCode.From("BR"));

    private static Store NewStore(string code = "sp-001", string name = "Paulista") =>
        Store.Create(
            CompanyId,
            code,
            name,
            SaoPauloAddress(),
            CurrencyCode.From("BRL"),
            TimeZoneId.From("America/Sao_Paulo"),
            CultureCode.From("pt-BR"),
            Now);

    [Fact]
    public void Create_sets_properties_and_timestamps()
    {
        var store = NewStore();

        Assert.NotEqual(Guid.Empty, store.Id);
        Assert.Equal(CompanyId, store.CompanyId);
        Assert.Equal("SP-001", store.Code);
        Assert.Equal("Paulista", store.Name);
        Assert.Equal("São Paulo", store.Address.City);
        Assert.Equal("BRL", store.Currency.Value);
        Assert.Equal("America/Sao_Paulo", store.TimeZone.Value);
        Assert.Equal("pt-BR", store.Culture.Value);
        Assert.Equal(Now, store.CreatedAt);
        Assert.Equal(Now, store.UpdatedAt);
    }

    [Fact]
    public void Create_requires_a_company()
    {
        Assert.Throws<DomainException>(() => Store.Create(
            Guid.Empty, "SP-001", "Paulista", SaoPauloAddress(),
            CurrencyCode.From("BRL"), TimeZoneId.From("America/Sao_Paulo"), CultureCode.From("pt-BR"), Now));
    }

    [Theory]
    [InlineData("", "Paulista")]
    [InlineData("SP-001", " ")]
    [InlineData("has space", "Paulista")]
    public void Create_rejects_invalid_code_or_name(string code, string name)
    {
        Assert.Throws<DomainException>(() => NewStore(code, name));
    }

    [Fact]
    public void Update_replaces_details_and_bumps_updated_at()
    {
        var store = NewStore();
        var later = Now.AddHours(3);
        var lisbon = new Address("Rua Augusta, 10", "Lisboa", null, "1100-053", CountryCode.From("PT"));

        store.Update("lx-001", "Baixa", lisbon, CurrencyCode.From("EUR"),
            TimeZoneId.From("Europe/Lisbon"), CultureCode.From("pt-PT"), later);

        Assert.Equal("LX-001", store.Code);
        Assert.Equal("Baixa", store.Name);
        Assert.Equal("PT", store.Address.Country.Value);
        Assert.Null(store.Address.State);
        Assert.Equal("EUR", store.Currency.Value);
        Assert.Equal(Now, store.CreatedAt);
        Assert.Equal(later, store.UpdatedAt);
    }
}

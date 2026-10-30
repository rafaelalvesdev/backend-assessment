using Stores.Application.Stores;
using Stores.Application.Stores.CreateStore;
using Stores.Application.Stores.UpdateStore;
using Stores.Domain.Companies;
using Stores.Domain.Stores;
using Stores.Domain.ValueObjects;

namespace Stores.Application.Tests;

internal static class TestData
{
    public static readonly DateTimeOffset Now = new(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);

    public static Company Company() => Domain.Companies.Company.Create("Acme Retail", CountryCode.From("BR"), Now);

    public static Store Store(Guid companyId, string code = "SP-001") =>
        Domain.Stores.Store.Create(
            companyId,
            code,
            "Paulista",
            new Address("Av. Paulista, 1000", "São Paulo", "SP", "01310-100", CountryCode.From("BR")),
            CurrencyCode.From("BRL"),
            TimeZoneId.From("America/Sao_Paulo"),
            CultureCode.From("pt-BR"),
            Now);

    public static AddressInput LisbonAddress() => new("Rua Augusta, 10", "Lisboa", null, "1100-053", "PT");

    public static CreateStoreCommand CreateCommand(Guid companyId, string code = "LX-001") =>
        new(companyId, code, "Baixa", LisbonAddress(), "EUR", "Europe/Lisbon", "pt-PT");

    public static UpdateStoreCommand UpdateCommand(Guid companyId, Guid storeId, string code = "LX-001") =>
        new(companyId, storeId, code, "Baixa", LisbonAddress(), "EUR", "Europe/Lisbon", "pt-PT");
}

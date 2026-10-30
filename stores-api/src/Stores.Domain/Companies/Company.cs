using Stores.Domain.Common;
using Stores.Domain.ValueObjects;

namespace Stores.Domain.Companies;

public sealed class Company
{
    public const int NameMaxLength = 150;

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public CountryCode Country { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }

    private Company() { }

    public static Company Create(string name, CountryCode country, DateTimeOffset now) => new()
    {
        Id = Guid.NewGuid(),
        Name = Guard.Required(name, "Name", NameMaxLength),
        Country = country,
        CreatedAt = now
    };
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stores.Domain.Companies;
using Stores.Domain.Stores;
using Stores.Domain.ValueObjects;

namespace Stores.Infrastructure.Persistence.Configurations;

internal sealed class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("Stores");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(s => s.Code).HasMaxLength(Store.CodeMaxLength).IsUnicode(false).IsRequired();
        builder.HasIndex(s => new { s.CompanyId, s.Code }).IsUnique();

        builder.Property(s => s.Name).HasMaxLength(Store.NameMaxLength).IsRequired();

        builder.Property(s => s.Currency)
            .HasColumnName("CurrencyCode")
            .HasConversion(ValueConverters.Currency)
            .HasMaxLength(3)
            .IsUnicode(false)
            .IsFixedLength();
        builder.Property(s => s.TimeZone)
            .HasColumnName("TimeZoneId")
            .HasConversion(ValueConverters.TimeZone)
            .HasMaxLength(64)
            .IsUnicode(false);
        builder.Property(s => s.Culture)
            .HasConversion(ValueConverters.Culture)
            .HasMaxLength(16)
            .IsUnicode(false);

        builder.ComplexProperty(s => s.Address, address =>
        {
            address.Property(a => a.Street).HasColumnName("Street").HasMaxLength(Address.StreetMaxLength);
            address.Property(a => a.City).HasColumnName("City").HasMaxLength(Address.CityMaxLength);
            address.Property(a => a.State).HasColumnName("State").HasMaxLength(Address.StateMaxLength);
            address.Property(a => a.PostalCode).HasColumnName("PostalCode").HasMaxLength(Address.PostalCodeMaxLength);
            address.Property(a => a.Country)
                .HasColumnName("CountryCode")
                .HasConversion(ValueConverters.Country)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
        });
    }
}

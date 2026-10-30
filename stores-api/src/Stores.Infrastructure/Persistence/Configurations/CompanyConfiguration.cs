using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stores.Domain.Companies;

namespace Stores.Infrastructure.Persistence.Configurations;

internal sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();
        builder.Property(c => c.Name).HasMaxLength(Company.NameMaxLength).IsRequired();
        builder.Property(c => c.Country)
            .HasColumnName("CountryCode")
            .HasConversion(ValueConverters.Country)
            .HasMaxLength(2)
            .IsUnicode(false)
            .IsFixedLength();
    }
}

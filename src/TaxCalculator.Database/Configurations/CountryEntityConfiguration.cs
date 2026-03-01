using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxCalculator.Database.Entities;

namespace TaxCalculator.Database.Configurations;

public class CountryEntityConfiguration : IEntityTypeConfiguration<CountryEntity>
{
    public void Configure(EntityTypeBuilder<CountryEntity> builder)
    {
        builder.ToTable("Countries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(3);

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.HasOne(x => x.TaxConfiguration)
            .WithOne(x => x.Country)
            .HasForeignKey<CountryTaxConfigurationEntity>(x => x.CountryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

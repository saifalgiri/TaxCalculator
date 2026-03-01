using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxCalculator.Database.Entities;

namespace TaxCalculator.Database.Configurations;

public class CountryTaxConfigurationEntityConfiguration
    : IEntityTypeConfiguration<CountryTaxConfigurationEntity>
{
    public void Configure(EntityTypeBuilder<CountryTaxConfigurationEntity> builder)
    {
        builder.ToTable("CountryTaxConfigurations");
        builder.HasKey(x => x.Id);

        // CountryId is the FK to Countries — unique because one config per country
        builder.HasIndex(x => x.CountryId)
            .IsUnique();

        builder.HasMany(x => x.TaxItems)
            .WithOne(x => x.CountryTaxConfiguration)
            .HasForeignKey(x => x.CountryTaxConfigurationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

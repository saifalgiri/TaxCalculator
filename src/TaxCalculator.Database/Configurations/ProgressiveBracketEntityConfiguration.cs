using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxCalculator.Database.Entities;

namespace TaxCalculator.Database.Configurations;

public class ProgressiveBracketEntityConfiguration
    : IEntityTypeConfiguration<ProgressiveBracketEntity>
{
    public void Configure(EntityTypeBuilder<ProgressiveBracketEntity> builder)
    {
        builder.ToTable("ProgressiveBrackets");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Threshold)
            .HasPrecision(18, 4);

        builder.Property(x => x.Rate)
            .IsRequired()
            .HasPrecision(8, 6);
    }
}

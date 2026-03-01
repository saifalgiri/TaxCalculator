using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxCalculator.Database.Entities;

namespace TaxCalculator.Database.Configurations;

public class TaxItemEntityConfiguration : IEntityTypeConfiguration<TaxItemEntity>
{
    public void Configure(EntityTypeBuilder<TaxItemEntity> builder)
    {
        builder.ToTable("TaxItems");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 4);

        builder.Property(x => x.Rate)
            .HasPrecision(8, 6);

        builder.HasMany(x => x.Brackets)
            .WithOne(x => x.TaxItem)
            .HasForeignKey(x => x.TaxItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

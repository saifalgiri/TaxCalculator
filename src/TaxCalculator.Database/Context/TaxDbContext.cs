using Microsoft.EntityFrameworkCore;
using TaxCalculator.Database.Configurations;
using TaxCalculator.Database.Entities;

namespace TaxCalculator.Database.Context;

public class TaxDbContext : DbContext
{
    public TaxDbContext(DbContextOptions<TaxDbContext> options) : base(options) { }

    public DbSet<CountryEntity> Countries => Set<CountryEntity>();
    public DbSet<CountryTaxConfigurationEntity> CountryTaxConfigurations => Set<CountryTaxConfigurationEntity>();
    public DbSet<TaxItemEntity> TaxItems => Set<TaxItemEntity>();
    public DbSet<ProgressiveBracketEntity> ProgressiveBrackets => Set<ProgressiveBracketEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CountryEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CountryTaxConfigurationEntityConfiguration());
        modelBuilder.ApplyConfiguration(new TaxItemEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ProgressiveBracketEntityConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}

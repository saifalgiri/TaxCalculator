using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaxCalculator.Database.Entities;
using TaxCalculator.Domain.Services;
using TaxCalculator.Domain.TaxCalculators;
using Xunit;

namespace TaxCalculator.Tests.Services;

public sealed class FixedTaxServiceTests
{
    private static FixedTaxService CreateService(TaxCalculator.Database.Context.TaxDbContext db) =>
        new(db, new FixedTaxCalculator());

    private static (TaxCalculator.Database.Context.TaxDbContext db, int countryId) SeedDb(
        string countryCode, List<TaxItemEntity> taxItems)
    {
        var db = DbContextFactory.Create();
        var country = new CountryEntity { Code = countryCode };
        db.Countries.Add(country);
        db.SaveChanges();

        var config = new CountryTaxConfigurationEntity { CountryId = country.Id, TaxItems = taxItems };
        db.CountryTaxConfigurations.Add(config);
        db.SaveChanges();
        return (db, country.Id);
    }

    [Fact]
    public async Task CalculateAsync_TwoFixedItems_ReturnsSumAndCorrectTaxableBase()
    {
        var (db, countryId) = SeedDb("DE", new()
        {
            new TaxItemEntity { Name = "CommunityTax", Type = "Fixed", Amount = 1_500m },
            new TaxItemEntity { Name = "RadioTax",     Type = "Fixed", Amount = 500m   },
        });

        var (items, taxableBase) = await CreateService(db).CalculateAsync(countryId, 62_000m);

        items.Should().HaveCount(2);
        taxableBase.Should().Be(60_000m);
    }

    [Fact]
    public async Task CalculateAsync_NoFixedItems_TaxableBaseEqualsGross()
    {
        var (db, countryId) = SeedDb("FR", new()
        {
            new TaxItemEntity { Name = "FlatTax", Type = "FlatRate", Rate = 0.20m }
        });

        var (items, taxableBase) = await CreateService(db).CalculateAsync(countryId, 50_000m);

        items.Should().BeEmpty();
        taxableBase.Should().Be(50_000m);
    }

    [Fact]
    public async Task CalculateAsync_FixedExceedsGross_TaxableBaseIsZero()
    {
        var (db, countryId) = SeedDb("US", new()
        {
            new TaxItemEntity { Name = "HugeFee", Type = "Fixed", Amount = 100_000m }
        });

        var (_, taxableBase) = await CreateService(db).CalculateAsync(countryId, 50_000m);

        taxableBase.Should().Be(0m);
    }

    [Fact]
    public async Task SaveAsync_PersistsOnlyFixedItems()
    {
        var db = DbContextFactory.Create();

        // Seed a country and config header first
        var country = new CountryEntity { Code = "ES" };
        db.Countries.Add(country);
        db.SaveChanges();
        var header = new CountryTaxConfigurationEntity { CountryId = country.Id };
        db.CountryTaxConfigurations.Add(header);
        db.SaveChanges();

        var config = new CountryTaxConfigurationEntity
        {
            CountryId = country.Id,
            TaxItems  = new()
            {
                new TaxItemEntity { Name = "FixedFee", Type = "Fixed",    Amount = 500m,  CountryTaxConfigurationId = header.Id },
                new TaxItemEntity { Name = "FlatTax",  Type = "FlatRate", Rate   = 0.10m, CountryTaxConfigurationId = header.Id },
            }
        };

        await CreateService(db).SaveAsync(config);

        var saved = await db.TaxItems.ToListAsync();
        saved.Should().HaveCount(1);
        saved[0].Name.Should().Be("FixedFee");
    }
}

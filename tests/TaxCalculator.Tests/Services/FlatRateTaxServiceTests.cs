using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaxCalculator.Database.Entities;
using TaxCalculator.Domain.Services;
using TaxCalculator.Domain.TaxCalculators;
using Xunit;

namespace TaxCalculator.Tests.Services;

public sealed class FlatRateTaxServiceTests
{
    private static FlatRateTaxService CreateService(TaxCalculator.Database.Context.TaxDbContext db) =>
        new(db, new FlatRateTaxCalculator());

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
    public async Task CalculateAsync_20PercentRate_On60000_Returns12000()
    {
        var (db, countryId) = SeedDb("DE", new()
        {
            new TaxItemEntity { Name = "PensionTax", Type = "FlatRate", Rate = 0.20m }
        });

        var results = await CreateService(db).CalculateAsync(countryId, 60_000m);

        results.Should().HaveCount(1);
        results[0].Amount.Should().Be(12_000m);
    }

    [Fact]
    public async Task CalculateAsync_MultipleRates_ReturnsEachAmount()
    {
        var (db, countryId) = SeedDb("ES", new()
        {
            new TaxItemEntity { Name = "Tax1", Type = "FlatRate", Rate = 0.10m },
            new TaxItemEntity { Name = "Tax2", Type = "FlatRate", Rate = 0.15m },
        });

        var results = await CreateService(db).CalculateAsync(countryId, 40_000m);

        results.Should().HaveCount(2);
        results.Single(r => r.Item.Name == "Tax1").Amount.Should().Be(4_000m);
        results.Single(r => r.Item.Name == "Tax2").Amount.Should().Be(6_000m);
    }

    [Fact]
    public async Task SaveAsync_PersistsOnlyFlatRateItems()
    {
        var db = DbContextFactory.Create();
        var country = new CountryEntity { Code = "IT" };
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
                new TaxItemEntity { Name = "FlatTax",  Type = "FlatRate", Rate   = 0.20m, CountryTaxConfigurationId = header.Id },
            }
        };

        await CreateService(db).SaveAsync(config);

        var saved = await db.TaxItems.ToListAsync();
        saved.Should().HaveCount(1);
        saved[0].Name.Should().Be("FlatTax");
    }
}

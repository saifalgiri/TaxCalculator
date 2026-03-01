using FluentAssertions;
using TaxCalculator.Database.Entities;
using TaxCalculator.Domain.Services;
using TaxCalculator.Domain.TaxCalculators;
using Xunit;

namespace TaxCalculator.Tests.Services;

public sealed class ProgressiveTaxServiceTests
{
    private static ProgressiveTaxService CreateService(TaxCalculator.Database.Context.TaxDbContext db) =>
        new(db, new ProgressiveTaxCalculator());

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
    public async Task CalculateAsync_SpecBrackets_60000_Returns16000()
    {
        var (db, countryId) = SeedDb("DE", new()
        {
            new TaxItemEntity
            {
                Name = "IncomeTax", Type = "Progressive",
                Brackets = new()
                {
                    new ProgressiveBracketEntity { Threshold = 10_000m, Rate = 0.00m },
                    new ProgressiveBracketEntity { Threshold = 30_000m, Rate = 0.20m },
                    new ProgressiveBracketEntity { Threshold = null,    Rate = 0.40m },
                }
            }
        });

        var results = await CreateService(db).CalculateAsync(countryId, 60_000m);

        results.Should().HaveCount(1);
        results[0].Amount.Should().Be(16_000m);
    }

    [Fact]
    public async Task CalculateAsync_TaxableBaseBelowFirstThreshold_ReturnsZero()
    {
        var (db, countryId) = SeedDb("FR", new()
        {
            new TaxItemEntity
            {
                Name = "IncomeTax", Type = "Progressive",
                Brackets = new()
                {
                    new ProgressiveBracketEntity { Threshold = 10_000m, Rate = 0.00m },
                    new ProgressiveBracketEntity { Threshold = null,    Rate = 0.20m },
                }
            }
        });

        var results = await CreateService(db).CalculateAsync(countryId, 5_000m);

        results.Should().HaveCount(1);
        results[0].Amount.Should().Be(0m);
    }
}

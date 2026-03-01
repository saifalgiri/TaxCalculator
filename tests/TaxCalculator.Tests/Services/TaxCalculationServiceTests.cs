using FluentAssertions;
using TaxCalculator.Database.Entities;
using TaxCalculator.Domain.Exceptions;
using TaxCalculator.Domain.Services;
using TaxCalculator.Domain.TaxCalculators;
using Xunit;

namespace TaxCalculator.Tests.Services;

public sealed class TaxCalculationServiceTests
{
    private static TaxCalculationService CreateService(TaxCalculator.Database.Context.TaxDbContext db)
    {
        var fixedSvc = new FixedTaxService(db, new FixedTaxCalculator());
        var flatSvc  = new FlatRateTaxService(db, new FlatRateTaxCalculator());
        var progSvc  = new ProgressiveTaxService(db, new ProgressiveTaxCalculator());
        return new TaxCalculationService(db, fixedSvc, flatSvc, progSvc);
    }

    private static TaxCalculator.Database.Context.TaxDbContext SeedDb(
        string countryCode, List<TaxItemEntity> taxItems)
    {
        var db = DbContextFactory.Create();
        var country = new CountryEntity { Code = countryCode };
        db.Countries.Add(country);
        db.SaveChanges();

        var config = new CountryTaxConfigurationEntity { CountryId = country.Id, TaxItems = taxItems };
        db.CountryTaxConfigurations.Add(config);
        db.SaveChanges();
        return db;
    }

    [Fact]
    public async Task CalculateAsync_SpecShowcase_ReturnsCorrectTotals()
    {
        var db = SeedDb("DE", new()
        {
            new TaxItemEntity { Name = "CommunityTax", Type = "Fixed",    Amount = 1_500m },
            new TaxItemEntity { Name = "RadioTax",     Type = "Fixed",    Amount = 500m   },
            new TaxItemEntity { Name = "PensionTax",   Type = "FlatRate", Rate   = 0.20m  },
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

        var result = await CreateService(db).CalculateAsync("DE", 62_000m);

        result.GrossSalary.Should().Be(62_000m);
        result.TaxableBase.Should().Be(60_000m);
        result.TotalTaxes.Should().Be(30_000m);
        result.NetSalary.Should().Be(32_000m);
        result.Breakdown.Should().HaveCount(4);
        result.Breakdown.Single(b => b.Name == "CommunityTax").Amount.Should().Be(1_500m);
        result.Breakdown.Single(b => b.Name == "RadioTax").Amount.Should().Be(500m);
        result.Breakdown.Single(b => b.Name == "PensionTax").Amount.Should().Be(12_000m);
        result.Breakdown.Single(b => b.Name == "IncomeTax").Amount.Should().Be(16_000m);
    }

    [Fact]
    public async Task CalculateAsync_CountryNotFound_ThrowsCountryConfigurationNotFoundException()
    {
        var db = DbContextFactory.Create();
        var act = () => CreateService(db).CalculateAsync("XX", 50_000m);
        await act.Should().ThrowAsync<CountryConfigurationNotFoundException>().WithMessage("*XX*");
    }

    [Fact]
    public async Task CalculateAsync_CountryCodeNormalisedToUpperCase()
    {
        var db = SeedDb("ES", new() { new TaxItemEntity { Name = "Fee", Type = "Fixed", Amount = 1_000m } });
        var result = await CreateService(db).CalculateAsync("es", 50_000m);
        result.TotalTaxes.Should().Be(1_000m);
    }

    [Fact]
    public async Task CalculateAsync_FixedTaxesExceedGross_TaxableBaseIsZero()
    {
        var db = SeedDb("US", new()
        {
            new TaxItemEntity { Name = "HugeFixed", Type = "Fixed",    Amount = 100_000m },
            new TaxItemEntity { Name = "FlatTax",   Type = "FlatRate", Rate   = 0.10m   },
        });

        var result = await CreateService(db).CalculateAsync("US", 50_000m);

        result.TaxableBase.Should().Be(0m);
        result.Breakdown.Single(b => b.Name == "FlatTax").Amount.Should().Be(0m);
    }
}

using AutoMapper;
using FluentAssertions;
using TaxCalculator.API.Mapping;
using TaxCalculator.Models.Enums;
using TaxCalculator.Models.Requests;
using TaxCalculator.Domain.Interfaces;
using Xunit;

namespace TaxCalculator.Tests.Validators;

public sealed class TaxMappingProfileTests
{
    private readonly IMapper _mapper;

    public TaxMappingProfileTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<TaxMappingProfile>());
        config.AssertConfigurationIsValid();
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void AutoMapper_Configuration_IsValid() =>
        _mapper.Should().NotBeNull();

    [Fact]
    public void Map_ConfigureTaxRuleRequest_CountryCodeUpperCasedInCountryNavigation()
    {
        var request = new ConfigureTaxRuleRequest
        {
            CountryCode = "de",
            TaxItems = new() { new TaxItemRequest { Name = "Fee", Type = TaxItemType.Fixed, Amount = 100m } }
        };

        var entity = _mapper.Map<TaxCalculator.Database.Entities.CountryTaxConfigurationEntity>(request);

        // CountryCode is mapped into the Country navigation property, not a direct column
        entity.Country.Should().NotBeNull();
        entity.Country.Code.Should().Be("DE");
        // CountryId is left as default (0) — service resolves real FK via DB upsert
        entity.CountryId.Should().Be(0);
    }

    [Fact]
    public void Map_FlatRateRequest_RateConvertedToFraction()
    {
        var request = new ConfigureTaxRuleRequest
        {
            CountryCode = "DE",
            TaxItems = new() { new TaxItemRequest { Name = "PensionTax", Type = TaxItemType.FlatRate, Rate = 20m } }
        };

        var entity = _mapper.Map<TaxCalculator.Database.Entities.CountryTaxConfigurationEntity>(request);

        entity.TaxItems.Single().Rate.Should().Be(0.20m);
    }

    [Fact]
    public void Map_ProgressiveBracket_RateConvertedToFraction()
    {
        var request = new ConfigureTaxRuleRequest
        {
            CountryCode = "DE",
            TaxItems = new()
            {
                new TaxItemRequest
                {
                    Name = "IncomeTax", Type = TaxItemType.Progressive,
                    Brackets = new()
                    {
                        new ProgressiveBracketRequest { Threshold = 10_000m, Rate = 0m  },
                        new ProgressiveBracketRequest { Threshold = null,    Rate = 40m },
                    }
                }
            }
        };

        var entity = _mapper.Map<TaxCalculator.Database.Entities.CountryTaxConfigurationEntity>(request);
        var brackets = entity.TaxItems.Single().Brackets;

        brackets[0].Rate.Should().Be(0.00m);
        brackets[1].Rate.Should().Be(0.40m);
    }

    [Fact]
    public void Map_TaxCalculationResult_ToResponse_MapsAllFields()
    {
        var result = new TaxCalculationResult
        {
            GrossSalary = 62_000m,
            TaxableBase = 60_000m,
            TotalTaxes  = 30_000m,
            NetSalary   = 32_000m,
            Breakdown   = new()
            {
                new TaxItemResult { Name = "IncomeTax", Type = "Progressive", Amount = 16_000m }
            }
        };

        var response = _mapper.Map<TaxCalculator.Models.Responses.CalculateTaxResponse>(result);

        response.GrossSalary.Should().Be(62_000m);
        response.NetSalary.Should().Be(32_000m);
        response.Breakdown.Single().Type.Should().Be(TaxItemType.Progressive);
        response.Breakdown.Single().Amount.Should().Be(16_000m);
    }
}

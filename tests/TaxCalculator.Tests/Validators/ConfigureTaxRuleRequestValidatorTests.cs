using FluentAssertions;
using FluentValidation.TestHelper;
using TaxCalculator.Models.Enums;
using TaxCalculator.Models.Requests;
using TaxCalculator.Models.Validators;
using Xunit;

namespace TaxCalculator.Tests.Validators;

public sealed class ConfigureTaxRuleRequestValidatorTests
{
    private readonly ConfigureTaxRuleRequestValidator _sut = new();

    private static ConfigureTaxRuleRequest ValidRequest(List<TaxItemRequest>? items = null) => new()
    {
        CountryCode = "DE",
        TaxItems = items ?? new()
        {
            new TaxItemRequest { Name = "Fee", Type = TaxItemType.Fixed, Amount = 500m }
        }
    };

    [Fact]
    public void ValidRequest_PassesValidation()
    {
        var result = _sut.TestValidate(ValidRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CountryCode_Empty_FailsValidation(string? code)
    {
        var baseReq = ValidRequest();
        var r = _sut.TestValidate(new ConfigureTaxRuleRequest
        {
            CountryCode = code!,
            TaxItems = baseReq.TaxItems
        });
        r.ShouldHaveValidationErrorFor(x => x.CountryCode);
    }

    [Theory]
    [InlineData("D")]     // too short
    [InlineData("DEUS")]  // too long
    [InlineData("D1")]    // contains digit
    public void CountryCode_InvalidFormat_FailsValidation(string code)
    {
        var baseReq = ValidRequest();
        var r = _sut.TestValidate(new ConfigureTaxRuleRequest
        {
            CountryCode = code!,
            TaxItems = baseReq.TaxItems
        });
        r.ShouldHaveValidationErrorFor(x => x.CountryCode);
    }

    [Fact]
    public void TaxItems_Empty_FailsValidation()
    {
        var r = _sut.TestValidate(ValidRequest(new()));
        r.ShouldHaveValidationErrorFor(x => x.TaxItems);
    }

    [Fact]
    public void MultipleProgressiveItems_FailsValidation()
    {
        var items = new List<TaxItemRequest>
        {
            new()
            {
                Name = "P1", Type = TaxItemType.Progressive,
                Brackets = new()
                {
                    new ProgressiveBracketRequest { Threshold = 10_000m, Rate = 10m },
                    new ProgressiveBracketRequest { Threshold = null,    Rate = 40m }
                }
            },
            new()
            {
                Name = "P2", Type = TaxItemType.Progressive,
                Brackets = new()
                {
                    new ProgressiveBracketRequest { Threshold = 20_000m, Rate = 15m },
                    new ProgressiveBracketRequest { Threshold = null,    Rate = 50m }
                }
            }
        };

        var r = _sut.TestValidate(ValidRequest(items));
        r.ShouldHaveValidationErrorFor(x => x.TaxItems);
    }

    [Fact]
    public void FixedItem_NullAmount_FailsValidation()
    {
        var items = new List<TaxItemRequest>
        {
            new() { Name = "Fee", Type = TaxItemType.Fixed, Amount = null }
        };
        var r = _sut.TestValidate(ValidRequest(items));
        r.ShouldHaveValidationErrorFor("TaxItems[0].Amount");
    }

    [Fact]
    public void FixedItem_NegativeAmount_FailsValidation()
    {
        var items = new List<TaxItemRequest>
        {
            new() { Name = "Fee", Type = TaxItemType.Fixed, Amount = -100m }
        };
        var r = _sut.TestValidate(ValidRequest(items));
        r.ShouldHaveValidationErrorFor("TaxItems[0].Amount");
    }

    [Fact]
    public void FlatRateItem_NullRate_FailsValidation()
    {
        var items = new List<TaxItemRequest>
        {
            new() { Name = "Tax", Type = TaxItemType.FlatRate, Rate = null }
        };
        var r = _sut.TestValidate(ValidRequest(items));
        r.ShouldHaveValidationErrorFor("TaxItems[0].Rate");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void FlatRateItem_RateOutOfRange_FailsValidation(decimal rate)
    {
        var items = new List<TaxItemRequest>
        {
            new() { Name = "Tax", Type = TaxItemType.FlatRate, Rate = rate }
        };
        var r = _sut.TestValidate(ValidRequest(items));
        r.ShouldHaveValidationErrorFor("TaxItems[0].Rate");
    }

    [Fact]
    public void ProgressiveItem_NoBrackets_FailsValidation()
    {
        var items = new List<TaxItemRequest>
        {
            new() { Name = "IncomeTax", Type = TaxItemType.Progressive, Brackets = new() }
        };
        var r = _sut.TestValidate(ValidRequest(items));
        r.ShouldHaveValidationErrorFor("TaxItems[0].Brackets");
    }

    [Fact]
    public void ProgressiveItem_OpenEndedBracketNotLast_FailsValidation()
    {
        var items = new List<TaxItemRequest>
        {
            new()
            {
                Name = "IncomeTax", Type = TaxItemType.Progressive,
                Brackets = new()
                {
                    new ProgressiveBracketRequest { Threshold = null,    Rate = 40m },
                    new ProgressiveBracketRequest { Threshold = 30_000m, Rate = 20m },
                }
            }
        };
        var r = _sut.TestValidate(ValidRequest(items));
        r.ShouldHaveValidationErrorFor("TaxItems[0].Brackets");
    }

    [Fact]
    public void ProgressiveItem_ValidBrackets_PassesValidation()
    {
        var items = new List<TaxItemRequest>
        {
            new()
            {
                Name = "IncomeTax", Type = TaxItemType.Progressive,
                Brackets = new()
                {
                    new ProgressiveBracketRequest { Threshold = 10_000m, Rate = 0m  },
                    new ProgressiveBracketRequest { Threshold = 30_000m, Rate = 20m },
                    new ProgressiveBracketRequest { Threshold = null,    Rate = 40m },
                }
            }
        };
        var r = _sut.TestValidate(ValidRequest(items));
        r.ShouldNotHaveAnyValidationErrors();
    }
}

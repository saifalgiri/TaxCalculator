using FluentAssertions;
using TaxCalculator.Database.Entities;
using TaxCalculator.Domain.Interfaces;
using TaxCalculator.Domain.TaxCalculators;
using Xunit;

namespace TaxCalculator.Tests.TaxCalculators;

public sealed class ProgressiveTaxCalculatorTests
{
    private readonly ProgressiveTaxCalculator _sut = new();

    // Standard brackets from the spec example
    private static TaxItemEntity SpecBrackets() => new()
    {
        Name = "IncomeTax",
        Type = "Progressive",
        Brackets = new()
        {
            new ProgressiveBracketEntity { Threshold = 10_000m, Rate = 0.00m },
            new ProgressiveBracketEntity { Threshold = 30_000m, Rate = 0.20m },
            new ProgressiveBracketEntity { Threshold = null,    Rate = 0.40m },
        }
    };

    [Fact]
    public void TaxItemType_Is_Progressive() =>
        _sut.TaxItemType.Should().Be("Progressive");

    [Fact]
    public void CanHandle_ProgressiveType_ReturnsTrue() =>
        _sut.CanHandle(new TaxItemEntity { Type = "Progressive" }).Should().BeTrue();

    [Fact]
    public void CanHandle_IsCaseInsensitive() =>
        _sut.CanHandle(new TaxItemEntity { Type = "progressive" }).Should().BeTrue();

    [Fact]
    public void Calculate_SpecShowcase_60000_Returns_16000()
    {
        // 10k@0%=0, 20k@20%=4000, 30k@40%=12000 → total 16,000
        var context = new TaxCalculationContext { GrossSalary = 62_000m, TaxableBase = 60_000m };

        _sut.Calculate(SpecBrackets(), context).Should().Be(16_000m);
    }

    [Fact]
    public void Calculate_TaxableBaseBelowFirstThreshold_ReturnsZero()
    {
        var context = new TaxCalculationContext { GrossSalary = 5_000m, TaxableBase = 5_000m };
        // All of 5k falls in 0% bracket
        _sut.Calculate(SpecBrackets(), context).Should().Be(0m);
    }

    [Fact]
    public void Calculate_TaxableBaseExactlyAtFirstThreshold()
    {
        var context = new TaxCalculationContext { GrossSalary = 10_000m, TaxableBase = 10_000m };
        // 10k @ 0% = 0
        _sut.Calculate(SpecBrackets(), context).Should().Be(0m);
    }

    [Fact]
    public void Calculate_TaxableBaseInSecondBracket()
    {
        var context = new TaxCalculationContext { GrossSalary = 20_000m, TaxableBase = 20_000m };
        // 10k@0% + 10k@20% = 2,000
        _sut.Calculate(SpecBrackets(), context).Should().Be(2_000m);
    }

    [Fact]
    public void Calculate_NoBrackets_ThrowsInvalidOperationException()
    {
        var item = new TaxItemEntity { Name = "Bad", Type = "Progressive", Brackets = new() };
        var context = new TaxCalculationContext { GrossSalary = 50_000m, TaxableBase = 50_000m };

        var act = () => _sut.Calculate(item, context);
        act.Should().Throw<InvalidOperationException>().WithMessage("*brackets*");
    }

    [Fact]
    public void Calculate_NullBrackets_ThrowsInvalidOperationException()
    {
        var item = new TaxItemEntity { Name = "Bad", Type = "Progressive", Brackets = null! };
        var context = new TaxCalculationContext { GrossSalary = 50_000m, TaxableBase = 50_000m };

        var act = () => _sut.Calculate(item, context);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Calculate_ZeroTaxableBase_ReturnsZero()
    {
        var context = new TaxCalculationContext { GrossSalary = 0m, TaxableBase = 0m };
        _sut.Calculate(SpecBrackets(), context).Should().Be(0m);
    }

    [Fact]
    public void Calculate_BracketsAppliedInOrderEvenIfUnordered()
    {
        // Brackets intentionally out of order — calculator sorts them
        var item = new TaxItemEntity
        {
            Name = "IncomeTax", Type = "Progressive",
            Brackets = new()
            {
                new ProgressiveBracketEntity { Threshold = null,    Rate = 0.40m },  // last after sort
                new ProgressiveBracketEntity { Threshold = 30_000m, Rate = 0.20m },
                new ProgressiveBracketEntity { Threshold = 10_000m, Rate = 0.00m },
            }
        };

        var context = new TaxCalculationContext { GrossSalary = 62_000m, TaxableBase = 60_000m };
        _sut.Calculate(item, context).Should().Be(16_000m);
    }
}

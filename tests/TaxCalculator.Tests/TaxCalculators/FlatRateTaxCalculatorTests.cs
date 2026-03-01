using FluentAssertions;
using TaxCalculator.Database.Entities;
using TaxCalculator.Domain.Interfaces;
using TaxCalculator.Domain.TaxCalculators;
using Xunit;

namespace TaxCalculator.Tests.TaxCalculators;

public sealed class FlatRateTaxCalculatorTests
{
    private readonly FlatRateTaxCalculator _sut = new();

    [Fact]
    public void TaxItemType_Is_FlatRate() =>
        _sut.TaxItemType.Should().Be("FlatRate");

    [Fact]
    public void CanHandle_FlatRateType_ReturnsTrue() =>
        _sut.CanHandle(new TaxItemEntity { Type = "FlatRate" }).Should().BeTrue();

    [Fact]
    public void CanHandle_IsCaseInsensitive() =>
        _sut.CanHandle(new TaxItemEntity { Type = "flatrate" }).Should().BeTrue();

    [Fact]
    public void CanHandle_FixedType_ReturnsFalse() =>
        _sut.CanHandle(new TaxItemEntity { Type = "Fixed" }).Should().BeFalse();

    [Fact]
    public void Calculate_20Percent_Of_60000_Returns_12000()
    {
        var item = new TaxItemEntity { Name = "PensionTax", Type = "FlatRate", Rate = 0.20m };
        var context = new TaxCalculationContext { GrossSalary = 62_000m, TaxableBase = 60_000m };

        _sut.Calculate(item, context).Should().Be(12_000m);
    }

    [Fact]
    public void Calculate_ZeroRate_ReturnsZero()
    {
        var item = new TaxItemEntity { Name = "ZeroTax", Type = "FlatRate", Rate = 0m };
        var context = new TaxCalculationContext { GrossSalary = 50_000m, TaxableBase = 50_000m };

        _sut.Calculate(item, context).Should().Be(0m);
    }

    [Fact]
    public void Calculate_RateIsNull_ThrowsInvalidOperationException()
    {
        var item = new TaxItemEntity { Name = "Bad", Type = "FlatRate", Rate = null };
        var context = new TaxCalculationContext { GrossSalary = 50_000m, TaxableBase = 50_000m };

        var act = () => _sut.Calculate(item, context);
        act.Should().Throw<InvalidOperationException>().WithMessage("*Rate*");
    }

    [Theory]
    [InlineData(0.10, 40_000, 4_000)]
    [InlineData(0.25, 80_000, 20_000)]
    [InlineData(0.50, 100_000, 50_000)]
    public void Calculate_VariousRates(decimal rate, decimal taxableBase, decimal expected)
    {
        var item = new TaxItemEntity { Name = "Tax", Type = "FlatRate", Rate = rate };
        var context = new TaxCalculationContext { GrossSalary = taxableBase, TaxableBase = taxableBase };

        _sut.Calculate(item, context).Should().Be(expected);
    }
}

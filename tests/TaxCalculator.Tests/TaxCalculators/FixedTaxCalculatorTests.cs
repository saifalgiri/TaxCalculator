using FluentAssertions;
using TaxCalculator.Database.Entities;
using TaxCalculator.Domain.Interfaces;
using TaxCalculator.Domain.TaxCalculators;
using Xunit;

namespace TaxCalculator.Tests.TaxCalculators;

public sealed class FixedTaxCalculatorTests
{
    private readonly FixedTaxCalculator _sut = new();

    [Fact]
    public void TaxItemType_Is_Fixed() =>
        _sut.TaxItemType.Should().Be("Fixed");

    [Fact]
    public void CanHandle_FixedType_ReturnsTrue() =>
        _sut.CanHandle(new TaxItemEntity { Type = "Fixed" }).Should().BeTrue();

    [Fact]
    public void CanHandle_FlatRateType_ReturnsFalse() =>
        _sut.CanHandle(new TaxItemEntity { Type = "FlatRate" }).Should().BeFalse();

    [Fact]
    public void CanHandle_IsCaseInsensitive() =>
        _sut.CanHandle(new TaxItemEntity { Type = "fixed" }).Should().BeTrue();

    [Fact]
    public void Calculate_ReturnsFixedAmount_RegardlessOfTaxableBase()
    {
        var item = new TaxItemEntity { Name = "CommunityTax", Type = "Fixed", Amount = 1500m };
        var context = new TaxCalculationContext { GrossSalary = 62_000m, TaxableBase = 60_000m };

        _sut.Calculate(item, context).Should().Be(1500m);
    }

    [Fact]
    public void Calculate_AmountIsNull_ThrowsInvalidOperationException()
    {
        var item = new TaxItemEntity { Name = "Bad", Type = "Fixed", Amount = null };
        var context = new TaxCalculationContext { GrossSalary = 50_000m, TaxableBase = 50_000m };

        var act = () => _sut.Calculate(item, context);
        act.Should().Throw<InvalidOperationException>().WithMessage("*Amount*");
    }

    [Fact]
    public void Calculate_ZeroAmount_ReturnsZero()
    {
        var item = new TaxItemEntity { Name = "ZeroFee", Type = "Fixed", Amount = 0m };
        var context = new TaxCalculationContext { GrossSalary = 10_000m, TaxableBase = 10_000m };

        _sut.Calculate(item, context).Should().Be(0m);
    }
}

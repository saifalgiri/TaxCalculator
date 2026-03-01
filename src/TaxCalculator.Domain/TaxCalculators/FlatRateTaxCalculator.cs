using TaxCalculator.Database.Entities;
using TaxCalculator.Domain.Interfaces;

namespace TaxCalculator.Domain.TaxCalculators;

/// <summary>
/// Calculates a flat percentage applied to the taxable base.
/// Rate is stored as a decimal fraction (e.g. 0.20 = 20%).
/// </summary>
public sealed class FlatRateTaxCalculator : ITaxCalculator
{
    public string TaxItemType => "FlatRate";

    public bool CanHandle(TaxItemEntity taxItem) =>
        taxItem.Type.Equals(TaxItemType, StringComparison.OrdinalIgnoreCase);

    public decimal Calculate(TaxItemEntity taxItem, TaxCalculationContext context)
    {
        if (taxItem.Rate is null)
            throw new InvalidOperationException(
                $"FlatRate tax item '{taxItem.Name}' is missing the Rate field.");

        return context.TaxableBase * taxItem.Rate.Value;
    }
}

using TaxCalculator.Database.Entities;
using TaxCalculator.Domain.Interfaces;

namespace TaxCalculator.Domain.TaxCalculators;

/// <summary>
/// Calculates a fixed monetary deduction.
/// The fixed amount is returned directly regardless of taxable base.
/// </summary>
public sealed class FixedTaxCalculator : ITaxCalculator
{
    public string TaxItemType => "Fixed";

    public bool CanHandle(TaxItemEntity taxItem) =>
        taxItem.Type.Equals(TaxItemType, StringComparison.OrdinalIgnoreCase);

    public decimal Calculate(TaxItemEntity taxItem, TaxCalculationContext context)
    {
        if (taxItem.Amount is null)
            throw new InvalidOperationException(
                $"Fixed tax item '{taxItem.Name}' is missing the Amount field.");

        return taxItem.Amount.Value;
    }
}

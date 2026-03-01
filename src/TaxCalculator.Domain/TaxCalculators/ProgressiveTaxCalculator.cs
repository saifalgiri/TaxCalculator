using TaxCalculator.Database.Entities;
using TaxCalculator.Domain.Interfaces;

namespace TaxCalculator.Domain.TaxCalculators;

/// <summary>
/// Calculates a progressive (bracket) tax applied incrementally to the taxable base.
///
/// Algorithm:
///   For each bracket, determine the income slice that falls within [previousThreshold, threshold].
///   Multiply that slice by the bracket rate and accumulate.
///
/// Example with TaxableBase = 60,000:
///   Bracket 1: 0 – 10,000  @ 0%  →      0
///   Bracket 2: 10,000 – 30,000 @ 20% →  4,000
///   Bracket 3: 30,000+ @ 40%    → 12,000
///   Total = 16,000
/// </summary>
public sealed class ProgressiveTaxCalculator : ITaxCalculator
{
    public string TaxItemType => "Progressive";

    public bool CanHandle(TaxItemEntity taxItem) =>
        taxItem.Type.Equals(TaxItemType, StringComparison.OrdinalIgnoreCase);

    public decimal Calculate(TaxItemEntity taxItem, TaxCalculationContext context)
    {
        if (taxItem.Brackets is null || taxItem.Brackets.Count == 0)
            throw new InvalidOperationException(
                $"Progressive tax item '{taxItem.Name}' has no brackets configured.");

        var taxableBase = context.TaxableBase;
        decimal totalTax = 0m;
        decimal previousThreshold = 0m;

        // Brackets must be ordered by threshold ascending (nulls last)
        var orderedBrackets = taxItem.Brackets
            .OrderBy(b => b.Threshold ?? decimal.MaxValue)
            .ToList();

        foreach (var bracket in orderedBrackets)
        {
            if (taxableBase <= previousThreshold)
                break;

            // For open-ended bracket (null threshold), use full remaining amount
            var upperBound = bracket.Threshold ?? taxableBase;
            var amountInBracket = Math.Min(taxableBase, upperBound) - previousThreshold;

            if (amountInBracket > 0)
                totalTax += amountInBracket * bracket.Rate;

            previousThreshold = bracket.Threshold ?? upperBound;
        }

        return totalTax;
    }
}

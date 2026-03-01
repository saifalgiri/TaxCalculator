using TaxCalculator.Database.Entities;

namespace TaxCalculator.Domain.Interfaces;

/// <summary>
/// SRP + OCP: each tax type owns its own calculation logic in isolation.
/// New tax types = new class, no existing code changed.
/// </summary>
public interface ITaxCalculator
{
    /// <summary>The type discriminator this calculator handles (e.g. "Fixed").</summary>
    string TaxItemType { get; }

    bool CanHandle(TaxItemEntity taxItem);

    decimal Calculate(TaxItemEntity taxItem, TaxCalculationContext context);
}

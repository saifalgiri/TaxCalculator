namespace TaxCalculator.Domain.Interfaces;

/// <summary>
/// Immutable context passed to each ITaxCalculator during calculation.
/// Decouples calculators from the orchestration pipeline.
/// </summary>
public sealed class TaxCalculationContext
{
    public decimal GrossSalary { get; init; }
    public decimal TaxableBase { get; init; }
}

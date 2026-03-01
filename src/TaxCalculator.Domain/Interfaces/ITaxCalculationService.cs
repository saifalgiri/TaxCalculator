using TaxCalculator.Database.Entities;

namespace TaxCalculator.Domain.Interfaces;

/// <summary>
/// Orchestrates the full tax calculation pipeline by coordinating the three
/// typed tax services: fixed → taxable base → flat-rate + progressive.
/// </summary>
public interface ITaxCalculationService
{
    /// <summary>
    /// Returns gross salary, taxable base, total taxes, net salary, and a per-item breakdown.
    /// </summary>
    Task<TaxCalculationResult> CalculateAsync(
        string countryCode,
        decimal grossSalary,
        CancellationToken cancellationToken = default);
}

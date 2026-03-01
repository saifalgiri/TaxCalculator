using TaxCalculator.Database.Entities;

namespace TaxCalculator.Domain.Interfaces;

/// <summary>
/// Owns all DB interaction and calculation logic for Fixed tax items.
/// Returns the total fixed tax amount and the resulting taxable base.
/// </summary>
public interface IFixedTaxService
{
    /// <summary>Persists fixed tax items belonging to the given configuration.</summary>
    Task SaveAsync(
        CountryTaxConfigurationEntity configuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates the sum of all fixed tax items and derives the taxable base.
    /// Queries by countryId (the FK, not the code string) for a direct index lookup.
    /// Returns (items, taxableBase).
    /// </summary>
    Task<(List<TaxItemEntity> Items, decimal TaxableBase)> CalculateAsync(
        int countryId,
        decimal grossSalary,
        CancellationToken cancellationToken = default);
}

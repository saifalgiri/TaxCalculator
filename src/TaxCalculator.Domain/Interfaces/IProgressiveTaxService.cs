using TaxCalculator.Database.Entities;

namespace TaxCalculator.Domain.Interfaces;

/// <summary>
/// Owns all DB interaction and calculation logic for Progressive tax items.
/// Only one progressive item per country is enforced at the API validation layer.
/// </summary>
public interface IProgressiveTaxService
{
    /// <summary>Persists progressive tax items (with brackets) belonging to the given configuration.</summary>
    Task SaveAsync(
        CountryTaxConfigurationEntity configuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates progressive taxes against the provided taxable base.
    /// Queries by countryId (the FK, not the code string) for a direct index lookup.
    /// </summary>
    Task<List<(TaxItemEntity Item, decimal Amount)>> CalculateAsync(
        int countryId,
        decimal taxableBase,
        CancellationToken cancellationToken = default);
}

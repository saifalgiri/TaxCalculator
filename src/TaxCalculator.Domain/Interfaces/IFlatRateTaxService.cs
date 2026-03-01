using TaxCalculator.Database.Entities;

namespace TaxCalculator.Domain.Interfaces;

/// <summary>
/// Owns all DB interaction and calculation logic for FlatRate tax items.
/// </summary>
public interface IFlatRateTaxService
{
    /// <summary>Persists flat-rate tax items belonging to the given configuration.</summary>
    Task SaveAsync(
        CountryTaxConfigurationEntity configuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates flat-rate taxes against the provided taxable base.
    /// Queries by countryId (the FK, not the code string) for a direct index lookup.
    /// </summary>
    Task<List<(TaxItemEntity Item, decimal Amount)>> CalculateAsync(
        int countryId,
        decimal taxableBase,
        CancellationToken cancellationToken = default);
}

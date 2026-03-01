using TaxCalculator.Database.Entities;

namespace TaxCalculator.Domain.Interfaces;

/// <summary>
/// Orchestrates persisting a complete country tax configuration.
/// Delegates to the three typed tax services for their own storage.
/// </summary>
public interface ITaxConfigurationService
{
    Task<string> SaveConfigurationAsync(
        CountryTaxConfigurationEntity configuration,
        CancellationToken cancellationToken = default);
}

namespace TaxCalculator.Domain.Interfaces;

/// <summary>
/// TODO (future — Tax Credits):
/// Inject into ITaxCalculationService. After all tax items are summed,
/// call GetTotalCreditsAsync(employeeId) and subtract from TotalTaxes.
/// </summary>
public interface ITaxCreditService
{
    // Task<decimal> GetTotalCreditsAsync(string employeeId, CancellationToken ct = default);
}

/// <summary>
/// TODO (future — External Providers):
/// Inject into ITaxConfigurationService or ITaxCalculationService.
/// If the DB returns no config for a country, fall back to this provider.
/// </summary>
public interface IExternalTaxConfigurationProvider
{
    // Task<CountryTaxConfigurationEntity?> GetAsync(string countryCode, CancellationToken ct = default);
}

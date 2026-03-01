using Microsoft.EntityFrameworkCore;
using TaxCalculator.Database.Context;
using TaxCalculator.Domain.Exceptions;
using TaxCalculator.Domain.Interfaces;

namespace TaxCalculator.Domain.Services;

/// <summary>
/// Orchestrates the full calculation pipeline:
///   1. Resolve CountryId from the country code (single indexed lookup on Countries table).
///   2. Verify a tax configuration exists for that CountryId.
///   3. Delegate fixed tax calculation using CountryId → obtain taxable base.
///   4. Delegate flat-rate tax calculation using CountryId against taxable base.
///   5. Delegate progressive tax calculation using CountryId against taxable base.
///   6. Aggregate into TaxCalculationResult.
///
/// All typed services receive an int CountryId — no string country-code joins anywhere downstream.
/// </summary>
public sealed class TaxCalculationService : ITaxCalculationService
{
    private readonly TaxDbContext _db;
    private readonly IFixedTaxService _fixedTaxService;
    private readonly IFlatRateTaxService _flatRateTaxService;
    private readonly IProgressiveTaxService _progressiveTaxService;

    // TODO (future): inject ITaxCreditService here
    // TODO (future): inject IExternalTaxConfigurationProvider here

    public TaxCalculationService(
        TaxDbContext db,
        IFixedTaxService fixedTaxService,
        IFlatRateTaxService flatRateTaxService,
        IProgressiveTaxService progressiveTaxService)
    {
        _db = db;
        _fixedTaxService = fixedTaxService;
        _flatRateTaxService = flatRateTaxService;
        _progressiveTaxService = progressiveTaxService;
        // TODO: Logs 
    }

    public async Task<TaxCalculationResult> CalculateAsync(
        string countryCode,
        decimal grossSalary,
        CancellationToken cancellationToken = default)
    {
        countryCode = countryCode.ToUpperInvariant();

        // Step 1: resolve CountryId — single index seek on Countries.Code
        var country = await _db.Countries
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == countryCode, cancellationToken);

        // TODO (future - External Providers):
        // if (country is null) country = await _externalProvider.GetAsync(countryCode, cancellationToken);

        if (country is null)
            throw new CountryConfigurationNotFoundException(countryCode);

        // Step 2: verify a tax configuration exists for this country
        var configExists = await _db.CountryTaxConfigurations
            .AsNoTracking()
            .AnyAsync(c => c.CountryId == country.Id, cancellationToken);

        if (!configExists)
            throw new CountryConfigurationNotFoundException(countryCode);

        // Step 3: fixed taxes — yields the taxable base; all downstream calls use country.Id
        var (fixedItems, taxableBase) = await _fixedTaxService
            .CalculateAsync(country.Id, grossSalary, cancellationToken);

        var breakdown = new List<TaxItemResult>();

        foreach (var item in fixedItems)
            breakdown.Add(new TaxItemResult { Name = item.Name, Type = item.Type, Amount = item.Amount ?? 0m });

        // Step 4: flat-rate taxes
        var flatRateResults = await _flatRateTaxService
            .CalculateAsync(country.Id, taxableBase, cancellationToken);

        foreach (var (item, amount) in flatRateResults)
            breakdown.Add(new TaxItemResult { Name = item.Name, Type = item.Type, Amount = amount });

        // Step 5: progressive taxes
        var progressiveResults = await _progressiveTaxService
            .CalculateAsync(country.Id, taxableBase, cancellationToken);

        foreach (var (item, amount) in progressiveResults)
            breakdown.Add(new TaxItemResult { Name = item.Name, Type = item.Type, Amount = amount });

        var totalTaxes = breakdown.Sum(b => b.Amount);

        // TODO (future - Tax Credits):
        // var credits = await _taxCreditService.GetTotalCreditsAsync(employeeId, cancellationToken);
        // totalTaxes -= credits;

        return new TaxCalculationResult
        {
            GrossSalary = grossSalary,
            TaxableBase = taxableBase,
            TotalTaxes = totalTaxes,
            NetSalary = grossSalary - totalTaxes,
            Breakdown = breakdown
        };
    }
}

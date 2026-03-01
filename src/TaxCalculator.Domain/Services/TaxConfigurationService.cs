using Microsoft.EntityFrameworkCore;
using TaxCalculator.Database.Context;
using TaxCalculator.Database.Entities;
using TaxCalculator.Domain.Interfaces;

namespace TaxCalculator.Domain.Services;

/// <summary>
/// Orchestrates upsert of a full country configuration:
///   1. Upsert the CountryEntity row (insert if new, reuse Id if existing).
///   2. Delete any existing CountryTaxConfigurationEntity for that CountryId (cascades to items + brackets).
///   3. Insert a fresh CountryTaxConfigurationEntity linked by CountryId.
///   4. Delegate each tax type's items to its specialist service.
///
/// SRP: knows only about coordination and the config header — not about tax math.
/// </summary>
public sealed class TaxConfigurationService : ITaxConfigurationService
{
    private readonly TaxDbContext _db;
    private readonly IFixedTaxService _fixedTaxService;
    private readonly IFlatRateTaxService _flatRateTaxService;
    private readonly IProgressiveTaxService _progressiveTaxService;

    public TaxConfigurationService(
        TaxDbContext db,
        IFixedTaxService fixedTaxService,
        IFlatRateTaxService flatRateTaxService,
        IProgressiveTaxService progressiveTaxService)
    {
        _db = db;
        _fixedTaxService = fixedTaxService;
        _flatRateTaxService = flatRateTaxService;
        _progressiveTaxService = progressiveTaxService;
    }

    public async Task<string> SaveConfigurationAsync(
        CountryTaxConfigurationEntity configuration,
        CancellationToken cancellationToken = default)
    {
        var countryCode = configuration.Country.Code.ToUpperInvariant();

        // Step 1: Upsert CountryEntity — get or create by code
        var country = await _db.Countries
            .FirstOrDefaultAsync(c => c.Code == countryCode, cancellationToken);

        if (country is null)
        {
            country = new CountryEntity { Code = countryCode };
            _db.Countries.Add(country);
            await _db.SaveChangesAsync(cancellationToken);
        }

        // Step 2: Remove existing tax configuration for this country (cascade deletes items + brackets)
        var existing = await _db.CountryTaxConfigurations
            .Include(c => c.TaxItems)
                .ThenInclude(t => t.Brackets)
            .FirstOrDefaultAsync(c => c.CountryId == country.Id, cancellationToken);

        if (existing is not null)
            _db.CountryTaxConfigurations.Remove(existing);

        // Step 3: Insert fresh configuration header linked by CountryId
        var header = new CountryTaxConfigurationEntity { CountryId = country.Id };
        _db.CountryTaxConfigurations.Add(header);
        await _db.SaveChangesAsync(cancellationToken);

        // Step 4: Assign the generated FK to all tax items before delegating
        foreach (var item in configuration.TaxItems)
            item.CountryTaxConfigurationId = header.Id;

        // Each typed service saves only its own item type
        await _fixedTaxService.SaveAsync(configuration, cancellationToken);
        await _flatRateTaxService.SaveAsync(configuration, cancellationToken);
        await _progressiveTaxService.SaveAsync(configuration, cancellationToken);

        return countryCode;
    }
}

using Microsoft.EntityFrameworkCore;
using TaxCalculator.Database.Context;
using TaxCalculator.Database.Entities;
using TaxCalculator.Domain.Interfaces;
using TaxCalculator.Domain.TaxCalculators;

namespace TaxCalculator.Domain.Services;

/// <summary>
/// Owns DB reads/writes and calculation for FlatRate tax items.
/// Queries use CountryId (int FK) — no string country-code joins.
/// </summary>
public sealed class FlatRateTaxService : IFlatRateTaxService
{
    private readonly TaxDbContext _db;
    private readonly FlatRateTaxCalculator _calculator;

    public FlatRateTaxService(TaxDbContext db, FlatRateTaxCalculator calculator)
    {
        _db = db;
        _calculator = calculator;
    }

    public async Task SaveAsync(
        CountryTaxConfigurationEntity configuration,
        CancellationToken cancellationToken = default)
    {
        var flatRateItems = configuration.TaxItems
            .Where(t => t.Type.Equals("FlatRate", StringComparison.OrdinalIgnoreCase))
            .ToList();

        _db.TaxItems.AddRange(flatRateItems);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<(TaxItemEntity Item, decimal Amount)>> CalculateAsync(
        int countryId,
        decimal taxableBase,
        CancellationToken cancellationToken = default)
    {
        var items = await _db.TaxItems
            .AsNoTracking()
            .Where(t => t.CountryTaxConfiguration.CountryId == countryId
                        && t.Type == "FlatRate")
            .ToListAsync(cancellationToken);

        var context = new TaxCalculationContext { GrossSalary = taxableBase, TaxableBase = taxableBase };

        return items
            .Select(item => (item, _calculator.Calculate(item, context)))
            .ToList();
    }
}

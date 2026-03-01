using Microsoft.EntityFrameworkCore;
using TaxCalculator.Database.Context;
using TaxCalculator.Database.Entities;
using TaxCalculator.Domain.Interfaces;
using TaxCalculator.Domain.TaxCalculators;

namespace TaxCalculator.Domain.Services;

/// <summary>
/// Owns DB reads/writes and calculation for Fixed tax items.
/// Queries use CountryTaxConfigurationId (int FK) — no string country-code joins.
/// </summary>
public sealed class FixedTaxService : IFixedTaxService
{
    private readonly TaxDbContext _db;
    private readonly FixedTaxCalculator _calculator;

    public FixedTaxService(TaxDbContext db, FixedTaxCalculator calculator)
    {
        _db = db;
        _calculator = calculator;
    }

    public async Task SaveAsync(
        CountryTaxConfigurationEntity configuration,
        CancellationToken cancellationToken = default)
    {
        var fixedItems = configuration.TaxItems
            .Where(t => t.Type.Equals("Fixed", StringComparison.OrdinalIgnoreCase))
            .ToList();

        _db.TaxItems.AddRange(fixedItems);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<(List<TaxItemEntity> Items, decimal TaxableBase)> CalculateAsync(
        int countryId,
        decimal grossSalary,
        CancellationToken cancellationToken = default)
    {
        // Join through CountryTaxConfigurations → Countries using the int FK — no string scan
        var fixedItems = await _db.TaxItems
            .AsNoTracking()
            .Where(t => t.CountryTaxConfiguration.CountryId == countryId
                        && t.Type == "Fixed")
            .ToListAsync(cancellationToken);

        var context = new TaxCalculationContext { GrossSalary = grossSalary, TaxableBase = grossSalary };
        var fixedTotal = fixedItems.Sum(item => _calculator.Calculate(item, context));
        var taxableBase = Math.Max(0m, grossSalary - fixedTotal);

        return (fixedItems, taxableBase);
    }
}

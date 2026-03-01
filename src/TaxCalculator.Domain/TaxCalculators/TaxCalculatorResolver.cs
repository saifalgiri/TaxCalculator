using TaxCalculator.Database.Entities;
using TaxCalculator.Domain.Exceptions;
using TaxCalculator.Domain.Interfaces;

namespace TaxCalculator.Domain.TaxCalculators;

/// <summary>
/// Resolves the correct ITaxCalculator for a given TaxItemEntity.
/// All registered calculators are injected via DI (IEnumerable&lt;ITaxCalculator&gt;),
/// so adding a new tax type only requires registering a new ITaxCalculator — no changes here.
/// </summary>
public sealed class TaxCalculatorResolver
{
    private readonly IEnumerable<ITaxCalculator> _calculators;

    public TaxCalculatorResolver(IEnumerable<ITaxCalculator> calculators) =>
        _calculators = calculators;

    public ITaxCalculator Resolve(TaxItemEntity taxItem)
    {
        var calculator = _calculators.FirstOrDefault(c => c.CanHandle(taxItem));

        if (calculator is null)
            throw new InvalidTaxConfigurationException(
                $"No calculator registered for tax type '{taxItem.Type}'.");

        return calculator;
    }
}


namespace TaxCalculator.Domain.Interfaces;

/// <summary>
/// Carries the full result of a tax calculation out of the service layer.
/// The API layer maps this to CalculateTaxResponse via AutoMapper.
/// </summary>
public sealed class TaxCalculationResult
{
    public decimal GrossSalary { get; init; }
    public decimal TaxableBase { get; init; }
    public decimal TotalTaxes { get; init; }
    public decimal NetSalary { get; init; }
    public List<TaxItemResult> Breakdown { get; init; } = new();
}

public sealed class TaxItemResult
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public decimal Amount { get; init; }
}

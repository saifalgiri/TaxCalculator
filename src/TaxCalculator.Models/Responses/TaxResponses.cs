using TaxCalculator.Models.Enums;

namespace TaxCalculator.Models.Responses;

public sealed class ConfigureTaxRuleResponse
{
    public string CountryCode { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

public sealed class CalculateTaxResponse
{
    public decimal GrossSalary { get; init; }
    public decimal TaxableBase { get; init; }
    public decimal TotalTaxes { get; init; }
    public decimal NetSalary { get; init; }
    public List<TaxBreakdownItemResponse> Breakdown { get; init; } = new();
}

public sealed class TaxBreakdownItemResponse
{
    public string Name { get; init; } = string.Empty;
    public TaxItemType Type { get; init; }
    public decimal Amount { get; init; }
}

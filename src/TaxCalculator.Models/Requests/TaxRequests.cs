using TaxCalculator.Models.Enums;

namespace TaxCalculator.Models.Requests;

public sealed class ConfigureTaxRuleRequest
{
    public string CountryCode { get; init; } = string.Empty;
    public List<TaxItemRequest> TaxItems { get; init; } = new();
}

public sealed class TaxItemRequest
{
    public string Name { get; init; } = string.Empty;
    public TaxItemType Type { get; init; }

    // Fixed
    public decimal? Amount { get; init; }

    // FlatRate — percentage value, e.g. 20 means 20%
    public decimal? Rate { get; init; }

    // Progressive
    public List<ProgressiveBracketRequest>? Brackets { get; init; }
}

public sealed class ProgressiveBracketRequest
{
    /// <summary>Upper bound of this bracket. Null = open-ended (no upper limit).</summary>
    public decimal? Threshold { get; init; }

    /// <summary>Rate as a percentage, e.g. 20 for 20%.</summary>
    public decimal Rate { get; init; }
}

public sealed class CalculateTaxRequest
{
    public string CountryCode { get; init; } = string.Empty;
    public decimal GrossSalary { get; init; }
}

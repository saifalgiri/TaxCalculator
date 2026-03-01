namespace TaxCalculator.Database.Entities;

/// <summary>
/// Represents a country. CountryTaxConfigurationEntity references this via CountryId FK.
/// </summary>
public class CountryEntity
{
    public int Id { get; set; }

    /// <summary>ISO country code, always stored upper-case (e.g. DE, US, ESP).</summary>
    public string Code { get; set; } = string.Empty;

    // Navigation
    public CountryTaxConfigurationEntity? TaxConfiguration { get; set; }
}

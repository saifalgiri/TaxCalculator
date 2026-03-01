namespace TaxCalculator.Database.Entities;

/// <summary>
/// Tax configuration for a country, linked via CountryId FK to CountryEntity.
/// One configuration per country (enforced by the unique index on CountryId).
/// </summary>
public class CountryTaxConfigurationEntity
{
    public int Id { get; set; }

    /// <summary>FK to CountryEntity. All lookups use this ID — not the country code string.</summary>
    public int CountryId { get; set; }

    // Navigation
    public CountryEntity Country { get; set; } = null!;
    public List<TaxItemEntity> TaxItems { get; set; } = new();
}

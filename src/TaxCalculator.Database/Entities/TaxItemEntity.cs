namespace TaxCalculator.Database.Entities;

public class TaxItemEntity
{
    public int Id { get; set; }
    public int CountryTaxConfigurationId { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Discriminator stored as string (e.g. "Fixed", "FlatRate", "Progressive").</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Populated for Fixed tax items.</summary>
    public decimal? Amount { get; set; }

    /// <summary>Populated for FlatRate tax items. Stored as a fraction (e.g. 0.20 = 20%).</summary>
    public decimal? Rate { get; set; }

    // Navigation
    public CountryTaxConfigurationEntity CountryTaxConfiguration { get; set; } = null!;
    public List<ProgressiveBracketEntity> Brackets { get; set; } = new();
}

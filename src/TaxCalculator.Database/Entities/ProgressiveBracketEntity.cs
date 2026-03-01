namespace TaxCalculator.Database.Entities;

public class ProgressiveBracketEntity
{
    public int Id { get; set; }
    public int TaxItemId { get; set; }

    /// <summary>Upper bound of this bracket. Null = open-ended (no upper limit).</summary>
    public decimal? Threshold { get; set; }

    /// <summary>Rate stored as a fraction (e.g. 0.20 = 20%).</summary>
    public decimal Rate { get; set; }

    // Navigation
    public TaxItemEntity TaxItem { get; set; } = null!;
}

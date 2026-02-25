namespace HeimdallWeb.Infrastructure.Data.Views.ViewModels;

/// <summary>
/// VIEW model for vw_admin_top_categories.
/// Top vulnerability categories across all findings.
/// </summary>
public class AdminTopCategories
{
    public string Category { get; set; } = string.Empty;
    public int TotalFindingsInCategory { get; set; }
    public int CriticalInCategory { get; set; }
    public int HighInCategory { get; set; }
    public int MediumInCategory { get; set; }
    public int LowInCategory { get; set; }
    public decimal PercentageOfTotal { get; set; }
}

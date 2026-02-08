namespace HeimdallWeb.Domain.Entities.Views;

/// <summary>
/// Read-only entity mapped to vw_user_category_breakdown SQL VIEW.
/// Shows findings distribution by category per user.
/// </summary>
public class UserCategoryBreakdown
{
    public int UserId { get; set; }
    public string Category { get; set; } = string.Empty;
    public int CategoryCount { get; set; }
    public int CriticalInCategory { get; set; }
    public int HighInCategory { get; set; }
    public int MediumInCategory { get; set; }
    public int LowInCategory { get; set; }
    public int InformationalInCategory { get; set; }
}

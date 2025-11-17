namespace HeimdallWeb.Models;

/// <summary>
/// Distribuição de vulnerabilidades por categoria para usuário.
/// Mapeado para vw_user_category_breakdown.
/// </summary>
public class UserCategoryBreakdown
{
    public int user_id { get; set; }
    public string? main_category { get; set; }
    public int category_count { get; set; }
    public int critical_in_category { get; set; }
    public int high_in_category { get; set; }
    public int medium_in_category { get; set; }
    public int low_in_category { get; set; }
}

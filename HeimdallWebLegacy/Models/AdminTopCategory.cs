namespace HeimdallWeb.Models;

/// <summary>
/// Categorias mais frequentes de vulnerabilidades.
/// Mapeado para vw_admin_top_categories.
/// </summary>
public class AdminTopCategory
{
    public string? main_category { get; set; }
    public int category_occurrences { get; set; }
    public int total_findings_in_category { get; set; }
    public int critical_in_category { get; set; }
    public int high_in_category { get; set; }
    public decimal percentage_of_total { get; set; }
}

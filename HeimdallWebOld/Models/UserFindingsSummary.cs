namespace HeimdallWeb.Models;

/// <summary>
/// Resumo de findings por severidade para um usuário.
/// Mapeado para vw_user_findings_summary.
/// </summary>
public class UserFindingsSummary
{
    public int user_id { get; set; }
    public int total_findings { get; set; }
    public int total_critical { get; set; }
    public int total_high { get; set; }
    public int total_medium { get; set; }
    public int total_low { get; set; }
    public decimal percent_critical { get; set; }
    public decimal percent_high { get; set; }
    public decimal percent_medium { get; set; }
    public decimal percent_low { get; set; }
}

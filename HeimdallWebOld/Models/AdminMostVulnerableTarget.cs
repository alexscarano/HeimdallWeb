namespace HeimdallWeb.Models;

/// <summary>
/// Targets com mais vulnerabilidades detectadas.
/// Mapeado para vw_admin_most_vulnerable_targets.
/// </summary>
public class AdminMostVulnerableTarget
{
    public string? target { get; set; }
    public int scan_count { get; set; }
    public int total_findings { get; set; }
    public int total_critical { get; set; }
    public int total_high { get; set; }
    public string? highest_risk_level { get; set; }
    public DateTime? last_scan_date { get; set; }
}

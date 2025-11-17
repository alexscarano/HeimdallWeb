namespace HeimdallWeb.Models;

/// <summary>
/// Resumo de scans para um usuário específico.
/// Mapeado para vw_user_scan_summary.
/// </summary>
public class UserScanSummary
{
    public int user_id { get; set; }
    public int total_scans { get; set; }
    public int completed_scans { get; set; }
    public int failed_scans { get; set; }
    public int unique_targets { get; set; }
    public decimal? avg_scan_duration_seconds { get; set; }
    public DateTime? last_scan_date { get; set; }
    public int scans_last_7_days { get; set; }
    public int scans_last_30_days { get; set; }
}

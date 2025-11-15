namespace HeimdallWeb.ViewModels;

/// <summary>
/// ViewModel para métricas de usuário comum (não admin).
/// A ser implementado futuramente para dashboard do usuário.
/// </summary>
public class UserMetricsViewModel
{
    public int TotalScans { get; set; }
    public int ScansThisMonth { get; set; }
    public double AvgScanTime { get; set; }
}

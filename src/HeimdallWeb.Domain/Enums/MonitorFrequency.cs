namespace HeimdallWeb.Domain.Enums;

/// <summary>
/// Defines how frequently a monitored target is scanned.
/// </summary>
public enum MonitorFrequency
{
    /// <summary>Scan runs once per day.</summary>
    Daily = 1,

    /// <summary>Scan runs once per week.</summary>
    Weekly = 7
}

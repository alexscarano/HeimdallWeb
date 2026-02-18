namespace HeimdallWeb.Application.Services.Scanners;

/// <summary>
/// Metadata describing a scanner's capabilities and default timeout.
/// Used by ScannerManager to orchestrate parallel execution.
/// </summary>
public sealed record ScannerMetadata(
    /// <summary>Unique identifier for the scanner, e.g. "SSL", "Headers".</summary>
    string Key,

    /// <summary>Human-readable display name.</summary>
    string DisplayName,

    /// <summary>Scanner category used to look up RiskWeight.</summary>
    string Category,

    /// <summary>Default individual timeout for this scanner.</summary>
    TimeSpan DefaultTimeout
);

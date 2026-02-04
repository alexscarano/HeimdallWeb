using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Exceptions;

namespace HeimdallWeb.Domain.Entities;

/// <summary>
/// AuditLog entity representing a system log entry for auditing and debugging.
/// </summary>
public class AuditLog
{
    public int LogId { get; private set; }
    public DateTime Timestamp { get; private set; }
    public LogEventCode Code { get; private set; }
    public string Level { get; private set; } = "Info";
    public string? Source { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public string? Details { get; private set; }
    public int? UserId { get; private set; }
    public int? HistoryId { get; private set; }
    public string? RemoteIp { get; private set; }

    // Navigation properties (parents)
    public User? User { get; private set; }
    public ScanHistory? History { get; private set; }

    // Parameterless constructor for EF Core
    private AuditLog() { }

    /// <summary>
    /// Creates a new AuditLog instance.
    /// </summary>
    public AuditLog(
        LogEventCode code,
        string level,
        string message,
        string? source = null,
        string? details = null,
        int? userId = null,
        int? historyId = null,
        string? remoteIp = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ValidationException("Log message cannot be empty.");

        if (message.Length > 500)
            throw new ValidationException("Log message cannot exceed 500 characters.");

        if (level.Length > 10)
            throw new ValidationException("Log level cannot exceed 10 characters.");

        if (source?.Length > 100)
            throw new ValidationException("Log source cannot exceed 100 characters.");

        Code = code;
        Level = level;
        Message = message;
        Source = source;
        Details = details;
        UserId = userId;
        HistoryId = historyId;
        RemoteIp = remoteIp;
        Timestamp = DateTime.UtcNow;
    }
}

namespace HeimdallWeb.Application.Commands.Scan.DeleteScanHistory;

/// <summary>
/// Command to delete a scan history record.
/// Users can only delete their own scan history for security.
/// </summary>
public record DeleteScanHistoryCommand(
    Guid HistoryId,
    Guid RequestingUserId
);

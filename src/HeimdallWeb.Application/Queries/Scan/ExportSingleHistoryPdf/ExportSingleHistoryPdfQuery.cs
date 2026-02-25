namespace HeimdallWeb.Application.Queries.Scan.ExportSingleHistoryPdf;

/// <summary>
/// Query to export a single scan history to a PDF report.
/// Generates a detailed report with findings and technologies.
/// Validates ownership before exporting.
/// </summary>
/// <param name="HistoryId">The scan history ID to export</param>
/// <param name="RequestingUserId">The user requesting the export (for ownership verification)</param>
/// <param name="Username">The username to display in the PDF footer</param>
public record ExportSingleHistoryPdfQuery(
    Guid HistoryId,
    Guid RequestingUserId,
    string Username
);

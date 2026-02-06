namespace HeimdallWeb.Application.Queries.Scan.ExportHistoryPdf;

/// <summary>
/// Query to export all user's scan histories to a PDF report.
/// Generates a comprehensive report with all scans, findings, and technologies.
/// </summary>
/// <param name="UserId">The user ID to export scan histories for</param>
/// <param name="Username">The username to display in the PDF footer</param>
public record ExportHistoryPdfQuery(
    int UserId,
    string Username
);

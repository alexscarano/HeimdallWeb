using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Scan;
using HeimdallWeb.Application.Interfaces;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Queries.Scan.ExportSingleHistoryPdf;

/// <summary>
/// Handler for ExportSingleHistoryPdfQuery.
/// Exports a single scan history to a detailed PDF report.
/// Validates ownership before exporting (users can only export their own scans, admins can export any).
///
/// Source: HeimdallWebOld/Controllers/HistoryController.cs lines 159-188 (ExportSinglePdf method)
/// </summary>
public class ExportSingleHistoryPdfQueryHandler : IQueryHandler<ExportSingleHistoryPdfQuery, PdfExportResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPdfService _pdfService;

    public ExportSingleHistoryPdfQueryHandler(IUnitOfWork unitOfWork, IPdfService pdfService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
    }

    public async Task<PdfExportResponse> Handle(ExportSingleHistoryPdfQuery query, CancellationToken cancellationToken = default)
    {
        // Get scan history with all includes by PublicId
        var scanHistory = await _unitOfWork.ScanHistories.GetByPublicIdWithIncludesAsync(query.HistoryId, cancellationToken);

        if (scanHistory == null)
            throw new NotFoundException("Scan history", query.HistoryId);

        // Verify ownership (users can only export their own scans, admins can export any)
        var user = await _unitOfWork.Users.GetByPublicIdAsync(query.RequestingUserId, cancellationToken);

        if (user == null)
            throw new NotFoundException("User", query.RequestingUserId);

        if (user.UserType != UserType.Admin && scanHistory.UserId != user.UserId)
            throw new NotFoundException("Scan history", query.HistoryId); // Security: 404 instead of 403

        // Generate PDF for single scan
        var pdfBytes = _pdfService.GenerateSingleHistoryPdf(scanHistory, query.Username);

        // Generate filename with target and timestamp
        var sanitizedTarget = SanitizeFileName(scanHistory.Target.Value);
        var fileName = $"Scan_{sanitizedTarget}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";

        return new PdfExportResponse(
            PdfData: pdfBytes,
            FileName: fileName,
            ContentType: "application/pdf",
            FileSize: pdfBytes.Length
        );
    }

    /// <summary>
    /// Sanitizes a string to be used as a file name by removing invalid characters.
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

        // Limit length to 50 characters
        if (sanitized.Length > 50)
            sanitized = sanitized[..50];

        return sanitized;
    }
}

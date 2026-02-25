using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Scan;
using HeimdallWeb.Application.Interfaces;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Queries.Scan.ExportHistoryPdf;

/// <summary>
/// Handler for ExportHistoryPdfQuery.
/// Exports all user's scan histories to a PDF report.
/// Includes findings and technologies for each scan.
///
/// Source: HeimdallWebOld/Controllers/HistoryController.cs lines 127-156 (ExportPdf method)
/// </summary>
public class ExportHistoryPdfQueryHandler : IQueryHandler<ExportHistoryPdfQuery, PdfExportResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPdfService _pdfService;

    public ExportHistoryPdfQueryHandler(IUnitOfWork unitOfWork, IPdfService pdfService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
    }

    public async Task<PdfExportResponse> Handle(ExportHistoryPdfQuery query, CancellationToken cancellationToken = default)
    {
        // Validate user exists and resolve to internal ID
        var user = await _unitOfWork.Users.GetByPublicIdAsync(query.UserId, cancellationToken);
        if (user == null)
            throw new NotFoundException("User", query.UserId);

        var userInternalId = user.UserId; // Use internal ID for FK query

        // Get all scan histories with includes
        var histories = await _unitOfWork.ScanHistories.GetAllByUserIdWithIncludesAsync(userInternalId, cancellationToken);

        var historiesList = histories.ToList();

        if (!historiesList.Any())
        {
            throw new NotFoundException("No scan histories found for this user");
        }

        // Generate PDF
        var pdfBytes = _pdfService.GenerateHistoryPdf(historiesList, query.Username);

        // Generate filename with timestamp
        var fileName = $"Historico_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";

        return new PdfExportResponse(
            PdfData: pdfBytes,
            FileName: fileName,
            ContentType: "application/pdf",
            FileSize: pdfBytes.Length
        );
    }
}

namespace HeimdallWeb.Application.DTOs.Scan;

/// <summary>
/// Response DTO for PDF export operations.
/// Contains the PDF file data and metadata.
/// </summary>
public record PdfExportResponse(
    byte[] PdfData,
    string FileName,
    string ContentType,
    long FileSize
);

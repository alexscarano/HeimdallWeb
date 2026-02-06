using HeimdallWeb.Domain.Entities;

namespace HeimdallWeb.Application.Interfaces;

/// <summary>
/// Service interface for PDF generation operations.
/// Uses QuestPDF to generate professional security scan reports.
/// </summary>
public interface IPdfService
{
    /// <summary>
    /// Generates a PDF report for multiple scan histories.
    /// </summary>
    /// <param name="histories">List of scan histories to include</param>
    /// <param name="userName">Username to display in footer</param>
    /// <returns>PDF file as byte array</returns>
    byte[] GenerateHistoryPdf(IEnumerable<ScanHistory> histories, string userName);

    /// <summary>
    /// Generates a detailed PDF report for a single scan history.
    /// </summary>
    /// <param name="history">Scan history with findings and technologies</param>
    /// <param name="userName">Username to display in footer</param>
    /// <returns>PDF file as byte array</returns>
    byte[] GenerateSingleHistoryPdf(ScanHistory history, string userName);
}

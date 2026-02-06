using HeimdallWeb.Application.Interfaces;
using HeimdallWeb.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HeimdallWeb.Application.Services;

/// <summary>
/// Service for generating PDF reports using QuestPDF.
/// Creates professional security scan reports with findings, technologies, and AI summaries.
///
/// TODO: This is a simplified version. Full implementation needs to be migrated from
/// HeimdallWebOld/Services/PdfService.cs with proper entity mapping.
///
/// Source: HeimdallWebOld/Services/PdfService.cs
/// </summary>
public class PdfService : IPdfService
{
    public PdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Generates a PDF report for multiple scan histories.
    /// </summary>
    public byte[] GenerateHistoryPdf(IEnumerable<ScanHistory> histories, string userName)
    {
        var historiesList = histories.ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.PageColor("#FFFFFF");
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(content => ComposeMultipleHistoriesContent(content, historiesList));
                page.Footer().Element(footer => ComposeFooter(footer, userName));
            });
        });

        return document.GeneratePdf();
    }

    /// <summary>
    /// Generates a detailed PDF report for a single scan history.
    /// </summary>
    public byte[] GenerateSingleHistoryPdf(ScanHistory history, string userName)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.PageColor("#FFFFFF");
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(c => ComposeSingleHeader(c, history));
                page.Content().Element(content => ComposeSingleContent(content, history));
                page.Footer().Element(footer => ComposeFooter(footer, userName));
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().AlignCenter().Text("Relatório de Histórico - Heimdall")
                .FontSize(20)
                .SemiBold()
                .FontColor("#1565C0");

            column.Item().PaddingTop(5).LineHorizontal(2).LineColor("#1565C0");

            column.Item().PaddingTop(10).Text($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                .FontSize(10)
                .FontColor("#616161");
        });
    }

    private void ComposeMultipleHistoriesContent(IContainer container, List<ScanHistory> histories)
    {
        container.PaddingVertical(20).Column(column =>
        {
            column.Item().Text("Resumo Executivo")
                .FontSize(14)
                .SemiBold()
                .FontColor("#1565C0");

            column.Item().PaddingTop(5).LineHorizontal(1).LineColor("#9E9E9E");

            column.Item().PaddingTop(10).Text($"Total de registros: {histories.Count}")
                .FontSize(11);

            var completedCount = histories.Count(h => h.HasCompleted);
            column.Item().PaddingTop(5).Text($"Scans completados: {completedCount}")
                .FontSize(11);

            // Table of histories
            column.Item().PaddingTop(20).Text("Detalhamento dos Registros")
                .FontSize(14)
                .SemiBold()
                .FontColor("#1565C0");

            foreach (var history in histories)
            {
                column.Item().PaddingTop(15).Background("#F5F5F5").Padding(10).Column(col =>
                {
                    col.Item().Text($"Alvo: {history.Target.Value}")
                        .FontSize(11)
                        .SemiBold();

                    col.Item().PaddingTop(3).Text($"Data: {history.CreatedDate:dd/MM/yyyy HH:mm}")
                        .FontSize(9);

                    if (history.Duration != null)
                    {
                        TimeSpan duration = history.Duration; // Implicit conversion
                        col.Item().Text($"Duração: {duration:mm\\:ss}")
                            .FontSize(9);
                    }

                    if (!string.IsNullOrWhiteSpace(history.Summary))
                    {
                        col.Item().PaddingTop(5).Text(TruncateText(history.Summary, 200))
                            .FontSize(8)
                            .FontColor("#757575");
                    }
                });
            }
        });
    }

    private void ComposeSingleHeader(IContainer container, ScanHistory history)
    {
        container.Column(column =>
        {
            column.Item().AlignCenter().Text("Relatório Detalhado de Scan de Segurança")
                .FontSize(18)
                .SemiBold()
                .FontColor("#1565C0");

            column.Item().PaddingTop(3).AlignCenter().Text("Heimdall Web Security Scanner")
                .FontSize(11)
                .FontColor("#757575");

            column.Item().PaddingTop(8).LineHorizontal(2).LineColor("#1565C0");

            column.Item().PaddingTop(10).Background("#F5F5F5").Padding(10).Column(col =>
            {
                col.Item().Text("Alvo Escaneado:")
                    .FontSize(9)
                    .FontColor("#616161");

                col.Item().PaddingTop(2).Text(history.Target.Value)
                    .FontSize(13)
                    .SemiBold()
                    .FontColor("#212121");

                col.Item().PaddingTop(8).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Data do Scan:")
                            .FontSize(8)
                            .FontColor("#616161");
                        c.Item().Text($"{history.CreatedDate:dd/MM/yyyy HH:mm:ss}")
                            .FontSize(9)
                            .SemiBold()
                            .FontColor("#424242");
                    });

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Status:")
                            .FontSize(8)
                            .FontColor("#616161");
                        c.Item().Text(history.HasCompleted ? "Completo" : "Em andamento")
                            .FontSize(9)
                            .SemiBold()
                            .FontColor(history.HasCompleted ? "#2E7D32" : "#EF6C00");
                    });
                });
            });
        });
    }

    private void ComposeSingleContent(IContainer container, ScanHistory history)
    {
        container.PaddingVertical(20).Column(column =>
        {
            column.Item().Text("Visão Geral do Scan")
                .FontSize(14)
                .SemiBold()
                .FontColor("#1565C0");

            column.Item().PaddingTop(5).LineHorizontal(1).LineColor("#9E9E9E");

            if (!string.IsNullOrWhiteSpace(history.Summary))
            {
                column.Item().PaddingTop(20).Text("Resumo de IA")
                    .FontSize(14)
                    .SemiBold()
                    .FontColor("#1565C0");

                column.Item().PaddingTop(10).Background("#F3E5F5").Padding(15).Text(history.Summary)
                    .FontSize(10)
                    .FontColor("#424242")
                    .LineHeight(1.6f);
            }

            // NOTE: Full implementation should include Findings and Technologies rendering
            // This requires navigation properties to be loaded (via GetByIdWithIncludesAsync)
            column.Item().PaddingTop(20).Text("Nota: Achados e tecnologias serão renderizados na versão completa")
                .FontSize(8)
                .Italic()
                .FontColor("#9E9E9E");
        });
    }

    private void ComposeFooter(IContainer container, string userName)
    {
        container.Column(column =>
        {
            column.Item().LineHorizontal(1).LineColor("#9E9E9E");

            column.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().AlignLeft().Text($"Gerado por: {userName}")
                    .FontSize(8)
                    .FontColor("#616161");

                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Página ")
                        .FontSize(8)
                        .FontColor("#616161");

                    text.CurrentPageNumber()
                        .FontSize(8)
                        .FontColor("#616161");

                    text.Span(" de ")
                        .FontSize(8)
                        .FontColor("#616161");

                    text.TotalPages()
                        .FontSize(8)
                        .FontColor("#616161");
                });
            });
        });
    }

    private string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return text.Length <= maxLength ? text : text[..maxLength] + "...";
    }
}

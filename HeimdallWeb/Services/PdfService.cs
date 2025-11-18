using HeimdallWeb.Interfaces;
using HeimdallWeb.Models;
using HeimdallWeb.Models.Map;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HeimdallWeb.Services
{
    public class PdfService : IPdfService
    {
        public byte[] GenerateHistoryPdf(PaginatedResult<HistoryModel> histories, string userName)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.PageColor("#FFFFFF");
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    // Cabeçalho
                    page.Header().Element(ComposeHeader);

                    // Conteúdo principal
                    page.Content().Element(content => ComposeContent(content, histories));

                    // Rodapé
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

        private void ComposeContent(IContainer container, PaginatedResult<HistoryModel> histories)
        {
            container.PaddingVertical(20).Column(column =>
            {
                // Resumo executivo
                column.Item().Element(content => ComposeSummary(content, histories));

                // Espaçamento
                column.Item().PaddingTop(20);

                // Tabela de histórico
                column.Item().Element(content => ComposeTable(content, histories));
            });
        }

        private void ComposeSummary(IContainer container, PaginatedResult<HistoryModel> histories)
        {
            container.Column(column =>
            {
                column.Item().Text("Resumo Executivo")
                    .FontSize(14)
                    .SemiBold()
                    .FontColor("#1565C0");

                column.Item().PaddingTop(5).LineHorizontal(1).LineColor("#9E9E9E");

                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text($"Total de registros: {histories.TotalCount}")
                            .FontSize(11);
                        col.Item().PaddingTop(5).Text($"Página: {histories.Page} de {histories.TotalPages}")
                            .FontSize(11);
                    });

                    row.RelativeItem().Column(col =>
                    {
                        var completedCount = histories.Items.Count(h => h.has_completed);
                        col.Item().Text($"Scans completados: {completedCount}")
                            .FontSize(11);
                        col.Item().PaddingTop(5).Text($"Scans em andamento: {histories.Items.Count - completedCount}")
                            .FontSize(11);
                    });
                });

                // Estatísticas de achados
                if (histories.Items.Any(h => h.Findings != null && h.Findings.Any()))
                {
                    column.Item().PaddingTop(15).Text("Distribuição de Severidade nos Achados")
                        .FontSize(12)
                        .SemiBold();

                    var allFindings = histories.Items
                        .Where(h => h.Findings != null)
                        .SelectMany(h => h.Findings!)
                        .ToList();

                    if (allFindings.Any())
                    {
                        var severityGroups = allFindings
                            .GroupBy(f => f.severity)
                            .Select(g => new { Severity = g.Key, Count = g.Count() })
                            .OrderByDescending(x => x.Severity)
                            .ToList();

                        column.Item().PaddingTop(5).Row(row =>
                        {
                            foreach (var group in severityGroups)
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text($"{group.Severity}: {group.Count}")
                                        .FontSize(10)
                                        .FontColor(GetSeverityColor(group.Severity));
                                });
                            }
                        });
                    }
                }
            });
        }

        private void ComposeTable(IContainer container, PaginatedResult<HistoryModel> histories)
        {
            container.Column(column =>
            {
                column.Item().Text("Detalhamento dos Registros")
                    .FontSize(14)
                    .SemiBold()
                    .FontColor("#1565C0");

                column.Item().PaddingTop(5).LineHorizontal(1).LineColor("#9E9E9E");

                column.Item().PaddingTop(10).Table(table =>
                {
                    // Definir colunas
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);  // ID
                        columns.RelativeColumn(3);   // Site
                        columns.RelativeColumn(2);   // Data
                        columns.ConstantColumn(70);  // Duração
                        columns.ConstantColumn(60);  // Status
                        columns.ConstantColumn(60);  // Achados
                    });

                    // Cabeçalho da tabela
                    table.Header(header =>
                    {
                        header.Cell().Background("#1565C0").Padding(5).Text("ID")
                            .FontColor("#FFFFFF").FontSize(9).SemiBold();
                        header.Cell().Background("#1565C0").Padding(5).Text("Site")
                            .FontColor("#FFFFFF").FontSize(9).SemiBold();
                        header.Cell().Background("#1565C0").Padding(5).Text("Data")
                            .FontColor("#FFFFFF").FontSize(9).SemiBold();
                        header.Cell().Background("#1565C0").Padding(5).Text("Duração")
                            .FontColor("#FFFFFF").FontSize(9).SemiBold();
                        header.Cell().Background("#1565C0").Padding(5).Text("Status")
                            .FontColor("#FFFFFF").FontSize(9).SemiBold();
                        header.Cell().Background("#1565C0").Padding(5).Text("Achados")
                            .FontColor("#FFFFFF").FontSize(9).SemiBold();
                    });

                    // Linhas da tabela
                    foreach (var history in histories.Items)
                    {
                        var backgroundColor = histories.Items.IndexOf(history) % 2 == 0 
                            ? "#EEEEEE"
                            : "#FFFFFF";

                        table.Cell().Background(backgroundColor).Padding(5).Text(history.history_id.ToString())
                            .FontSize(8);
                        
                        table.Cell().Background(backgroundColor).Padding(5).Text(TruncateText(history.target, 40))
                            .FontSize(8);
                        
                        table.Cell().Background(backgroundColor).Padding(5).Text(history.created_date.ToString("dd/MM/yyyy HH:mm"))
                            .FontSize(8);
                        
                        table.Cell().Background(backgroundColor).Padding(5).Text(
                            history.duration.HasValue 
                                ? history.duration.Value.ToString(@"mm\:ss") 
                                : "N/A")
                            .FontSize(8);
                        
                        table.Cell().Background(backgroundColor).Padding(5).Text(
                            history.has_completed ? "Completo" : "Em andamento")
                            .FontSize(8)
                            .FontColor(history.has_completed ? "#2E7D32" : "#EF6C00");
                        
                        table.Cell().Background(backgroundColor).Padding(5).Text(
                            history.Findings?.Count.ToString() ?? "0")
                            .FontSize(8);
                    }
                });

                // Seção de resumo IA (se houver)
                if (histories.Items.Any(h => !string.IsNullOrWhiteSpace(h.summary)))
                {
                    column.Item().PaddingTop(20).Text("Resumos de IA")
                        .FontSize(12)
                        .SemiBold()
                        .FontColor("#1565C0");

                    foreach (var history in histories.Items.Where(h => !string.IsNullOrWhiteSpace(h.summary)))
                    {
                        column.Item().PaddingTop(10).Column(col =>
                        {
                            col.Item().Text($"ID {history.history_id} - {history.target}")
                                .FontSize(10)
                                .SemiBold();
                            col.Item().PaddingTop(3).Text(TruncateText(history.summary, 500))
                                .FontSize(9)
                                .FontColor("#757575");
                        });
                    }
                }
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

                    row.RelativeItem().AlignRight().Text($"Página X")
                        .FontSize(8)
                        .FontColor("#616161");
                });
            });
        }

        private string GetSeverityColor(Enums.SeverityLevel severity)
        {
            return severity switch
            {
                Enums.SeverityLevel.Critical => "#c62828",
                Enums.SeverityLevel.High => "#ef6c00",
                Enums.SeverityLevel.Medium => "#f9a825",
                Enums.SeverityLevel.Low => "#1565c0",
                _ => "#757575"
            };
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.Length <= maxLength ? text : text[..maxLength] + "...";
        }
    }
}

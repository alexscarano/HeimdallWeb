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
                                    col.Item().Text($"{GetSeverityLabel(group.Severity)}: {group.Count}")
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

        private string GetSeverityLabel(Enums.SeverityLevel severity)
        {
            return severity switch
            {
                Enums.SeverityLevel.Critical => "Crítico",
                Enums.SeverityLevel.High => "Alto",
                Enums.SeverityLevel.Medium => "Médio",
                Enums.SeverityLevel.Low => "Baixo",
                _ => "Desconhecido"
            };
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.Length <= maxLength ? text : text[..maxLength] + "...";
        }

        public byte[] GenerateSingleHistoryPdf(HistoryModel history, string userName)
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
                    page.Header().Element(c => ComposeSingleHeader(c, history));

                    // Conteúdo principal
                    page.Content().Element(content => ComposeSingleContent(content, history));

                    // Rodapé
                    page.Footer().Element(footer => ComposeFooter(footer, userName));
                });
            });

            return document.GeneratePdf();
        }

        private void ComposeSingleHeader(IContainer container, HistoryModel history)
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
                    
                    col.Item().PaddingTop(2).Text(history.target)
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
                            c.Item().Text($"{history.created_date:dd/MM/yyyy HH:mm:ss}")
                                .FontSize(9)
                                .SemiBold()
                                .FontColor("#424242");
                        });

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Duração:")
                                .FontSize(8)
                                .FontColor("#616161");
                            c.Item().Text(history.duration.HasValue ? history.duration.Value.ToString(@"mm\:ss") + " min" : "N/A")
                                .FontSize(9)
                                .SemiBold()
                                .FontColor("#424242");
                        });

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Status:")
                                .FontSize(8)
                                .FontColor("#616161");
                            c.Item().Text(history.has_completed ? "Completo" : "Em andamento")
                                .FontSize(9)
                                .SemiBold()
                                .FontColor(history.has_completed ? "#2E7D32" : "#EF6C00");
                        });
                    });
                });
            });
        }

        private void ComposeSingleContent(IContainer container, HistoryModel history)
        {
            container.PaddingVertical(20).Column(column =>
            {
                // Status do Scan
                column.Item().Element(c => ComposeScanStatus(c, history));

                // Resumo IA
                if (!string.IsNullOrWhiteSpace(history.summary))
                {
                    column.Item().PaddingTop(20).Element(c => ComposeIASummary(c, history));
                }

                // Achados (Findings)
                if (history.Findings != null && history.Findings.Any())
                {
                    column.Item().PaddingTop(20).Element(c => ComposeFindings(c, history));
                }

                // Tecnologias
                if (history.Technologies != null && history.Technologies.Any())
                {
                    column.Item().PaddingTop(20).Element(c => ComposeTechnologies(c, history));
                }
            });
        }

        private void ComposeScanStatus(IContainer container, HistoryModel history)
        {
            container.Column(column =>
            {
                column.Item().Text("Visão Geral do Scan")
                    .FontSize(14)
                    .SemiBold()
                    .FontColor("#1565C0");

                column.Item().PaddingTop(5).LineHorizontal(1).LineColor("#9E9E9E");

                column.Item().PaddingTop(10).Row(row =>
                {
                    // Card de Achados
                    row.RelativeItem().Background("#FFEBEE").Padding(10).Column(col =>
                    {
                        var findingsCount = history.Findings?.Count ?? 0;
                        var criticalCount = history.Findings?.Count(f => f.severity == Enums.SeverityLevel.Critical) ?? 0;
                        var highCount = history.Findings?.Count(f => f.severity == Enums.SeverityLevel.High) ?? 0;
                        
                        col.Item().Text("Achados de Segurança")
                            .FontSize(10)
                            .SemiBold()
                            .FontColor("#616161");
                        
                        col.Item().PaddingTop(5).Text(findingsCount.ToString())
                            .FontSize(24)
                            .Bold()
                            .FontColor("#C62828");
                        
                        if (criticalCount > 0 || highCount > 0)
                        {
                            col.Item().PaddingTop(3).Text($"{criticalCount} Críticos | {highCount} Altos")
                                .FontSize(8)
                                .FontColor("#D32F2F");
                        }
                        else
                        {
                            col.Item().PaddingTop(3).Text("Sem riscos críticos")
                                .FontSize(8)
                                .FontColor("#388E3C");
                        }
                    });

                    row.ConstantItem(10); // Espaçamento

                    // Card de Tecnologias
                    row.RelativeItem().Background("#E3F2FD").Padding(10).Column(col =>
                    {
                        var techCount = history.Technologies?.Count ?? 0;
                        
                        col.Item().Text("Tecnologias Detectadas")
                            .FontSize(10)
                            .SemiBold()
                            .FontColor("#616161");
                        
                        col.Item().PaddingTop(5).Text(techCount.ToString())
                            .FontSize(24)
                            .Bold()
                            .FontColor("#1565C0");
                        
                        col.Item().PaddingTop(3).Text(techCount > 0 ? "Stack tecnológico identificado" : "Nenhuma tecnologia detectada")
                            .FontSize(8)
                            .FontColor("#0277BD");
                    });

                    row.ConstantItem(10); // Espaçamento

                    // Card de Status
                    row.RelativeItem().Background(history.has_completed ? "#E8F5E9" : "#FFF3E0").Padding(10).Column(col =>
                    {
                        col.Item().Text("Status do Processo")
                            .FontSize(10)
                            .SemiBold()
                            .FontColor("#616161");
                        
                        col.Item().PaddingTop(5).Text(history.has_completed ? "OK" : "...")
                            .FontSize(24)
                            .Bold()
                            .FontColor(history.has_completed ? "#2E7D32" : "#F57C00");
                        
                        col.Item().PaddingTop(3).Text(history.has_completed ? "Scan finalizado" : "Processamento em andamento")
                            .FontSize(8)
                            .FontColor(history.has_completed ? "#388E3C" : "#EF6C00");
                    });
                });
            });
        }

        private void ComposeIASummary(IContainer container, HistoryModel history)
        {
            container.Column(column =>
            {
                column.Item().Text("Análise de Inteligência Artificial")
                    .FontSize(14)
                    .SemiBold()
                    .FontColor("#1565C0");

                column.Item().PaddingTop(5).LineHorizontal(1).LineColor("#9E9E9E");

                column.Item().PaddingTop(10).Background("#F3E5F5").Border(1).BorderColor("#9C27B0")
                    .Padding(15).Column(summaryCol =>
                {
                    summaryCol.Item().Row(iconRow =>
                    {
                        iconRow.AutoItem().Background("#9C27B0").Padding(6).Text("AI")
                            .FontSize(10)
                            .Bold()
                            .FontColor("#FFFFFF");
                        
                        iconRow.ConstantItem(10);
                        
                        iconRow.RelativeItem().Text("Resumo Automatizado de Segurança")
                            .FontSize(11)
                            .SemiBold()
                            .FontColor("#6A1B9A");
                    });

                    summaryCol.Item().PaddingTop(12).Text(history.summary)
                        .FontSize(10)
                        .FontColor("#424242")
                        .LineHeight(1.6f);

                    summaryCol.Item().PaddingTop(10).LineHorizontal(1).LineColor("#CE93D8");

                    summaryCol.Item().PaddingTop(8).Text("Esta análise foi gerada automaticamente com base nos achados e tecnologias detectadas.")
                        .FontSize(8)
                        .Italic()
                        .FontColor("#7B1FA2");
                });
            });
        }

        private void ComposeFindings(IContainer container, HistoryModel history)
        {
            container.Column(column =>
            {
                column.Item().Text("Achados de Segurança Detalhados")
                    .FontSize(14)
                    .SemiBold()
                    .FontColor("#1565C0");

                column.Item().PaddingTop(5).LineHorizontal(1).LineColor("#9E9E9E");

                // Estatísticas por severidade com cards coloridos
                var severityGroups = history.Findings!
                    .GroupBy(f => f.severity)
                    .Select(g => new { Severity = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Severity)
                    .ToList();

                column.Item().PaddingTop(10).Row(row =>
                {
                    foreach (var group in severityGroups)
                    {
                        var bgColor = group.Severity switch
                        {
                            Enums.SeverityLevel.Critical => "#FFCDD2",
                            Enums.SeverityLevel.High => "#FFE0B2",
                            Enums.SeverityLevel.Medium => "#FFF9C4",
                            Enums.SeverityLevel.Low => "#BBDEFB",
                            _ => "#F5F5F5"
                        };

                        row.RelativeItem().Background(bgColor).Padding(8).Column(col =>
                        {
                            col.Item().AlignCenter().Text(GetSeverityLabel(group.Severity))
                                .FontSize(9)
                                .SemiBold()
                                .FontColor(GetSeverityColor(group.Severity));
                            
                            col.Item().PaddingTop(2).AlignCenter().Text(group.Count.ToString())
                                .FontSize(16)
                                .Bold()
                                .FontColor(GetSeverityColor(group.Severity));
                        });
                    }
                });

                // Lista detalhada de cada achado
                column.Item().PaddingTop(20).Text("Detalhes dos Achados:")
                    .FontSize(12)
                    .SemiBold();

                var findingNumber = 1;
                foreach (var finding in history.Findings!)
                {
                    column.Item().PaddingTop(15).Column(findingCol =>
                    {
                        // Header do achado com número e severidade
                        findingCol.Item().Background("#F5F5F5").Padding(8).Row(headerRow =>
                        {
                            headerRow.RelativeItem().Text($"Achado #{findingNumber}")
                                .FontSize(11)
                                .SemiBold()
                                .FontColor("#212121");

                            headerRow.ConstantItem(10);

                            headerRow.ConstantItem(80).Background(GetSeverityColor(finding.severity))
                                .Padding(3).AlignCenter().Text(GetSeverityLabel(finding.severity))
                                .FontSize(8)
                                .Bold()
                                .FontColor("#FFFFFF");
                        });

                        // Conteúdo do achado
                        findingCol.Item().Border(1).BorderColor("#E0E0E0").Padding(10).Column(contentCol =>
                        {
                            // Tipo
                            contentCol.Item().Row(row =>
                            {
                                row.ConstantItem(100).Text("Tipo:")
                                    .FontSize(9)
                                    .SemiBold()
                                    .FontColor("#616161");
                                row.RelativeItem().Text(finding.type)
                                    .FontSize(9)
                                    .FontColor("#212121");
                            });

                            contentCol.Item().PaddingTop(8).Row(row =>
                            {
                                row.ConstantItem(100).Text("Descrição:")
                                    .FontSize(9)
                                    .SemiBold()
                                    .FontColor("#616161");
                                row.RelativeItem().Text(finding.description)
                                    .FontSize(9)
                                    .FontColor("#424242")
                                    .LineHeight(1.4f);
                            });

                            // Evidência (se houver)
                            if (!string.IsNullOrWhiteSpace(finding.evidence))
                            {
                                contentCol.Item().PaddingTop(8).Row(row =>
                                {
                                    row.ConstantItem(100).Text("Evidência:")
                                        .FontSize(9)
                                        .SemiBold()
                                        .FontColor("#616161");
                                    row.RelativeItem().Background("#FFFDE7").Padding(5).Text(finding.evidence)
                                        .FontSize(8)
                                        .FontFamily("Courier New")
                                        .FontColor("#F57F17");
                                });
                            }

                            // Recomendação
                            if (!string.IsNullOrWhiteSpace(finding.recommendation))
                            {
                                contentCol.Item().PaddingTop(8).Row(row =>
                                {
                                    row.ConstantItem(100).Text("Recomendação:")
                                        .FontSize(9)
                                        .SemiBold()
                                        .FontColor("#616161");
                                    row.RelativeItem().Background("#E8F5E9").Padding(5).Text(finding.recommendation)
                                        .FontSize(9)
                                        .FontColor("#2E7D32")
                                        .LineHeight(1.4f);
                                });
                            }

                            // Data de criação
                            contentCol.Item().PaddingTop(6).Row(row =>
                            {
                                row.ConstantItem(100).Text("Detectado em:")
                                    .FontSize(8)
                                    .FontColor("#9E9E9E");
                                row.RelativeItem().Text(finding.created_at.ToString("dd/MM/yyyy HH:mm:ss"))
                                    .FontSize(8)
                                    .FontColor("#757575");
                            });
                        });
                    });

                    findingNumber++;
                }

                // Resumo final
                column.Item().PaddingTop(15).Background("#E3F2FD").Padding(10).Row(row =>
                {
                    row.RelativeItem().Text($"Total de {history.Findings.Count} achado(s) de segurança identificado(s) neste scan.")
                        .FontSize(9)
                        .Italic()
                        .FontColor("#1565C0");
                });
            });
        }

        private void ComposeTechnologies(IContainer container, HistoryModel history)
        {
            container.Column(column =>
            {
                column.Item().Text("Stack Tecnológico Detectado")
                    .FontSize(14)
                    .SemiBold()
                    .FontColor("#1565C0");

                column.Item().PaddingTop(5).LineHorizontal(1).LineColor("#9E9E9E");

                // Estatísticas de categorias
                if (history.Technologies!.Any(t => !string.IsNullOrWhiteSpace(t.technology_category)))
                {
                    var categoryGroups = history.Technologies
                        .Where(t => !string.IsNullOrWhiteSpace(t.technology_category))
                        .GroupBy(t => t.technology_category)
                        .Select(g => new { Category = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .ToList();

                    if (categoryGroups.Any())
                    {
                        column.Item().PaddingTop(10).Text("Distribuição por Categoria:")
                            .FontSize(11)
                            .SemiBold();

                        column.Item().PaddingTop(5).Row(row =>
                        {
                            foreach (var group in categoryGroups.Take(4))
                            {
                                row.RelativeItem().Background("#E3F2FD").Padding(6).Column(col =>
                                {
                                    col.Item().Text(group.Category ?? "N/A")
                                        .FontSize(8)
                                        .FontColor("#616161");
                                    col.Item().PaddingTop(2).Text(group.Count.ToString())
                                        .FontSize(14)
                                        .SemiBold()
                                        .FontColor("#1565C0");
                                });
                            }
                        });
                    }
                }

                // Lista detalhada de tecnologias
                column.Item().PaddingTop(20).Text("Tecnologias Identificadas:")
                    .FontSize(12)
                    .SemiBold();

                var techNumber = 1;
                foreach (var tech in history.Technologies!)
                {
                    column.Item().PaddingTop(12).Column(techCol =>
                    {
                        // Header da tecnologia
                        techCol.Item().Background("#FAFAFA").Padding(8).Row(headerRow =>
                        {
                            headerRow.ConstantItem(30).Text($"#{techNumber}")
                                .FontSize(10)
                                .SemiBold()
                                .FontColor("#9E9E9E");

                            headerRow.RelativeItem().Text(tech.technology_name ?? "Tecnologia Desconhecida")
                                .FontSize(11)
                                .SemiBold()
                                .FontColor("#1565C0");

                            if (!string.IsNullOrWhiteSpace(tech.version))
                            {
                                headerRow.ConstantItem(10);
                                headerRow.ConstantItem(60).Background("#4CAF50")
                                    .Padding(3).AlignCenter().Text($"v{tech.version}")
                                    .FontSize(7)
                                    .Bold()
                                    .FontColor("#FFFFFF");
                            }
                        });

                        // Conteúdo da tecnologia
                        techCol.Item().Border(1).BorderColor("#E0E0E0").Padding(10).Column(contentCol =>
                        {
                            // Categoria
                            if (!string.IsNullOrWhiteSpace(tech.technology_category))
                            {
                                contentCol.Item().Row(row =>
                                {
                                    row.ConstantItem(80).Text("Categoria:")
                                        .FontSize(9)
                                        .SemiBold()
                                        .FontColor("#616161");
                                    row.RelativeItem().Text(tech.technology_category)
                                        .FontSize(9)
                                        .FontColor("#424242");
                                });
                            }

                            // Descrição
                            if (!string.IsNullOrWhiteSpace(tech.technology_description))
                            {
                                contentCol.Item().PaddingTop(6).Row(row =>
                                {
                                    row.ConstantItem(80).Text("Descrição:")
                                        .FontSize(9)
                                        .SemiBold()
                                        .FontColor("#616161");
                                    row.RelativeItem().Text(tech.technology_description)
                                        .FontSize(9)
                                        .FontColor("#757575")
                                        .LineHeight(1.4f);
                                });
                            }
                            else
                            {
                                contentCol.Item().PaddingTop(6).Text("Sem descrição disponível")
                                    .FontSize(8)
                                    .Italic()
                                    .FontColor("#BDBDBD");
                            }

                            // Informações adicionais em linha
                            contentCol.Item().PaddingTop(8).Row(infoRow =>
                            {
                                if (!string.IsNullOrWhiteSpace(tech.version))
                                {
                                    infoRow.AutoItem().Background("#E8F5E9").Padding(4).Text($"Versão: {tech.version}")
                                        .FontSize(7)
                                        .FontColor("#2E7D32");
                                    infoRow.ConstantItem(5);
                                }

                                if (!string.IsNullOrWhiteSpace(tech.technology_category))
                                {
                                    infoRow.AutoItem().Background("#FFF3E0").Padding(4).Text($"{tech.technology_category}")
                                        .FontSize(7)
                                        .FontColor("#E65100");
                                }
                            });
                        });
                    });

                    techNumber++;
                }

                // Resumo final
                column.Item().PaddingTop(15).Background("#E3F2FD").Padding(10).Row(row =>
                {
                    row.RelativeItem().Text($"Total de {history.Technologies.Count} tecnologia(s) identificada(s) no alvo.")
                        .FontSize(9)
                        .Italic()
                        .FontColor("#1565C0");
                });
            });
        }
    }
}

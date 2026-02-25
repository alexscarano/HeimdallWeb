using System.Text.Json;
using HeimdallWeb.DTO;
using HeimdallWeb.Enums;
using HeimdallWeb.Helpers;
using HeimdallWeb.Interfaces;
using HeimdallWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace HeimdallWeb.Repository;

public class IASummaryRepository : IIASummaryRepository
{
    private readonly AppDbContext _db;
    private readonly ILogRepository _logRepository;

    public IASummaryRepository(AppDbContext db, ILogRepository logRepository)
    {
        _db = db;
        _logRepository = logRepository;
    }

    public async Task SaveIASummaryFromFindings(int historyId, string iaResponse)
    {
        try
        {
            // Parse da resposta da IA
            var wrapper = JsonSerializer.Deserialize<AIResponseDTO>(iaResponse);
            var findings = wrapper?.achados;

            if (findings == null || findings.Count == 0)
            {
                // Criar resumo vazio se não houver findings
                var emptySummary = new IASummaryModel
                {
                    history_id = historyId,
                    summary_text = "Nenhuma vulnerabilidade detectada",
                    main_category = "Geral",
                    overall_risk = "Baixo",
                    total_findings = 0,
                    findings_critical = 0,
                    findings_high = 0,
                    findings_medium = 0,
                    findings_low = 0,
                    ia_notes = wrapper?.resumo ?? "Scan completo sem achados críticos",
                    created_date = DateTime.Now
                };

                await _db.IASummary.AddAsync(emptySummary);
                // SaveChangesAsync será chamado no ScanService dentro da transação
                return;
            }

            // Contar findings por severidade
            int critical = findings.Count(f => f.risco?.Equals("Critico", StringComparison.OrdinalIgnoreCase) == true);
            int high = findings.Count(f => f.risco?.Equals("Alto", StringComparison.OrdinalIgnoreCase) == true);
            int medium = findings.Count(f => f.risco?.Equals("Medio", StringComparison.OrdinalIgnoreCase) == true);
            int low = findings.Count(f => f.risco?.Equals("Baixo", StringComparison.OrdinalIgnoreCase) == true);

            // Determinar risco geral (prioridade: Critico > Alto > Medio > Baixo)
            string overallRisk = critical > 0 ? "Critico" :
                                high > 0 ? "Alto" :
                                medium > 0 ? "Medio" : "Baixo";

            // Determinar categoria principal (mais frequente)
            var mainCategory = findings
                .GroupBy(f => f.categoria)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "Geral";

            // Criar texto resumido
            var summaryText = $"{findings.Count} vulnerabilidade(s) detectada(s): " +
                            $"{critical} crítica(s), {high} alta(s), {medium} média(s), {low} baixa(s). " +
                            $"Categoria principal: {mainCategory}.";

            var iaSummary = new IASummaryModel
            {
                history_id = historyId,
                summary_text = summaryText.Length > 1000 ? summaryText.Substring(0, 997) + "..." : summaryText,
                main_category = mainCategory,
                overall_risk = overallRisk,
                total_findings = findings.Count,
                findings_critical = critical,
                findings_high = high,
                findings_medium = medium,
                findings_low = low,
                ia_notes = wrapper?.resumo,
                created_date = DateTime.Now
            };

            await _db.IASummary.AddAsync(iaSummary);
            // SaveChangesAsync será chamado no ScanService dentro da transação
        }
        catch (Exception ex)
        {
            // Logging será tratado no ScanService
            throw;
        }
    }

    public async Task<List<IASummaryModel>> GetUserIASummaries(int userId, int limit = 10)
    {
        return await _db.IASummary
            .Include(s => s.History)
            .Where(s => s.History != null && s.History.user_id == userId)
            .OrderByDescending(s => s.created_date)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();
    }
}

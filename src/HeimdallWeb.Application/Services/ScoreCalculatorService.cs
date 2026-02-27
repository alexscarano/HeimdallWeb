using HeimdallWeb.Application.Interfaces;
using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace HeimdallWeb.Application.Services;

/// <summary>
/// Computes a security score (0–100) and grade (A–F) from a scan's findings.
///
/// Algorithm:
///   1. Load active RiskWeights from DB (10-min memory cache).
///   2. For each finding: deduction = severity_base_points × category_weight.
///   3. Score = max(0, 100 − total_deductions).
///   4. Grade: A≥90, B≥80, C≥70, D≥60, F&lt;60.
///
/// Severity base points:
///   Critical = 20, High = 10, Medium = 5, Low = 2, Informational = 0.
/// </summary>
public class ScoreCalculatorService : IScoreCalculatorService
{
    private const string CacheKey = "risk_weights_active";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;

    // Base deduction points per severity level
    private static readonly Dictionary<SeverityLevel, int> BasePoints = new()
    {
        [SeverityLevel.Critical] = 25,
        [SeverityLevel.High] = 15,
        [SeverityLevel.Medium] = 5,
        [SeverityLevel.Low] = 2,
        [SeverityLevel.Informational] = 0,
    };

    public ScoreCalculatorService(IUnitOfWork unitOfWork, IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<(int Score, string Grade)> CalculateAsync(
        IEnumerable<Finding> findings,
        CancellationToken ct = default)
    {
        var weights = await GetWeightsAsync(ct);

        decimal totalDeduction = 0;

        // Group findings by unique Type to avoid dropping the score to zero
        // just because the exact same issue is found on 100 different pages.
        // Pick the most persistent finding per type so it gets the correct multiplier.
        var distinctFindings = findings
            .GroupBy(f => f.Type)
            .Select(g => g
                .OrderByDescending(f => f.PresenteHaScans ?? -1)
                .ThenByDescending(f => (int)f.Severity)
                .First());

        foreach (var finding in distinctFindings)
        {
            var basePoints = BasePoints.TryGetValue(finding.Severity, out var pts) ? pts : 0;
            if (basePoints == 0) continue;

            // Match weight by category (case-insensitive). Fall back to 1.0 (neutral).
            var category = finding.Type;
            var weight = FindWeight(weights, category);

            var multiplier = GetPersistenceMultiplier(finding);
            totalDeduction += basePoints * weight * multiplier;
        }

        var score = (int)Math.Max(0m, 100m - totalDeduction);
        var grade = ToGrade(score);

        return (score, grade);
    }

    // -------------------------------------------------------------------------

    private async Task<Dictionary<string, decimal>> GetWeightsAsync(CancellationToken ct)
    {
        if (_cache.TryGetValue(CacheKey, out Dictionary<string, decimal>? cached) && cached is not null)
            return cached;

        var activeWeights = await _unitOfWork.RiskWeights.GetAllActiveAsync(ct);

        var dict = activeWeights.ToDictionary(
            rw => rw.Category.ToLowerInvariant(),
            rw => rw.Weight);

        _cache.Set(CacheKey, dict, CacheDuration);
        return dict;
    }

    private static decimal GetPersistenceMultiplier(Finding finding)
    {
        if (finding.StatusHistorico is null || finding.PresenteHaScans is null)
            return 1.0m;
        if (finding.StatusHistorico == "novo" || finding.PresenteHaScans == 0)
            return 1.0m;
        if (finding.PresenteHaScans == 1)
            return 1.25m;
        return 1.5m; // PresenteHaScans >= 2
    }

    private static decimal FindWeight(Dictionary<string, decimal> weights, string category)
    {
        // Try exact match first, then partial match for categories like "SSL", "Headers"
        var key = category.ToLowerInvariant();

        if (weights.TryGetValue(key, out var w)) return w;

        // Try partial: if category contains a known key (e.g. "SSL Certificate" → "ssl")
        foreach (var (k, v) in weights)
        {
            if (key.Contains(k, StringComparison.OrdinalIgnoreCase))
                return v;
        }

        return 1.0m; // Default neutral weight
    }

    private static string ToGrade(int score) => score switch
    {
        >= 80 => "A",
        >= 60 => "B",
        >= 40 => "C",
        >= 20 => "D",
        _ => "E"
    };
}

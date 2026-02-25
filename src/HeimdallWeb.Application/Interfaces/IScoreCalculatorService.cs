using HeimdallWeb.Domain.Entities;

namespace HeimdallWeb.Application.Interfaces;

/// <summary>
/// Computes a security score (0–100) and letter grade (A–F) from a list of findings.
/// Weights per category are read from tb_risk_weights and cached in memory.
/// </summary>
public interface IScoreCalculatorService
{
    /// <summary>
    /// Calculates the score and grade for a completed scan.
    /// </summary>
    /// <param name="findings">The findings produced by the AI analysis.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Score (0–100) and Grade ("A"–"F").</returns>
    Task<(int Score, string Grade)> CalculateAsync(IEnumerable<Finding> findings, CancellationToken ct = default);
}

namespace HeimdallWeb.Domain.Entities;

/// <summary>
/// Represents configurable risk weights per scanner category.
/// Allows calibrating the scoring system without redeploy.
/// Stored in tb_risk_weights.
/// </summary>
public class RiskWeight
{
    public int Id { get; private set; }

    /// <summary>
    /// Scanner category identifier, e.g. "SSL", "Headers", "Port", "Robots", "Sensitive", "Redirect".
    /// </summary>
    public string Category { get; private set; } = string.Empty;

    /// <summary>
    /// Multiplier applied to base severity points for findings in this category.
    /// Default is 1.0. Values > 1.0 increase the penalty; values &lt; 1.0 reduce it.
    /// </summary>
    public decimal Weight { get; private set; }

    /// <summary>
    /// When false, findings in this category do not affect the score.
    /// Useful for temporarily disabling noisy scanners without redeploy.
    /// </summary>
    public bool IsActive { get; private set; }

    // Parameterless constructor for EF Core
    private RiskWeight() { }

    public RiskWeight(string category, decimal weight, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be empty.", nameof(category));

        if (weight < 0)
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight cannot be negative.");

        Category = category;
        Weight = weight;
        IsActive = isActive;
    }

    public void UpdateWeight(decimal weight)
    {
        if (weight < 0)
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight cannot be negative.");

        Weight = weight;
    }

    public void SetActive(bool isActive) => IsActive = isActive;
}

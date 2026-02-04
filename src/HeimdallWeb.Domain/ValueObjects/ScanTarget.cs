using System.Text.RegularExpressions;
using HeimdallWeb.Domain.Exceptions;

namespace HeimdallWeb.Domain.ValueObjects;

/// <summary>
/// Value Object representing a validated and normalized scan target (domain/URL).
/// Ensures the target is a valid domain or URL and normalizes it for consistency.
/// </summary>
public sealed class ScanTarget : IEquatable<ScanTarget>
{
    private static readonly Regex DomainRegex = new(
        @"^([a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex UrlRegex = new(
        @"^(https?://)?([a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}(/.*)?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private ScanTarget(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new ScanTarget instance with validation and normalization.
    /// Removes protocol, www prefix, and trailing slashes.
    /// </summary>
    /// <param name="target">The domain or URL to validate</param>
    /// <returns>A validated and normalized ScanTarget instance</returns>
    /// <exception cref="ValidationException">Thrown when target format is invalid</exception>
    public static ScanTarget Create(string target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            throw new ValidationException("Scan target cannot be empty.");
        }

        var normalized = NormalizeTarget(target.Trim());

        if (!DomainRegex.IsMatch(normalized) && !UrlRegex.IsMatch(normalized))
        {
            throw new ValidationException($"Scan target '{target}' is not a valid domain or URL.");
        }

        return new ScanTarget(normalized);
    }

    /// <summary>
    /// Normalizes the target by removing protocol, www, and trailing slashes.
    /// </summary>
    private static string NormalizeTarget(string target)
    {
        var normalized = target.ToLowerInvariant();

        // Remove http:// or https://
        normalized = Regex.Replace(normalized, @"^https?://", string.Empty);

        // Remove www.
        normalized = Regex.Replace(normalized, @"^www\.", string.Empty);

        // Remove trailing slashes and paths for domain-only representation
        normalized = Regex.Replace(normalized, @"/.*$", string.Empty);

        return normalized;
    }

    // Implicit conversion from string
    public static implicit operator string(ScanTarget target) => target.Value;

    // Explicit conversion to ScanTarget
    public static explicit operator ScanTarget(string target) => Create(target);

    public override string ToString() => Value;

    public override bool Equals(object? obj) => obj is ScanTarget other && Equals(other);

    public bool Equals(ScanTarget? other)
    {
        if (other is null) return false;
        return Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode() => Value.GetHashCode(StringComparison.OrdinalIgnoreCase);

    public static bool operator ==(ScanTarget? left, ScanTarget? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(ScanTarget? left, ScanTarget? right) =>
        !(left == right);
}

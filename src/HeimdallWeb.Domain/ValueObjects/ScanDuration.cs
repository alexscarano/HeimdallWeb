using HeimdallWeb.Domain.Exceptions;

namespace HeimdallWeb.Domain.ValueObjects;

/// <summary>
/// Value Object representing a validated scan duration.
/// Ensures duration is positive and valid.
/// </summary>
public sealed class ScanDuration : IEquatable<ScanDuration>
{
    public TimeSpan Value { get; }

    private ScanDuration(TimeSpan value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new ScanDuration instance with validation.
    /// </summary>
    /// <param name="duration">The duration to validate</param>
    /// <returns>A validated ScanDuration instance</returns>
    /// <exception cref="ValidationException">Thrown when duration is negative or zero</exception>
    public static ScanDuration Create(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
        {
            throw new ValidationException("Scan duration must be positive.");
        }

        return new ScanDuration(duration);
    }

    /// <summary>
    /// Creates a ScanDuration from seconds.
    /// </summary>
    public static ScanDuration FromSeconds(double seconds) => Create(TimeSpan.FromSeconds(seconds));

    /// <summary>
    /// Creates a ScanDuration from milliseconds.
    /// </summary>
    public static ScanDuration FromMilliseconds(double milliseconds) => Create(TimeSpan.FromMilliseconds(milliseconds));

    // Implicit conversion from TimeSpan
    public static implicit operator TimeSpan(ScanDuration duration) => duration.Value;

    // Explicit conversion to ScanDuration
    public static explicit operator ScanDuration(TimeSpan duration) => Create(duration);

    public override string ToString() => Value.ToString();

    public override bool Equals(object? obj) => obj is ScanDuration other && Equals(other);

    public bool Equals(ScanDuration? other)
    {
        if (other is null) return false;
        return Value.Equals(other.Value);
    }

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(ScanDuration? left, ScanDuration? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(ScanDuration? left, ScanDuration? right) =>
        !(left == right);
}

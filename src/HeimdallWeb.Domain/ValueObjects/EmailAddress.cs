using System.Text.RegularExpressions;
using HeimdallWeb.Domain.Exceptions;

namespace HeimdallWeb.Domain.ValueObjects;

/// <summary>
/// Value Object representing a validated email address.
/// Ensures email format is valid and normalized.
/// </summary>
public sealed class EmailAddress : IEquatable<EmailAddress>
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private EmailAddress(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new EmailAddress instance with validation.
    /// </summary>
    /// <param name="email">The email address to validate</param>
    /// <returns>A validated EmailAddress instance</returns>
    /// <exception cref="ValidationException">Thrown when email format is invalid</exception>
    public static EmailAddress Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ValidationException("Email address cannot be empty.");
        }

        // Normalize to lowercase
        var normalized = email.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(normalized))
        {
            throw new ValidationException($"Email address '{email}' is not valid.");
        }

        return new EmailAddress(normalized);
    }

    // Implicit conversion from string
    public static implicit operator string(EmailAddress email) => email.Value;

    // Explicit conversion to EmailAddress
    public static explicit operator EmailAddress(string email) => Create(email);

    public override string ToString() => Value;

    public override bool Equals(object? obj) => obj is EmailAddress other && Equals(other);

    public bool Equals(EmailAddress? other)
    {
        if (other is null) return false;
        return Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode() => Value.GetHashCode(StringComparison.OrdinalIgnoreCase);

    public static bool operator ==(EmailAddress? left, EmailAddress? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(EmailAddress? left, EmailAddress? right) =>
        !(left == right);
}

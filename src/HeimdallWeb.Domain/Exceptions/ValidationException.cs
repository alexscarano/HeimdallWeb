namespace HeimdallWeb.Domain.Exceptions;

/// <summary>
/// Exception thrown when domain validation rules are violated.
/// </summary>
public class ValidationException : DomainException
{
    public ValidationException(string message) : base(message)
    {
    }
}

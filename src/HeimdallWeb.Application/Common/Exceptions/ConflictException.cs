namespace HeimdallWeb.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a resource conflict occurs (e.g., duplicate email/username).
/// Maps to HTTP 409 Conflict status code.
/// </summary>
public class ConflictException : ApplicationException
{
    public ConflictException()
        : base("Resource conflict detected.")
    {
    }

    public ConflictException(string message)
        : base(message)
    {
    }
}

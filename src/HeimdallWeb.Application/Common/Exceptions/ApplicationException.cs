namespace HeimdallWeb.Application.Common.Exceptions;

/// <summary>
/// Base exception for application layer errors.
/// </summary>
public class ApplicationException : Exception
{
    public ApplicationException()
    {
    }

    public ApplicationException(string message)
        : base(message)
    {
    }

    public ApplicationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

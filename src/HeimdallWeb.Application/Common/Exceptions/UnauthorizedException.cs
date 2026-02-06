namespace HeimdallWeb.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when user is not authorized to perform an action.
/// </summary>
public class UnauthorizedException : ApplicationException
{
    public UnauthorizedException()
        : base("Unauthorized access.")
    {
    }

    public UnauthorizedException(string message)
        : base(message)
    {
    }
}

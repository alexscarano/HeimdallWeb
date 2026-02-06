namespace HeimdallWeb.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when user is forbidden from performing an action.
/// </summary>
public class ForbiddenException : ApplicationException
{
    public ForbiddenException()
        : base("Access forbidden.")
    {
    }

    public ForbiddenException(string message)
        : base(message)
    {
    }
}

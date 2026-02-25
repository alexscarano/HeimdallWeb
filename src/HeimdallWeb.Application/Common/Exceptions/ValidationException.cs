namespace HeimdallWeb.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public class ValidationException : ApplicationException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException()
        : base("One or more validation errors occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string propertyName, string errorMessage)
        : base($"Validation failed for {propertyName}: {errorMessage}")
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, new[] { errorMessage } }
        };
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}

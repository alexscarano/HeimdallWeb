using FluentValidation;

namespace HeimdallWeb.Application.Commands.Scan.ExecuteScan;

/// <summary>
/// Validator for ExecuteScanCommand.
/// Validates target URL format, user ID, and remote IP format.
/// </summary>
public class ExecuteScanCommandValidator : AbstractValidator<ExecuteScanCommand>
{
    public ExecuteScanCommandValidator()
    {
        RuleFor(x => x.Target)
            .NotEmpty().WithMessage("Target URL or IP address is required")
            .MaximumLength(500).WithMessage("Target URL is too long (max 500 characters)")
            .Must(BeValidUrlOrIp).WithMessage("Target must be a valid URL or IP address");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.RemoteIp)
            .NotEmpty().WithMessage("Remote IP address is required")
            .Must(BeValidIp).WithMessage("Invalid IP address format");
    }

    private bool BeValidUrlOrIp(string target)
    {
        if (string.IsNullOrWhiteSpace(target))
            return false;

        // Try to parse as IP address
        if (System.Net.IPAddress.TryParse(target, out _))
            return true;

        // Try to parse as URL with explicit scheme
        if (Uri.TryCreate(target, UriKind.Absolute, out var uriResult))
        {
            if (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)
            {
                // Ensure host is valid (not just a string without dots or proper domain)
                return !string.IsNullOrWhiteSpace(uriResult.Host) && 
                       (uriResult.Host.Contains('.') || uriResult.Host == "localhost");
            }
        }

        // Try to parse as domain/hostname without scheme (add https:// prefix)
        if (!target.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !target.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            var withScheme = $"https://{target}";
            
            if (Uri.TryCreate(withScheme, UriKind.Absolute, out var uriResult2) &&
                (uriResult2.Scheme == Uri.UriSchemeHttp || uriResult2.Scheme == Uri.UriSchemeHttps))
            {
                // Validate domain has at least one dot (e.g., example.com) or is localhost
                return !string.IsNullOrWhiteSpace(uriResult2.Host) && 
                       (uriResult2.Host.Contains('.') || uriResult2.Host == "localhost");
            }
        }

        return false;
    }

    private bool BeValidIp(string ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
            return false;

        return System.Net.IPAddress.TryParse(ip, out _);
    }
}

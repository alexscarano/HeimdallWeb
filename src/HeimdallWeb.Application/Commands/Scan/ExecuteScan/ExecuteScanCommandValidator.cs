using FluentValidation;
using HeimdallWeb.Application.Helpers;

namespace HeimdallWeb.Application.Commands.Scan.ExecuteScan;

/// <summary>
/// Validator for ExecuteScanCommand.
/// Validates target URL format, DNS resolution, user ID, and remote IP format.
/// Fails fast if domain doesn't exist (prevents 75-second timeout).
/// Localhost and IP addresses are NOT allowed.
/// </summary>
public class ExecuteScanCommandValidator : AbstractValidator<ExecuteScanCommand>
{
    public ExecuteScanCommandValidator()
    {
        RuleFor(x => x.Target)
            .NotEmpty().WithMessage("Target URL or domain is required")
            .MaximumLength(500).WithMessage("Target URL is too long (max 500 characters)")
            .Must(BeValidUrlOrIp).WithMessage("Target must be a valid domain or URL (localhost and IP addresses not accepted)")
            .DependentRules(() =>
            {
                // Only check DNS if format is valid (avoids duplicate error messages)
                RuleFor(x => x.Target)
                    .Must(BeResolvableHost).WithMessage("Target domain does not exist or cannot be resolved. Please verify the domain is correct.");
            });

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.RemoteIp)
            .NotEmpty().WithMessage("Remote IP address is required")
            .Must(BeValidIp).WithMessage("Invalid IP address format");
    }

    /// <summary>
    /// Validates URL/domain syntax (format check).
    /// IP addresses and localhost are NOT accepted - only public domains/URLs.
    /// </summary>
    private bool BeValidUrlOrIp(string target)
    {
        if (string.IsNullOrWhiteSpace(target))
            return false;

        // Reject raw IP addresses - only domains allowed
        if (NetworkUtils.IsIPAddress(target))
            return false;

        // Try to parse as URL with NetworkUtils
        var normalized = NetworkUtils.NormalizeUrl(target);
        if (string.IsNullOrEmpty(normalized))
            return false;

        if (NetworkUtils.IsValidUrl(normalized, out var uriResult))
        {
            // Reject localhost explicitly
            if (uriResult.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                return false;

            // Reject IP addresses in URL
            if (NetworkUtils.IsIPAddress(uriResult.Host))
                return false;

            // Ensure host has proper domain format (must have dots for TLD)
            return !string.IsNullOrWhiteSpace(uriResult.Host) && uriResult.Host.Contains('.');
        }

        return false;
    }

    /// <summary>
    /// Validates that the domain can be resolved via DNS.
    /// Assumes BeValidUrlOrIp already validated format (no localhost/IPs here).
    /// </summary>
    private bool BeResolvableHost(string target)
    {
        if (string.IsNullOrWhiteSpace(target))
            return false;

        try
        {
            // Normalize the URL first
            var normalized = NetworkUtils.NormalizeUrl(target);
            if (string.IsNullOrEmpty(normalized))
                return false;

            // Try DNS resolution - this will throw if domain doesn't exist
            // DNS resolution automatically gets IP addresses
            var addresses = NetworkUtils.GetIPv4Addresses(normalized);
            return addresses != null && addresses.Length > 0;
        }
        catch
        {
            // DNS resolution failed - domain doesn't exist
            return false;
        }
    }

    private bool BeValidIp(string ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
            return false;

        return System.Net.IPAddress.TryParse(ip, out _);
    }
}

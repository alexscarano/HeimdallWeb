using FluentValidation;
using HeimdallWeb.Application.Helpers;

namespace HeimdallWeb.Application.Commands.Scan.ExecuteScan;

/// <summary>
/// Validator for ExecuteScanCommand.
/// Validates target URL format, DNS resolution, user ID, and remote IP format.
/// Fails fast if domain doesn't exist (prevents 75-second timeout).
/// </summary>
public class ExecuteScanCommandValidator : AbstractValidator<ExecuteScanCommand>
{
    public ExecuteScanCommandValidator()
    {
        RuleFor(x => x.Target)
            .NotEmpty().WithMessage("Target URL or IP address is required")
            .MaximumLength(500).WithMessage("Target URL is too long (max 500 characters)")
            .Must(BeValidUrlOrIp).WithMessage("Target must be a valid URL or IP address")
            .Must(BeResolvableHost).WithMessage("Target domain does not exist or cannot be resolved. Please verify the domain is correct.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.RemoteIp)
            .NotEmpty().WithMessage("Remote IP address is required")
            .Must(BeValidIp).WithMessage("Invalid IP address format");
    }

    /// <summary>
    /// Validates URL/IP syntax (format check).
    /// </summary>
    private bool BeValidUrlOrIp(string target)
    {
        if (string.IsNullOrWhiteSpace(target))
            return false;

        // Try to parse as IP address
        if (NetworkUtils.IsIPAddress(target))
            return true;

        // Try to parse as URL with NetworkUtils
        var normalized = NetworkUtils.NormalizeUrl(target);
        if (string.IsNullOrEmpty(normalized))
            return false;

        if (NetworkUtils.IsValidUrl(normalized, out var uriResult))
        {
            // Ensure host is valid (not just a string without dots or proper domain)
            return !string.IsNullOrWhiteSpace(uriResult.Host) && 
                   (uriResult.Host.Contains('.') || uriResult.Host == "localhost");
        }

        return false;
    }

    /// <summary>
    /// Validates that the domain/host can be resolved via DNS.
    /// For IPs, this always returns true (no DNS needed).
    /// For domains, performs DNS lookup to verify existence.
    /// </summary>
    private bool BeResolvableHost(string target)
    {
        if (string.IsNullOrWhiteSpace(target))
            return false;

        try
        {
            // If it's already an IP address, it's resolvable
            if (NetworkUtils.IsIPAddress(target))
                return true;

            // Normalize the URL first
            var normalized = NetworkUtils.NormalizeUrl(target);
            if (string.IsNullOrEmpty(normalized))
                return false;

            // Extract host from URL
            var host = NetworkUtils.RemoveHttpString(normalized);
            if (string.IsNullOrWhiteSpace(host))
                return false;

            // Localhost is always valid
            if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                return true;

            // Try DNS resolution - this will throw if domain doesn't exist
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

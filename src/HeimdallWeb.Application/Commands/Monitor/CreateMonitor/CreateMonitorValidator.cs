using System.Net;
using FluentValidation;
using HeimdallWeb.Application.Helpers;
using HeimdallWeb.Domain.Enums;

namespace HeimdallWeb.Application.Commands.Monitor.CreateMonitor;

/// <summary>
/// Validator for <see cref="CreateMonitorCommand"/>.
/// Mirrors the SSRF protections from ExecuteScanCommandValidator:
/// blocks localhost, private IPs, link-local, and raw IP addresses.
/// </summary>
public class CreateMonitorValidator : AbstractValidator<CreateMonitorCommand>
{
    /// <summary>Maximum number of monitored targets allowed per user.</summary>
    public const int MaxMonitorsPerUser = 20;

    public CreateMonitorValidator()
    {
        RuleFor(x => x.UserPublicId)
            .NotEmpty().WithMessage("UserPublicId cannot be empty.");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("URL is required.")
            .MaximumLength(2048).WithMessage("URL cannot exceed 2048 characters.")
            .Must(BeAValidPublicUrl).WithMessage("URL must be a valid public HTTP or HTTPS address. Localhost, private IPs, and raw IP addresses are not allowed.")
            .DependentRules(() =>
            {
                RuleFor(x => x.Url)
                    .Must(BeResolvableHost).WithMessage("Target domain does not exist or cannot be resolved.");
            });

        RuleFor(x => x.Frequency)
            .IsInEnum().WithMessage("Frequency must be Daily or Weekly.");
    }

    /// <summary>
    /// Validates URL format and blocks SSRF vectors: localhost, private IPs, link-local, raw IPs.
    /// Mirrors the same protections applied in ExecuteScanCommandValidator.
    /// </summary>
    private static bool BeAValidPublicUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        // Reject raw IP addresses
        if (NetworkUtils.IsIPAddress(url))
            return false;

        var normalized = NetworkUtils.NormalizeUrl(url);
        if (string.IsNullOrEmpty(normalized))
            return false;

        if (!NetworkUtils.IsValidUrl(normalized, out var uriResult) || uriResult == null)
            return false;

        // Reject localhost
        if (uriResult.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            return false;

        // Reject IP addresses embedded in URLs
        if (NetworkUtils.IsIPAddress(uriResult.Host))
            return false;

        // Reject hosts without a TLD (e.g., "intranet", "db-server")
        if (!uriResult.Host.Contains('.'))
            return false;

        // Attempt to resolve and reject private/reserved IP ranges
        try
        {
            var addresses = Dns.GetHostAddresses(uriResult.Host);
            foreach (var addr in addresses)
            {
                if (IsPrivateOrReservedAddress(addr))
                    return false;
            }
        }
        catch
        {
            // DNS resolution failure will be caught by BeResolvableHost
        }

        return true;
    }

    /// <summary>
    /// Validates that the domain can be resolved via DNS.
    /// </summary>
    private static bool BeResolvableHost(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        try
        {
            var normalized = NetworkUtils.NormalizeUrl(url);
            if (string.IsNullOrEmpty(normalized))
                return false;

            var addresses = NetworkUtils.GetIPv4Addresses(normalized);
            return addresses != null && addresses.Length > 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Returns true if the IP address belongs to a private, loopback, link-local, or reserved range.
    /// Prevents SSRF attacks targeting internal infrastructure or cloud metadata endpoints.
    /// </summary>
    private static bool IsPrivateOrReservedAddress(IPAddress address)
    {
        var bytes = address.GetAddressBytes();

        // IPv4 checks
        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && bytes.Length == 4)
        {
            // 127.0.0.0/8 (loopback)
            if (bytes[0] == 127)
                return true;

            // 10.0.0.0/8 (private)
            if (bytes[0] == 10)
                return true;

            // 172.16.0.0/12 (private)
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                return true;

            // 192.168.0.0/16 (private)
            if (bytes[0] == 192 && bytes[1] == 168)
                return true;

            // 169.254.0.0/16 (link-local, includes AWS metadata 169.254.169.254)
            if (bytes[0] == 169 && bytes[1] == 254)
                return true;

            // 0.0.0.0/8
            if (bytes[0] == 0)
                return true;
        }

        // IPv6: reject loopback (::1) and link-local (fe80::/10)
        if (IPAddress.IsLoopback(address))
            return true;

        if (address.IsIPv6LinkLocal)
            return true;

        return false;
    }
}

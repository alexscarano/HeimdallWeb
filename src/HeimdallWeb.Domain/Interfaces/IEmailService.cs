namespace HeimdallWeb.Domain.Interfaces;

/// <summary>
/// Contract for sending transactional emails from the application.
/// Implementations should handle graceful degradation when SMTP is not configured.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a password reset email with a secure reset link.
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="toName">Recipient display name</param>
    /// <param name="resetLink">The full password reset URL (contains the raw token)</param>
    /// <param name="ct">Cancellation token</param>
    Task SendPasswordResetEmailAsync(
        string toEmail,
        string toName,
        string resetLink,
        CancellationToken ct = default);

    /// <summary>
    /// Forwards a support contact form submission to the configured support email.
    /// </summary>
    /// <param name="fromName">Sender's name</param>
    /// <param name="fromEmail">Sender's email address</param>
    /// <param name="subject">Message subject</param>
    /// <param name="message">Message body</param>
    /// <param name="ct">Cancellation token</param>
    Task SendContactEmailAsync(
        string fromName,
        string fromEmail,
        string subject,
        string message,
        CancellationToken ct = default);
}

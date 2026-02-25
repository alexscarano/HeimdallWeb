using System.Net;
using HeimdallWeb.Domain.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace HeimdallWeb.Infrastructure.External;

/// <summary>
/// Email service implementation using MailKit and MimeKit.
/// Provides graceful degradation when SMTP is not configured (useful for development environments).
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    // Configuration keys
    private const string SmtpHostKey = "Email:SmtpHost";
    private const string SmtpPortKey = "Email:SmtpPort";
    private const string UsernameKey = "Email:Username";
    private const string PasswordKey = "Email:Password";
    private const string FromAddressKey = "Email:FromAddress";
    private const string FromNameKey = "Email:FromName";
    private const string ContactEmailKey = "Email:ContactEmail";

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task SendPasswordResetEmailAsync(
        string toEmail,
        string toName,
        string resetLink,
        CancellationToken ct = default)
    {
        if (!IsSmtpConfigured())
        {
            // Security: NEVER log the reset link — it contains the raw token
            _logger.LogWarning(
                "SMTP is not configured. Skipping password reset email to {Email}. Token will expire unused.",
                toEmail);
            return;
        }

        var fromAddress = _configuration[FromAddressKey] ?? "noreply@heimdall.app";
        var fromName = _configuration[FromNameKey] ?? "HeimdallWeb";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = "HeimdallWeb — Password Reset Request";

        var body = new BodyBuilder
        {
            HtmlBody = BuildPasswordResetHtml(toName, resetLink),
            TextBody = BuildPasswordResetText(toName, resetLink)
        };

        message.Body = body.ToMessageBody();

        await SendMessageAsync(message, ct);

        _logger.LogInformation("Password reset email sent to {Email}", toEmail);
    }

    /// <inheritdoc />
    public async Task SendContactEmailAsync(
        string fromName,
        string fromEmail,
        string subject,
        string message,
        CancellationToken ct = default)
    {
        if (!IsSmtpConfigured())
        {
            _logger.LogWarning(
                "SMTP is not configured. Skipping contact form email from {FromEmail} (subject: {Subject})",
                fromEmail,
                subject);
            return;
        }

        var smtpFromAddress = _configuration[FromAddressKey] ?? "noreply@heimdall.app";
        var smtpFromName = _configuration[FromNameKey] ?? "HeimdallWeb";
        var contactEmail = _configuration[ContactEmailKey] ?? smtpFromAddress;

        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(smtpFromName, smtpFromAddress));
        mimeMessage.To.Add(new MailboxAddress("Support Team", contactEmail));
        mimeMessage.ReplyTo.Add(new MailboxAddress(fromName, fromEmail));
        mimeMessage.Subject = $"[Contact Form] {subject}";

        var body = new BodyBuilder
        {
            HtmlBody = BuildContactEmailHtml(fromName, fromEmail, subject, message),
            TextBody = BuildContactEmailText(fromName, fromEmail, subject, message)
        };

        mimeMessage.Body = body.ToMessageBody();

        await SendMessageAsync(mimeMessage, ct);

        _logger.LogInformation(
            "Contact form email forwarded from {FromEmail} (subject: {Subject})",
            fromEmail,
            subject);
    }

    /// <summary>
    /// Sends a MIME message via SMTP, handling connection and authentication.
    /// </summary>
    private async Task SendMessageAsync(MimeMessage message, CancellationToken ct)
    {
        var host = _configuration[SmtpHostKey] ?? "smtp.gmail.com";
        var port = int.TryParse(_configuration[SmtpPortKey], out var p) ? p : 587;
        var username = _configuration[UsernameKey]!;
        var password = _configuration[PasswordKey] ?? string.Empty;

        using var client = new SmtpClient();

        try
        {
            // Use StartTls for port 587 (STARTTLS), or SslOnConnect for port 465 (SMTPS)
            // Security: use StartTls (not StartTlsWhenAvailable) to REQUIRE TLS — prevents downgrade attacks
            var secureSocketOptions = port == 465
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;

            await client.ConnectAsync(host, port, secureSocketOptions, ct);
            await client.AuthenticateAsync(username, password, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(quit: true, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via SMTP to {Host}:{Port}", host, port);
            throw;
        }
    }

    /// <summary>
    /// Returns true if SMTP username (the minimum required credential) is configured.
    /// When username is empty, the service degrades gracefully (logs and returns).
    /// </summary>
    private bool IsSmtpConfigured()
    {
        var username = _configuration[UsernameKey];
        return !string.IsNullOrWhiteSpace(username);
    }

    private static string BuildPasswordResetHtml(string toName, string resetLink)
    {
        // Security: HTML-encode user-controlled values
        var safeName = WebUtility.HtmlEncode(toName);
        var safeLink = WebUtility.HtmlEncode(resetLink);

        return $"""
                <!DOCTYPE html>
                <html lang="pt-BR">
                <head><meta charset="utf-8"><title>Redefinição de Senha</title></head>
                <body style="font-family: Arial, sans-serif; background: #f4f4f4; padding: 20px;">
                  <div style="max-width: 600px; margin: 0 auto; background: #fff; border-radius: 8px; padding: 32px;">
                    <h2 style="color: #1a1a2e;">HeimdallWeb — Redefinição de Senha</h2>
                    <p>Olá, <strong>{safeName}</strong></p>
                    <p>Recebemos uma solicitação para redefinir a senha da sua conta no HeimdallWeb. Clique no botão abaixo para criar uma nova senha.</p>
                    <p style="text-align: center; margin: 32px 0;">
                      <a href="{safeLink}"
                         style="background: #4f46e5; color: #fff; padding: 12px 24px; border-radius: 6px; text-decoration: none; font-weight: bold;">
                        Redefinir Senha
                      </a>
                    </p>
                    <p style="color: #666; font-size: 14px;">Este link expira em <strong>1 hora</strong>. Se você não solicitou a redefinição de senha, pode ignorar este e-mail com segurança.</p>
                    <hr style="border: none; border-top: 1px solid #eee; margin: 24px 0;">
                    <p style="color: #aaa; font-size: 12px;">HeimdallWeb Security Scanner &mdash; Por favor, não responda a este e-mail.</p>
                  </div>
                </body>
                </html>
                """;
    }

    private static string BuildPasswordResetText(string toName, string resetLink)
    {
        return $"""
                HeimdallWeb — Redefinição de Senha

                Olá, {toName}

                Recebemos uma solicitação para redefinir a senha da sua conta no HeimdallWeb. 
                Clique no link abaixo para criar uma nova senha (o link expira em 1 hora):

                {resetLink}

                Se você não solicitou a redefinição de senha, pode ignorar este e-mail com segurança.

                HeimdallWeb Security Scanner
                """;
    }

    private static string BuildContactEmailHtml(string fromName, string fromEmail, string subject, string message)
    {
        // Security: HTML-encode all user-supplied values to prevent HTML/script injection
        var safeName = WebUtility.HtmlEncode(fromName);
        var safeEmail = WebUtility.HtmlEncode(fromEmail);
        var safeSubject = WebUtility.HtmlEncode(subject);
        var safeMessage = WebUtility.HtmlEncode(message);

        return $"""
                <!DOCTYPE html>
                <html lang="pt-BR">
                <head><meta charset="utf-8"><title>Formulário de Contato</title></head>
                <body style="font-family: Arial, sans-serif; background: #f4f4f4; padding: 20px;">
                  <div style="max-width: 600px; margin: 0 auto; background: #fff; border-radius: 8px; padding: 32px;">
                    <h2 style="color: #1a1a2e;">Novo Envio do Formulário de Contato</h2>
                    <table style="width: 100%; border-collapse: collapse;">
                      <tr><td style="padding: 8px; font-weight: bold; width: 120px;">De:</td><td style="padding: 8px;">{safeName} ({safeEmail})</td></tr>
                      <tr><td style="padding: 8px; font-weight: bold;">Assunto:</td><td style="padding: 8px;">{safeSubject}</td></tr>
                    </table>
                    <hr style="border: none; border-top: 1px solid #eee; margin: 16px 0;">
                    <h3 style="color: #333;">Mensagem:</h3>
                    <p style="white-space: pre-line; color: #444;">{safeMessage}</p>
                    <hr style="border: none; border-top: 1px solid #eee; margin: 24px 0;">
                    <p style="color: #aaa; font-size: 12px;">Responda diretamente a este e-mail para contatar o remetente.</p>
                  </div>
                </body>
                </html>
                """;
    }

    private static string BuildContactEmailText(string fromName, string fromEmail, string subject, string message)
    {
        return $"""
                Novo Envio do Formulário de Contato — HeimdallWeb

                De: {fromName} ({fromEmail})
                Assunto: {subject}

                Mensagem:
                {message}

                --
                Responda diretamente a este e-mail para contatar o remetente.
                """;
    }
}

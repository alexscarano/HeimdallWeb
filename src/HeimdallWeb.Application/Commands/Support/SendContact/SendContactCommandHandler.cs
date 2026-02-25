using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HeimdallWeb.Application.Commands.Support.SendContact;

/// <summary>
/// Handles the support contact form submission:
/// 1. Validates input via FluentValidation.
/// 2. Forwards the message to the configured support email address.
/// 3. Logs the event in the audit log (anonymous — no userId).
/// </summary>
public class SendContactCommandHandler : ICommandHandler<SendContactCommand, SendContactResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<SendContactCommandHandler> _logger;

    public SendContactCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<SendContactCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SendContactResponse> Handle(SendContactCommand command, CancellationToken ct = default)
    {
        // Validate input via FluentValidation
        var validator = new SendContactCommandValidator();
        var validationResult = await validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
            throw new ValidationException(errors);
        }

        // Forward message to support email — graceful degradation if SMTP not configured
        try
        {
            await _emailService.SendContactEmailAsync(
                fromName: command.Name.Trim(),
                fromEmail: command.Email.Trim().ToLower(),
                subject: command.Subject.Trim(),
                message: command.Message.Trim(),
                ct: ct);
        }
        catch (Exception ex)
        {
            // Log but do not expose internal error to the client
            _logger.LogError(ex,
                "Failed to forward contact form email from {Email} (subject: {Subject})",
                command.Email,
                command.Subject);
        }

        // Audit log — anonymous contact form (no userId)
        var log = new Domain.Entities.AuditLog(
            code: LogEventCode.CONTACT_FORM_SENT,
            level: "Info",
            message: "Support contact form submitted",
            source: "SendContactCommandHandler",
            details: $"From: {command.Name} <{command.Email}>, Subject: {command.Subject}",
            remoteIp: command.RemoteIp
        );

        await _unitOfWork.AuditLogs.AddAsync(log, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Contact form submitted from {Email} (subject: {Subject}) via IP {RemoteIp}",
            command.Email,
            command.Subject,
            command.RemoteIp);

        return new SendContactResponse("Your message was sent successfully.");
    }
}

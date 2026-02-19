using FluentValidation;
using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Monitor;
using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.Application.Commands.Monitor.CreateMonitor;

/// <summary>
/// Handles <see cref="CreateMonitorCommand"/>.
/// Resolves the user's internal ID from their public UUID, checks for duplicate registrations,
/// persists the new <see cref="MonitoredTarget"/>, and returns a <see cref="MonitoredTargetResponse"/>.
/// </summary>
public class CreateMonitorCommandHandler : ICommandHandler<CreateMonitorCommand, MonitoredTargetResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateMonitorCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<MonitoredTargetResponse> Handle(
        CreateMonitorCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validate command
        var validator = new CreateMonitorValidator();
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        // Resolve user's internal integer ID from their public UUID
        var user = await _unitOfWork.Users.GetByPublicIdAsync(command.UserPublicId, cancellationToken);
        if (user == null)
            throw new NotFoundException("User", command.UserPublicId);

        // Enforce per-user monitor limit to prevent resource exhaustion (DoS)
        var existingTargets = await _unitOfWork.MonitoredTargets.GetByUserIdAsync(user.UserId, cancellationToken);
        if (existingTargets.Count() >= CreateMonitorValidator.MaxMonitorsPerUser)
            throw new ConflictException($"Maximum number of monitored targets ({CreateMonitorValidator.MaxMonitorsPerUser}) reached. Remove an existing target before adding a new one.");

        // Prevent duplicate registrations for the same user + URL combination
        var alreadyExists = await _unitOfWork.MonitoredTargets.ExistsByUserAndUrlAsync(
            user.UserId, command.Url, cancellationToken);

        if (alreadyExists)
            throw new ConflictException("URL already being monitored.");

        // Create and persist the new monitored target using the internal int UserId
        var target = new MonitoredTarget(user.UserId, command.Url, command.Frequency);
        await _unitOfWork.MonitoredTargets.AddAsync(target, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new MonitoredTargetResponse(
            Id: target.Id,
            Url: target.Url,
            Frequency: target.Frequency.ToString(),
            LastCheck: target.LastCheck,
            NextCheck: target.NextCheck,
            IsActive: target.IsActive,
            CreatedAt: target.CreatedAt);
    }
}

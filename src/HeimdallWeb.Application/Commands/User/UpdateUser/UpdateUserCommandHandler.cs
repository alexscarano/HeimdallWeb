using HeimdallWeb.Application.Common.Exceptions;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.User;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.Interfaces;
using HeimdallWeb.Domain.ValueObjects;

namespace HeimdallWeb.Application.Commands.User.UpdateUser;

/// <summary>
/// Handles updating user profile information (username and/or email).
/// Validates uniqueness of username and email before updating.
/// </summary>
public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, UpdateUserResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateUserResponse> Handle(UpdateUserCommand request, CancellationToken ct = default)
    {
        // Validate input
        var validator = new UpdateUserCommandValidator();
        var validationResult = await validator.ValidateAsync(request, ct);
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

        // Security check: verify user can only update themselves
        if (request.UserId != request.RequestingUserId)
        {
            throw new ForbiddenException("You can only update your own profile");
        }

        // Get user from database by PublicId
        var user = await _unitOfWork.Users.GetByPublicIdAsync(request.UserId, ct);
        if (user is null)
        {
            throw new NotFoundException("User", request.UserId);
        }

        var isUpdated = false;

        // Update username if provided
        if (!string.IsNullOrWhiteSpace(request.NewUsername))
        {
            var trimmedUsername = request.NewUsername.Trim();

            // Check if username is already taken by another user
            var existingUserWithUsername = await _unitOfWork.Users
                .GetByUsernameAsync(trimmedUsername, user.UserId, ct);

            if (existingUserWithUsername is not null)
            {
                throw new ConflictException($"Username '{trimmedUsername}' is already taken");
            }

            // Update username
            user.UpdateUsername(trimmedUsername);
            isUpdated = true;
        }

        // Update email if provided
        if (!string.IsNullOrWhiteSpace(request.NewEmail))
        {
            var trimmedEmail = request.NewEmail.Trim().ToLower();
            var emailAddress = EmailAddress.Create(trimmedEmail);

            // Check if email is already taken by another user
            var existingUserWithEmail = await _unitOfWork.Users
                .GetByEmailAsync(emailAddress, user.UserId, ct);

            if (existingUserWithEmail is not null)
            {
                throw new ConflictException($"Email '{trimmedEmail}' is already registered");
            }

            // Update email
            user.UpdateEmail(emailAddress);
            isUpdated = true;
        }

        // Save changes if any updates were made
        if (isUpdated)
        {
            await _unitOfWork.Users.UpdateAsync(user, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            // Log successful update
            await LogProfileUpdateAsync(user.UserId, ct);
        }

        return new UpdateUserResponse(
            UserId: user.PublicId,
            Username: user.Username,
            Email: user.Email.Value,
            UserType: (int)user.UserType,
            IsActive: user.IsActive
        );
    }

    private async Task LogProfileUpdateAsync(int userId, CancellationToken ct)
    {
        var log = new Domain.Entities.AuditLog(
            code: LogEventCode.USER_PROFILE_UPDATED,
            level: "Info",
            message: "User profile updated successfully",
            source: "UpdateUserCommandHandler",
            userId: userId
        );

        await _unitOfWork.AuditLogs.AddAsync(log, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}

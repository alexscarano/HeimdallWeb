using System.Security.Claims;
using HeimdallWeb.Application.Commands.User.UpdateUser;
using HeimdallWeb.Application.Commands.User.UpdatePassword;
using HeimdallWeb.Application.Commands.User.DeleteUser;
using HeimdallWeb.Application.Commands.User.UpdateProfileImage;
using HeimdallWeb.Application.Queries.User.GetUserProfile;
using HeimdallWeb.Application.Queries.User.GetUserStatistics;
using HeimdallWeb.Application.DTOs.User;
using HeimdallWeb.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HeimdallWeb.WebApi.Endpoints;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/{id:guid}/profile", GetUserProfile);
        group.MapGet("/{id:guid}/statistics", GetUserStatistics);
        group.MapPut("/{id:guid}", UpdateUser);
        group.MapPatch("/{id:guid}/password", UpdatePassword);
        group.MapDelete("/{id:guid}", DeleteUser);
        group.MapPost("/{id:guid}/profile-image", UpdateProfileImage);

        return group;
    }

    private static async Task<IResult> GetUserProfile(
        Guid id,
        IQueryHandler<GetUserProfileQuery, UserProfileResponse> handler,
        HttpContext context)
    {
        var authenticatedUserId = Guid.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        var query = new GetUserProfileQuery(id, authenticatedUserId);
        var result = await handler.Handle(query);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetUserStatistics(
        Guid id,
        IQueryHandler<GetUserStatisticsQuery, UserStatisticsResponse> handler,
        HttpContext context)
    {
        var authenticatedUserId = Guid.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        var query = new GetUserStatisticsQuery(id, authenticatedUserId);
        var result = await handler.Handle(query);

        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateUser(
        Guid id,
        [FromBody] UpdateUserCommand request,
        ICommandHandler<UpdateUserCommand, UpdateUserResponse> handler,
        HttpContext context)
    {
        var authenticatedUserId = Guid.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        // Create command with authenticated user context
        var command = new UpdateUserCommand(
            id,
            authenticatedUserId,
            request.NewUsername,
            request.NewEmail
        );

        var result = await handler.Handle(command);

        return Results.Ok(result);
    }

    private static async Task<IResult> UpdatePassword(
        Guid id,
        [FromBody] UpdatePasswordCommand request,
        ICommandHandler<UpdatePasswordCommand, UpdatePasswordResponse> handler,
        HttpContext context)
    {
        var authenticatedUserId = Guid.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        // Create command with authenticated user context
        var command = new UpdatePasswordCommand(
            id,
            authenticatedUserId,
            request.CurrentPassword,
            request.NewPassword,
            request.ConfirmPassword
        );

        var result = await handler.Handle(command);

        return Results.Ok(result);
    }

    private static async Task<IResult> DeleteUser(
        Guid id,
        ICommandHandler<DeleteUserCommand, DeleteUserResponse> handler,
        HttpContext context,
        string? password = null,
        bool confirmDelete = false)
    {
        var authenticatedUserId = Guid.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        // For admin users, password confirmation may not be required
        // For regular users, they must provide their password to delete their own account
        var command = new DeleteUserCommand(id, authenticatedUserId, password ?? string.Empty, confirmDelete);
        var result = await handler.Handle(command);

        return Results.Ok(new { message = "User deleted successfully", userId = id });
    }

    private static async Task<IResult> UpdateProfileImage(
        Guid id,
        [FromBody] UpdateProfileImageCommand request,
        ICommandHandler<UpdateProfileImageCommand, UpdateProfileImageResponse> handler,
        HttpContext context)
    {
        var authenticatedUserId = Guid.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        // Create command with authenticated user context
        var command = new UpdateProfileImageCommand(
            id,
            request.ImageBase64,
            authenticatedUserId
        );

        var result = await handler.Handle(command);

        return Results.Ok(result);
    }
}

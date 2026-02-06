using System.Security.Claims;
using HeimdallWeb.Application.Commands.User.UpdateUser;
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

        group.MapGet("/{id:int}/profile", GetUserProfile);
        group.MapGet("/{id:int}/statistics", GetUserStatistics);
        group.MapPut("/{id:int}", UpdateUser);
        group.MapDelete("/{id:int}", DeleteUser);
        group.MapPost("/{id:int}/profile-image", UpdateProfileImage);

        return group;
    }

    private static async Task<IResult> GetUserProfile(
        int id,
        IQueryHandler<GetUserProfileQuery, UserProfileResponse> handler)
    {
        var query = new GetUserProfileQuery(id);
        var result = await handler.Handle(query);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetUserStatistics(
        int id,
        IQueryHandler<GetUserStatisticsQuery, UserStatisticsResponse> handler)
    {
        var query = new GetUserStatisticsQuery(id);
        var result = await handler.Handle(query);

        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateUser(
        int id,
        [FromBody] UpdateUserCommand request,
        ICommandHandler<UpdateUserCommand, UpdateUserResponse> handler,
        HttpContext context)
    {
        var authenticatedUserId = int.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

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

    private static async Task<IResult> DeleteUser(
        int id,
        [FromBody] DeleteUserCommand request,
        ICommandHandler<DeleteUserCommand, DeleteUserResponse> handler,
        HttpContext context)
    {
        var authenticatedUserId = int.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var command = new DeleteUserCommand(id, authenticatedUserId, request.Password, request.ConfirmDelete);
        await handler.Handle(command);

        return Results.NoContent();
    }

    private static async Task<IResult> UpdateProfileImage(
        int id,
        [FromBody] UpdateProfileImageCommand request,
        ICommandHandler<UpdateProfileImageCommand, UpdateProfileImageResponse> handler,
        HttpContext context)
    {
        var authenticatedUserId = int.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

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

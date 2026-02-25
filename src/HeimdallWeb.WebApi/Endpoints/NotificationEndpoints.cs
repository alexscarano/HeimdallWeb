using System.Security.Claims;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.Notifications.Commands;
using HeimdallWeb.Application.Notifications.DTOs;
using HeimdallWeb.Application.Notifications.Queries;
using HeimdallWeb.Domain.Interfaces;

namespace HeimdallWeb.WebApi.Endpoints;

/// <summary>
/// Minimal API endpoints for the notifications feature (G0).
/// Allows authenticated users to retrieve, count, and mark their notifications as read.
/// </summary>
public static class NotificationEndpoints
{
    /// <summary>
    /// Maps all notification-related endpoints under <c>/api/v1/notifications</c>.
    /// </summary>
    public static RouteGroupBuilder MapNotificationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/notifications")
            .WithTags("Notifications")
            .RequireAuthorization();

        group.MapGet("", GetNotifications)
            .WithSummary("List paginated notifications for the authenticated user.");

        group.MapGet("/unread-count", GetUnreadCount)
            .WithSummary("Return the total count of unread notifications for the authenticated user.");

        group.MapPatch("/{id:int}/read", MarkRead)
            .WithSummary("Mark a single notification as read.");

        group.MapPatch("/read-all", MarkAllRead)
            .WithSummary("Mark all notifications for the authenticated user as read.");

        group.MapDelete("/clear-all", ClearAll)
            .WithSummary("Clear (delete) all notifications for the authenticated user.");

        return group;
    }

    private static async Task<IResult> GetNotifications(
        IQueryHandler<GetNotificationsQuery, IEnumerable<NotificationResponse>> handler,
        IUnitOfWork unitOfWork,
        HttpContext context,
        int page = 1,
        int pageSize = 10)
    {
        var userInternalId = await ResolveUserInternalIdAsync(context, unitOfWork);
        if (userInternalId == null)
            return Results.Unauthorized();

        var finalPage = page > 0 ? page : 1;
        var finalPageSize = pageSize > 0 && pageSize <= 100 ? pageSize : 10;

        var query = new GetNotificationsQuery(userInternalId.Value, finalPage, finalPageSize);
        var result = await handler.Handle(query);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetUnreadCount(
        IQueryHandler<GetUnreadCountQuery, int> handler,
        IUnitOfWork unitOfWork,
        HttpContext context)
    {
        var userInternalId = await ResolveUserInternalIdAsync(context, unitOfWork);
        if (userInternalId == null)
            return Results.Unauthorized();

        var count = await handler.Handle(new GetUnreadCountQuery(userInternalId.Value));

        return Results.Ok(new { count });
    }

    private static async Task<IResult> MarkRead(
        int id,
        ICommandHandler<MarkNotificationReadCommand, bool> handler,
        IUnitOfWork unitOfWork,
        HttpContext context)
    {
        var userInternalId = await ResolveUserInternalIdAsync(context, unitOfWork);
        if (userInternalId == null)
            return Results.Unauthorized();

        var found = await handler.Handle(new MarkNotificationReadCommand(id, userInternalId.Value));

        return found ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> MarkAllRead(
        ICommandHandler<MarkAllReadCommand, bool> handler,
        IUnitOfWork unitOfWork,
        HttpContext context)
    {
        var userInternalId = await ResolveUserInternalIdAsync(context, unitOfWork);
        if (userInternalId == null)
            return Results.Unauthorized();

        await handler.Handle(new MarkAllReadCommand(userInternalId.Value));

        return Results.NoContent();
    }

    private static async Task<IResult> ClearAll(
        ICommandHandler<ClearAllNotificationsCommand, bool> handler,
        IUnitOfWork unitOfWork,
        HttpContext context)
    {
        var userInternalId = await ResolveUserInternalIdAsync(context, unitOfWork);
        if (userInternalId == null)
            return Results.Unauthorized();

        await handler.Handle(new ClearAllNotificationsCommand(userInternalId.Value));

        return Results.NoContent();
    }

    /// <summary>
    /// Resolves the authenticated user's internal integer ID from the JWT PublicId (Guid) claim.
    /// Returns null if the claim is absent, malformed, or the user cannot be found.
    /// </summary>
    private static async Task<int?> ResolveUserInternalIdAsync(HttpContext context, IUnitOfWork unitOfWork)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(claim) || !Guid.TryParse(claim, out var publicId) || publicId == Guid.Empty)
            return null;

        var user = await unitOfWork.Users.GetByPublicIdAsync(publicId);
        return user?.UserId;
    }
}

using System.Security.Claims;
using HeimdallWeb.Application.Commands.Admin.ToggleUserStatus;
using HeimdallWeb.Application.Commands.Admin.DeleteUserByAdmin;
using HeimdallWeb.Application.Queries.Admin.GetAdminDashboard;
using HeimdallWeb.Application.Queries.Admin.GetUsers;
using HeimdallWeb.Application.DTOs.Admin;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HeimdallWeb.WebApi.Endpoints;

public static class DashboardEndpoints
{
    public static RouteGroupBuilder MapDashboardEndpoints(this WebApplication app)
    {
        var dashboardGroup = app.MapGroup("/api/v1/dashboard")
            .WithTags("Dashboard")
            .RequireAuthorization();

        var adminGroup = app.MapGroup("/api/v1/admin")
            .WithTags("Admin")
            .RequireAuthorization();

        // Dashboard endpoints
        dashboardGroup.MapGet("/admin", GetAdminDashboard);
        dashboardGroup.MapGet("/users", GetUsers);

        // Admin commands
        adminGroup.MapPatch("/users/{id:guid}/status", ToggleUserStatus);
        adminGroup.MapDelete("/users/{id:guid}", DeleteUserByAdmin);

        return dashboardGroup;
    }

    private static async Task<IResult> GetAdminDashboard(
        [FromQuery] int logPage,
        [FromQuery] int logPageSize,
        [FromQuery] string? logLevel,
        [FromQuery] DateTime? logStartDate,
        [FromQuery] DateTime? logEndDate,
        IQueryHandler<GetAdminDashboardQuery, AdminDashboardResponse> handler,
        HttpContext context)
    {
        var userId = Guid.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        // Default pagination values
        if (logPage <= 0) logPage = 1;
        if (logPageSize <= 0 || logPageSize > 50) logPageSize = 10;

        var query = new GetAdminDashboardQuery(
            userId,
            logPage,
            logPageSize,
            logLevel,
            logStartDate,
            logEndDate
        );

        var result = await handler.Handle(query);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetUsers(
        [FromQuery] string? search,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromQuery] bool? isActive,
        [FromQuery] bool? isAdmin,
        [FromQuery] DateTime? createdFrom,
        [FromQuery] DateTime? createdTo,
        IQueryHandler<GetUsersQuery, PaginatedUsersResponse> handler,
        HttpContext context)
    {
        var adminUserId = Guid.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        // Default pagination values
        if (page <= 0) page = 1;
        if (pageSize <= 0 || pageSize > 100) pageSize = 10;

        var query = new GetUsersQuery(
            adminUserId,
            page,
            pageSize,
            search,
            isActive,
            isAdmin,
            createdFrom,
            createdTo
        );

        var result = await handler.Handle(query);

        return Results.Ok(result);
    }

    private static async Task<IResult> ToggleUserStatus(
        Guid id,
        [FromBody] ToggleUserStatusCommand request,
        ICommandHandler<ToggleUserStatusCommand, ToggleUserStatusResponse> handler,
        HttpContext context)
    {
        // Get UserType from Role claim
        var userTypeString = context.User.FindFirst(ClaimTypes.Role)?.Value ?? "1";
        var userType = (UserType)int.Parse(userTypeString);

        var command = new ToggleUserStatusCommand(id, request.IsActive, userType);
        var result = await handler.Handle(command);

        return Results.Ok(result);
    }

    private static async Task<IResult> DeleteUserByAdmin(
        Guid id,
        ICommandHandler<DeleteUserByAdminCommand, DeleteUserByAdminResponse> handler,
        HttpContext context)
    {
        var adminUserId = Guid.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var userTypeString = context.User.FindFirst(ClaimTypes.Role)?.Value ?? "1";
        var userType = (UserType)int.Parse(userTypeString);

        var command = new DeleteUserByAdminCommand(id, userType, adminUserId);
        await handler.Handle(command);

        return Results.NoContent();
    }
}

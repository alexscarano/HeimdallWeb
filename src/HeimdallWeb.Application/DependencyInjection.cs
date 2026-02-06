using System.Reflection;
using FluentValidation;
using HeimdallWeb.Application.Commands.Admin.DeleteUserByAdmin;
using HeimdallWeb.Application.Commands.Admin.ToggleUserStatus;
using HeimdallWeb.Application.Commands.Auth.Login;
using HeimdallWeb.Application.Commands.Auth.Register;
using HeimdallWeb.Application.Commands.Scan.DeleteScanHistory;
using HeimdallWeb.Application.Commands.Scan.ExecuteScan;
using HeimdallWeb.Application.Commands.User.DeleteUser;
using HeimdallWeb.Application.Commands.User.UpdateProfileImage;
using HeimdallWeb.Application.Commands.User.UpdateUser;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Admin;
using HeimdallWeb.Application.DTOs.Auth;
using HeimdallWeb.Application.DTOs.Scan;
using HeimdallWeb.Application.DTOs.User;
using HeimdallWeb.Application.Helpers;
using HeimdallWeb.Application.Interfaces;
using HeimdallWeb.Application.Queries.Admin.GetAdminDashboard;
using HeimdallWeb.Application.Queries.Admin.GetUsers;
using HeimdallWeb.Application.Queries.Scan.ExportHistoryPdf;
using HeimdallWeb.Application.Queries.Scan.ExportSingleHistoryPdf;
using HeimdallWeb.Application.Queries.Scan.GetFindingsByHistoryId;
using HeimdallWeb.Application.Queries.Scan.GetScanHistoryById;
using HeimdallWeb.Application.Queries.Scan.GetTechnologiesByHistoryId;
using HeimdallWeb.Application.Queries.Scan.GetUserScanHistories;
using HeimdallWeb.Application.Queries.User.GetUserProfile;
using HeimdallWeb.Application.Queries.User.GetUserStatistics;
using HeimdallWeb.Application.Services;
using HeimdallWeb.Application.Services.AI;
using Microsoft.Extensions.DependencyInjection;

namespace HeimdallWeb.Application;

/// <summary>
/// Extension methods for registering Application layer services.
/// Called from WebAPI Program.cs: builder.Services.AddApplication()
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Application layer dependencies:
    /// - FluentValidation validators (9 validators)
    /// - Command handlers (9 handlers)
    /// - Query handlers (10 handlers)
    /// - Application services (4 services: Scanner, Gemini, Pdf, Token)
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // ===== FluentValidation =====
        // Automatically discovers and registers all IValidator<T> implementations
        // from this assembly (9 validators for commands)
        services.AddValidatorsFromAssembly(assembly);

        // ===== Application Services =====
        // Core services used by handlers
        services.AddScoped<IScannerService, ScannerService>();
        services.AddScoped<IGeminiService, GeminiService>();
        services.AddScoped<IPdfService, PdfService>();
        // TokenService is static - no registration needed

        // ===== Command Handlers (9 total) =====

        // Auth Commands (2)
        services.AddScoped<ICommandHandler<LoginCommand, LoginResponse>, LoginCommandHandler>();
        services.AddScoped<ICommandHandler<RegisterUserCommand, RegisterUserResponse>, RegisterUserCommandHandler>();

        // Scan Commands (2)
        services.AddScoped<ICommandHandler<ExecuteScanCommand, ExecuteScanResponse>, ExecuteScanCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteScanHistoryCommand, DeleteScanHistoryResponse>, DeleteScanHistoryCommandHandler>();

        // User Commands (3)
        services.AddScoped<ICommandHandler<UpdateUserCommand, UpdateUserResponse>, UpdateUserCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteUserCommand, DeleteUserResponse>, DeleteUserCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateProfileImageCommand, UpdateProfileImageResponse>, UpdateProfileImageCommandHandler>();

        // Admin Commands (2)
        services.AddScoped<ICommandHandler<ToggleUserStatusCommand, ToggleUserStatusResponse>, ToggleUserStatusCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteUserByAdminCommand, DeleteUserByAdminResponse>, DeleteUserByAdminCommandHandler>();

        // ===== Query Handlers (10 total) =====

        // Scan Queries (6)
        services.AddScoped<IQueryHandler<GetScanHistoryByIdQuery, ScanHistoryDetailResponse>, GetScanHistoryByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetUserScanHistoriesQuery, PaginatedScanHistoriesResponse>, GetUserScanHistoriesQueryHandler>();
        services.AddScoped<IQueryHandler<GetFindingsByHistoryIdQuery, IEnumerable<FindingResponse>>, GetFindingsByHistoryIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetTechnologiesByHistoryIdQuery, IEnumerable<TechnologyResponse>>, GetTechnologiesByHistoryIdQueryHandler>();
        services.AddScoped<IQueryHandler<ExportHistoryPdfQuery, PdfExportResponse>, ExportHistoryPdfQueryHandler>();
        services.AddScoped<IQueryHandler<ExportSingleHistoryPdfQuery, PdfExportResponse>, ExportSingleHistoryPdfQueryHandler>();

        // User Queries (2)
        services.AddScoped<IQueryHandler<GetUserProfileQuery, UserProfileResponse>, GetUserProfileQueryHandler>();
        services.AddScoped<IQueryHandler<GetUserStatisticsQuery, UserStatisticsResponse>, GetUserStatisticsQueryHandler>();

        // Admin Queries (2)
        services.AddScoped<IQueryHandler<GetAdminDashboardQuery, AdminDashboardResponse>, GetAdminDashboardQueryHandler>();
        services.AddScoped<IQueryHandler<GetUsersQuery, PaginatedUsersResponse>, GetUsersQueryHandler>();

        return services;
    }
}

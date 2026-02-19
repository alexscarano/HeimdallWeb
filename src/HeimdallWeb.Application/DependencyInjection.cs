using System.Reflection;
using FluentValidation;
using HeimdallWeb.Application.Commands.Admin.DeleteUserByAdmin;
using HeimdallWeb.Application.Commands.Admin.ToggleUserStatus;
using HeimdallWeb.Application.Commands.Auth.ForgotPassword;
using HeimdallWeb.Application.Commands.Auth.GoogleAuth;
using HeimdallWeb.Application.Commands.Auth.Login;
using HeimdallWeb.Application.Commands.Auth.Register;
using HeimdallWeb.Application.Commands.Auth.ResetPassword;
using HeimdallWeb.Application.Commands.Support.SendContact;
using HeimdallWeb.Application.Commands.Monitor.CreateMonitor;
using HeimdallWeb.Application.Commands.Monitor.DeleteMonitor;
using HeimdallWeb.Application.Commands.Scan.DeleteScanHistory;
using HeimdallWeb.Application.Commands.Scan.ExecuteScan;
using HeimdallWeb.Application.Commands.User.DeleteUser;
using HeimdallWeb.Application.Commands.User.UpdatePassword;
using HeimdallWeb.Application.Commands.User.UpdateProfileImage;
using HeimdallWeb.Application.Commands.User.UpdateUser;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Admin;
using HeimdallWeb.Application.DTOs.Auth;
using HeimdallWeb.Application.DTOs.Monitor;
using HeimdallWeb.Application.DTOs.Scan;
using HeimdallWeb.Application.DTOs.User;
using HeimdallWeb.Application.Helpers;
using HeimdallWeb.Application.Interfaces;
using HeimdallWeb.Application.Queries.Admin.GetAdminDashboard;
using HeimdallWeb.Application.Queries.Admin.GetUsers;
using HeimdallWeb.Application.Queries.Monitor.GetMonitorHistory;
using HeimdallWeb.Application.Queries.Monitor.GetUserMonitors;
using HeimdallWeb.Application.Queries.Scan.ExportHistoryPdf;
using HeimdallWeb.Application.Queries.Scan.ExportSingleHistoryPdf;
using HeimdallWeb.Application.Queries.Scan.GetAISummaryByHistoryId;
using HeimdallWeb.Application.Queries.Scan.GetFindingsByHistoryId;
using HeimdallWeb.Application.Queries.Scan.GetScanHistoryById;
using HeimdallWeb.Application.Queries.Scan.GetTechnologiesByHistoryId;
using HeimdallWeb.Application.Queries.Scan.GetScanProfiles;
using HeimdallWeb.Application.Queries.Scan.GetUserScanHistories;
using HeimdallWeb.Application.Queries.User.GetUserProfile;
using HeimdallWeb.Application.Queries.User.GetUserStatistics;
using HeimdallWeb.Application.Services;
using HeimdallWeb.Application.Services.AI;
using HeimdallWeb.Application.Workers;
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

        // ===== Memory Cache =====
        // Required by ScoreCalculatorService for caching RiskWeights (10-min TTL).
        // AddMemoryCache is idempotent — safe to call even if already registered.
        services.AddMemoryCache();

        // ===== FluentValidation =====
        // Automatically discovers and registers all IValidator<T> implementations
        // from this assembly (9 validators for commands)
        services.AddValidatorsFromAssembly(assembly);

        // ===== Application Services =====
        // Core services used by handlers
        services.AddScoped<IScannerService, ScannerService>();
        services.AddScoped<IGeminiService, GeminiService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IScoreCalculatorService, ScoreCalculatorService>();
        // TokenService is static - no registration needed

        // Sprint 4: Monitoring & Cache services
        services.AddScoped<IScanCacheService, ScanCacheService>();
        services.AddScoped<IRiskDeltaService, RiskDeltaService>();

        // Sprint 4: Background service
        services.AddHostedService<MonitoringWorker>();

        // ===== Command Handlers (9 total) =====

        // Auth Commands (5 — Sprint 5 adds ForgotPassword, ResetPassword, GoogleAuth)
        services.AddScoped<ICommandHandler<LoginCommand, LoginResponse>, LoginCommandHandler>();
        services.AddScoped<ICommandHandler<RegisterUserCommand, RegisterUserResponse>, RegisterUserCommandHandler>();
        services.AddScoped<ICommandHandler<ForgotPasswordCommand, ForgotPasswordResponse>, ForgotPasswordCommandHandler>();
        services.AddScoped<ICommandHandler<ResetPasswordCommand, ResetPasswordResponse>, ResetPasswordCommandHandler>();
        services.AddScoped<ICommandHandler<GoogleAuthCommand, LoginResponse>, GoogleAuthCommandHandler>();

        // Support Commands (1 — Sprint 5)
        services.AddScoped<ICommandHandler<SendContactCommand, SendContactResponse>, SendContactCommandHandler>();

        // Scan Commands (2)
        services.AddScoped<ICommandHandler<ExecuteScanCommand, ExecuteScanResponse>, ExecuteScanCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteScanHistoryCommand, DeleteScanHistoryResponse>, DeleteScanHistoryCommandHandler>();

        // User Commands (3)
        services.AddScoped<ICommandHandler<UpdateUserCommand, UpdateUserResponse>, UpdateUserCommandHandler>();
        services.AddScoped<ICommandHandler<UpdatePasswordCommand, UpdatePasswordResponse>, UpdatePasswordCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteUserCommand, DeleteUserResponse>, DeleteUserCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateProfileImageCommand, UpdateProfileImageResponse>, UpdateProfileImageCommandHandler>();

        // Admin Commands (2)
        services.AddScoped<ICommandHandler<ToggleUserStatusCommand, ToggleUserStatusResponse>, ToggleUserStatusCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteUserByAdminCommand, DeleteUserByAdminResponse>, DeleteUserByAdminCommandHandler>();

        // Monitor Commands - Sprint 4 (2)
        services.AddScoped<ICommandHandler<CreateMonitorCommand, MonitoredTargetResponse>, CreateMonitorCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteMonitorCommand, bool>, DeleteMonitorCommandHandler>();

        // ===== Query Handlers (10 total) =====

        // Scan Queries (8)
        services.AddScoped<IQueryHandler<GetScanProfilesQuery, IEnumerable<ScanProfileResponse>>, GetScanProfilesQueryHandler>();
        services.AddScoped<IQueryHandler<GetScanHistoryByIdQuery, ScanHistoryDetailResponse>, GetScanHistoryByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetUserScanHistoriesQuery, PaginatedScanHistoriesResponse>, GetUserScanHistoriesQueryHandler>();
        services.AddScoped<IQueryHandler<GetFindingsByHistoryIdQuery, IEnumerable<FindingResponse>>, GetFindingsByHistoryIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetTechnologiesByHistoryIdQuery, IEnumerable<TechnologyResponse>>, GetTechnologiesByHistoryIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetAISummaryByHistoryIdQuery, IASummaryResponse?>, GetAISummaryByHistoryIdQueryHandler>();
        services.AddScoped<IQueryHandler<ExportHistoryPdfQuery, PdfExportResponse>, ExportHistoryPdfQueryHandler>();
        services.AddScoped<IQueryHandler<ExportSingleHistoryPdfQuery, PdfExportResponse>, ExportSingleHistoryPdfQueryHandler>();

        // User Queries (2)
        services.AddScoped<IQueryHandler<GetUserProfileQuery, UserProfileResponse>, GetUserProfileQueryHandler>();
        services.AddScoped<IQueryHandler<GetUserStatisticsQuery, UserStatisticsResponse>, GetUserStatisticsQueryHandler>();

        // Admin Queries (2)
        services.AddScoped<IQueryHandler<GetAdminDashboardQuery, AdminDashboardResponse>, GetAdminDashboardQueryHandler>();
        services.AddScoped<IQueryHandler<GetUsersQuery, PaginatedUsersResponse>, GetUsersQueryHandler>();

        // Monitor Queries - Sprint 4 (2)
        services.AddScoped<IQueryHandler<GetUserMonitorsQuery, IEnumerable<MonitoredTargetResponse>>, GetUserMonitorsQueryHandler>();
        services.AddScoped<IQueryHandler<GetMonitorHistoryQuery, IEnumerable<RiskSnapshotResponse>>, GetMonitorHistoryQueryHandler>();

        return services;
    }
}

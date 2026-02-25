using System.Security.Claims;
using HeimdallWeb.Application.Commands.Auth.ForgotPassword;
using HeimdallWeb.Application.Commands.Auth.GoogleAuth;
using HeimdallWeb.Application.Commands.Auth.Login;
using HeimdallWeb.Application.Commands.Auth.Register;
using HeimdallWeb.Application.Commands.Auth.ResetPassword;
using HeimdallWeb.Application.DTOs.Auth;
using HeimdallWeb.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HeimdallWeb.WebApi.Endpoints;

public static class AuthenticationEndpoints
{
    public static RouteGroupBuilder MapAuthenticationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Authentication");

        group.MapPost("/login", Login)
            .AllowAnonymous()
            .RequireRateLimiting("AuthPolicy");

        group.MapPost("/register", Register)
            .AllowAnonymous()
            .RequireRateLimiting("AuthPolicy");

        group.MapPost("/logout", Logout)
            .RequireAuthorization();

        // Sprint 5: Password reset flow
        group.MapPost("/forgot-password", ForgotPassword)
            .AllowAnonymous()
            .RequireRateLimiting("AuthPolicy");

        group.MapPost("/reset-password", ResetPassword)
            .AllowAnonymous()
            .RequireRateLimiting("AuthPolicy");

        // Sprint 5: Google OAuth
        group.MapPost("/google", GoogleAuth)
            .AllowAnonymous()
            .RequireRateLimiting("AuthPolicy");

        return group;
    }

    private static async Task<IResult> Login(
        [FromBody] LoginRequest request,
        ICommandHandler<LoginCommand, LoginResponse> handler,
        HttpContext context)
    {
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var command = new LoginCommand(
            request.EmailOrLogin,
            request.Password,
            remoteIp
        );

        var result = await handler.Handle(command);

        // Set JWT token in HttpOnly cookie (following old CookiesHelper pattern)
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // HTTPS only in production
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(24)
        };

        context.Response.Cookies.Append("authHeimdallCookie", result.Token, cookieOptions);

        return Results.Ok(result);
    }

    private static async Task<IResult> Register(
        [FromBody] RegisterUserRequest request,
        ICommandHandler<RegisterUserCommand, RegisterUserResponse> handler,
        HttpContext context)
    {
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var command = new RegisterUserCommand(
            request.Email,
            request.Username,
            request.Password,
            remoteIp
        );

        var result = await handler.Handle(command);

        // Return 201 Created with location header
        return Results.Created($"/api/v1/users/{result.UserId}/profile", result);
    }

    private static IResult Logout(HttpContext context)
    {
        // Delete authentication cookie
        context.Response.Cookies.Delete("authHeimdallCookie");

        return Results.NoContent();
    }

    /// <summary>
    /// Sprint 5: Initiates the forgot-password flow.
    /// Always returns 200 OK with a neutral message to prevent email enumeration.
    /// </summary>
    private static async Task<IResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        ICommandHandler<ForgotPasswordCommand, ForgotPasswordResponse> handler,
        HttpContext context)
    {
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var command = new ForgotPasswordCommand(
            Email: request.Email,
            RemoteIp: remoteIp
        );

        var result = await handler.Handle(command);

        return Results.Ok(result);
    }

    /// <summary>
    /// Sprint 5: Completes the password reset using a valid, unexpired token.
    /// </summary>
    private static async Task<IResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        ICommandHandler<ResetPasswordCommand, ResetPasswordResponse> handler,
        HttpContext context)
    {
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var command = new ResetPasswordCommand(
            Token: request.Token,
            NewPassword: request.NewPassword,
            RemoteIp: remoteIp
        );

        var result = await handler.Handle(command);

        return Results.Ok(result);
    }

    /// <summary>
    /// Sprint 5: Authenticates or registers a user via Google OAuth id_token.
    /// Sets the same authHeimdallCookie as the standard login endpoint.
    /// </summary>
    private static async Task<IResult> GoogleAuth(
        [FromBody] GoogleAuthRequest request,
        ICommandHandler<GoogleAuthCommand, LoginResponse> handler,
        HttpContext context)
    {
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var command = new GoogleAuthCommand(
            IdToken: request.IdToken,
            RemoteIp: remoteIp
        );

        var result = await handler.Handle(command);

        // Set JWT cookie — same as standard login
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(24)
        };

        context.Response.Cookies.Append("authHeimdallCookie", result.Token, cookieOptions);

        return Results.Ok(result);
    }
}

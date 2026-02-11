using System.Security.Claims;
using HeimdallWeb.Application.Commands.Auth.Login;
using HeimdallWeb.Application.Commands.Auth.Register;
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
}

using HeimdallWeb.Application.Commands.Support.SendContact;
using HeimdallWeb.Application.Common.Interfaces;
using HeimdallWeb.Application.DTOs.Support;
using Microsoft.AspNetCore.Mvc;

namespace HeimdallWeb.WebApi.Endpoints;

/// <summary>
/// Sprint 5: Support endpoints for the contact form.
/// </summary>
public static class SupportEndpoints
{
    public static RouteGroupBuilder MapSupportEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/support")
            .WithTags("Support");

        group.MapPost("/contact", SendContact)
            .AllowAnonymous()
            .RequireRateLimiting("AuthPolicy");

        return group;
    }

    /// <summary>
    /// Forwards a support contact form submission to the configured support email.
    /// Anonymous endpoint — no authentication required.
    /// Rate-limited via AuthPolicy to prevent abuse.
    /// </summary>
    private static async Task<IResult> SendContact(
        [FromBody] SendContactRequest request,
        ICommandHandler<SendContactCommand, SendContactResponse> handler,
        HttpContext context)
    {
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var command = new SendContactCommand(
            Name: request.Name,
            Email: request.Email,
            Subject: request.Subject,
            Message: request.Message,
            RemoteIp: remoteIp
        );

        var result = await handler.Handle(command);

        return Results.Ok(result);
    }
}

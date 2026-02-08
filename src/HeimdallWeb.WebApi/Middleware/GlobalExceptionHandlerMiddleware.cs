using System.Net;
using System.Text.Json;
using FluentValidation;
using HeimdallWeb.Application.Common.Exceptions;

namespace HeimdallWeb.WebApi.Middleware;

/// <summary>
/// Global exception handling middleware that catches exceptions and returns appropriate HTTP responses.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse();

        switch (exception)
        {
            // FluentValidation ValidationException
            case FluentValidation.ValidationException validationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.StatusCode = response.StatusCode;
                errorResponse.Message = "One or more validation errors occurred.";
                errorResponse.Errors = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                _logger.LogWarning(validationException, "Validation failed: {Errors}", 
                    JsonSerializer.Serialize(errorResponse.Errors));
                break;

            // Custom Application ValidationException
            case HeimdallWeb.Application.Common.Exceptions.ValidationException appValidationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.StatusCode = response.StatusCode;
                errorResponse.Message = appValidationException.Message;
                _logger.LogWarning(appValidationException, "Application validation failed: {Message}", 
                    appValidationException.Message);
                break;

            // Custom NotFoundException
            case NotFoundException notFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.StatusCode = response.StatusCode;
                errorResponse.Message = notFoundException.Message;
                _logger.LogWarning(notFoundException, "Resource not found: {Message}", 
                    notFoundException.Message);
                break;

            // Custom UnauthorizedException
            case UnauthorizedException unauthorizedException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.StatusCode = response.StatusCode;
                errorResponse.Message = unauthorizedException.Message;
                _logger.LogWarning(unauthorizedException, "Unauthorized access: {Message}", 
                    unauthorizedException.Message);
                break;

            // Custom ForbiddenException
            case ForbiddenException forbiddenException:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                errorResponse.StatusCode = response.StatusCode;
                errorResponse.Message = forbiddenException.Message;
                _logger.LogWarning(forbiddenException, "Forbidden access: {Message}", 
                    forbiddenException.Message);
                break;

            // Custom ConflictException (e.g., duplicate email)
            case ConflictException conflictException:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                errorResponse.StatusCode = response.StatusCode;
                errorResponse.Message = conflictException.Message;
                _logger.LogWarning(conflictException, "Conflict: {Message}", 
                    conflictException.Message);
                break;

            // Generic exceptions (500 Internal Server Error)
            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.StatusCode = response.StatusCode;
                errorResponse.Message = "An unexpected error occurred. Please try again later.";
                _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(jsonResponse);
    }
}

/// <summary>
/// Standardized error response model.
/// </summary>
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string[]>? Errors { get; set; }
}

/// <summary>
/// Extension method to register the global exception handler middleware.
/// </summary>
public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}

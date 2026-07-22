using FluentValidation;
using System.Net;
using System.Text.Json;

namespace AIAgent.API.Middleware;

/// <summary>Converts exceptions into consistent API responses and logs unexpected failures.</summary>
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>Handles validation and system errors without leaking internal details.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = "Validation failed", details = ex.Errors.Select(e => e.ErrorMessage) }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in AIAgentService");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = "Unexpected server error" }));
        }
    }
}

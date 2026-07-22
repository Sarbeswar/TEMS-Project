namespace AIAgent.API.Middleware;

/// <summary>Ensures every request has a correlation id for Serilog, Splunk, Kafka, and downstream services.</summary>
public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    /// <summary>Reads or creates correlation id and returns it to callers for traceability.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault() ?? Guid.NewGuid().ToString("N");
        context.Items[HeaderName] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;
        await _next(context);
    }
}

using MediatR;
using Microsoft.Extensions.Logging;

namespace AIAgent.Application.Behaviors;

/// <summary>MediatR behavior that logs request start/end for Splunk-friendly traceability.</summary>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) => _logger = logger;

    /// <summary>Records request execution without polluting individual handlers.</summary>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling MediatR request {RequestName}", typeof(TRequest).Name);
        var response = await next();
        _logger.LogInformation("Handled MediatR request {RequestName}", typeof(TRequest).Name);
        return response;
    }
}

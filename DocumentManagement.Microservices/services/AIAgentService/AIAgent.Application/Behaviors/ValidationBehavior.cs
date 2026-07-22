using FluentValidation;
using MediatR;

namespace AIAgent.Application.Behaviors;

/// <summary>MediatR pipeline behavior that centralizes request validation for every command/query.</summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

    /// <summary>Runs validators before the handler so business handlers stay focused on use cases.</summary>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var failures = _validators
            .Select(validator => validator.Validate(request))
            .SelectMany(result => result.Errors)
            .Where(error => error is not null)
            .ToList();

        if (failures.Count != 0) throw new ValidationException(failures);
        return await next();
    }
}

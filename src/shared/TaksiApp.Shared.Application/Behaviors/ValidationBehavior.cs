using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using TaksiApp.Shared.Kernel.Results;

namespace TaksiApp.Shared.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that validates commands and queries using FluentValidation.
/// </summary>
/// <remarks>
/// <para>
/// This behavior executes all registered <see cref="IValidator{T}"/> instances
/// for the incoming request.
/// </para>
/// <para>
/// Validation failures are converted into <see cref="Result.Failure"/> instances
/// and returned to the caller without throwing exceptions.
/// </para>
/// <para>
/// This enforces a consistent, exception-free validation model aligned with
/// the Result pattern.
/// </para>
/// </remarks>
/// <typeparam name="TRequest">The type of the request being validated.</typeparam>
/// <typeparam name="TResponse">The type of the response expected from the handler.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="validators">The validators associated with the request type.</param>
    /// <param name="logger">The logger used to record validation failures.</param>
    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    /// <summary>
    /// Executes validation logic before invoking the next pipeline behavior or handler.
    /// </summary>
    /// <param name="request">The incoming command or query.</param>
    /// <param name="next">The next delegate in the MediatR pipeline.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A successful response if validation passes; otherwise,
    /// a <see cref="Result.Failure"/> containing validation details.
    /// </returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var requestName = typeof(TRequest).Name;
        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        _logger.LogWarning(
            "Validation failed for {RequestName} with {FailureCount} errors: {Errors}",
            requestName,
            failures.Count,
            string.Join("; ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}")));

        return CreateValidationResult(failures);
    }

    private TResponse CreateValidationResult(
        List<FluentValidation.Results.ValidationFailure> failures)
    {
        var firstFailure = failures.First();

        var metadata = new Dictionary<string, string>
        {
            ["property"] = firstFailure.PropertyName,
            ["attemptedValue"] = firstFailure.AttemptedValue?.ToString() ?? "null",
            ["errorCount"] = failures.Count.ToString()
        };

        var error = Error.Validation(
            $"Validation.{firstFailure.PropertyName}",
            firstFailure.ErrorMessage,
            metadata);

        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var resultType = typeof(TResponse).GetGenericArguments()[0];
            var failureMethod = typeof(Result)
                .GetMethod(nameof(Result.Failure), 1, new[] { typeof(Error) })!
                .MakeGenericMethod(resultType);

            return (TResponse)failureMethod.Invoke(null, new object[] { error })!;
        }

        if (typeof(TResponse) == typeof(Result))
            return (TResponse)(object)Result.Failure(error);

        _logger.LogCritical(
            "ValidationBehavior misused with response type {ResponseType}.",
            typeof(TResponse).FullName);

        return (TResponse)(object)Result.Failure(
            Error.Failure(
                "Validation.InvalidResponseType",
                $"ValidationBehavior cannot handle response type {typeof(TResponse).Name}"));
    }
}

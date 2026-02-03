using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TaksiApp.Shared.Application.Abstractions;
using TaksiApp.Shared.Kernel.Results;

namespace TaksiApp.Shared.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior responsible for logging the execution of commands and queries.
/// </summary>
/// <remarks>
/// <para>
/// This behavior logs:
/// <list type="bullet">
/// <item>Request type (Command / Query)</item>
/// <item>Request name</item>
/// <item>Correlation ID from <see cref="IExecutionContext"/></item>
/// <item>Execution duration</item>
/// <item>Success or failure based on the <see cref="Result"/> pattern</item>
/// </list>
/// </para>
/// <para>
/// Logging is performed using structured logging and scoped properties
/// to ensure compatibility with centralized logging and observability systems.
/// </para>
/// </remarks>
/// <typeparam name="TRequest">The type of the request being handled.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the handler.</typeparam>
public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly IExecutionContext _executionContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="logger">The logger used to write execution logs.</param>
    /// <param name="executionContext">
    /// The execution context providing correlation and request-level metadata.
    /// </param>
    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        IExecutionContext executionContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _executionContext = executionContext ?? throw new ArgumentNullException(nameof(executionContext));
    }

    /// <summary>
    /// Handles the request by logging execution start, end, duration,
    /// and success or failure status.
    /// </summary>
    /// <param name="request">The incoming command or query.</param>
    /// <param name="next">The next delegate in the MediatR pipeline.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The response from the next pipeline behavior or handler.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestType = GetRequestType(request);

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestType"] = requestType,
            ["RequestName"] = requestName,
            ["RequestId"] = _executionContext.CorrelationId,
            ["CorrelationId"] = _executionContext.CorrelationId
        });

        _logger.LogInformation(
            "Executing {RequestType} {RequestName}",
            requestType, requestName);

        var sw = Stopwatch.StartNew();

        try
        {
            var response = await next();
            sw.Stop();

            var isSuccess = IsSuccessResponse(response);
            var logLevel = isSuccess ? LogLevel.Information : LogLevel.Warning;

            _logger.Log(
                logLevel,
                "Executed {RequestType} {RequestName} in {ElapsedMs}ms - Success: {Success}",
                requestType,
                requestName,
                sw.ElapsedMilliseconds,
                isSuccess);

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();

            _logger.LogError(
                ex,
                "Error executing {RequestType} {RequestName} after {ElapsedMs}ms",
                requestType,
                requestName,
                sw.ElapsedMilliseconds);

            throw;
        }
    }

    private static string GetRequestType(TRequest request) =>
        request switch
        {
            ICommand => "Command",
            IQuery<TResponse> => "Query",
            _ => "Request"
        };

    private static bool IsSuccessResponse(TResponse response)
    {
        if (response is Result result)
            return result.IsSuccess;

        var resultProperty = response?.GetType().GetProperty("IsSuccess");
        if (resultProperty != null)
            return (bool)(resultProperty.GetValue(response) ?? false);

        return true;
    }
}

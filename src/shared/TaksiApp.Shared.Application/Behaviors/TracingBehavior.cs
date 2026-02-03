using MediatR;
using System.Diagnostics;
using TaksiApp.Shared.Application.Abstractions;
using TaksiApp.Shared.Kernel.Results;

namespace TaksiApp.Shared.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that creates OpenTelemetry-compatible tracing spans
/// for commands and queries.
/// </summary>
/// <remarks>
/// <para>
/// This behavior creates an <see cref="Activity"/> for each request, enabling
/// distributed tracing across service boundaries.
/// </para>
/// <para>
/// The activity captures:
/// <list type="bullet">
/// <item>Request type (Command / Query)</item>
/// <item>Request name</item>
/// <item>Success or failure based on the <see cref="Result"/> pattern</item>
/// </list>
/// </para>
/// </remarks>
/// <typeparam name="TRequest">The type of the request being handled.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the handler.</typeparam>
public sealed class TracingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ActivitySource _activitySource;

    /// <summary>
    /// Initializes a new instance of the <see cref="TracingBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="activitySource">
    /// The <see cref="ActivitySource"/> used to create tracing spans.
    /// </param>
    public TracingBehavior(ActivitySource activitySource)
    {
        _activitySource = activitySource;
    }

    /// <summary>
    /// Handles the request by creating a tracing span and recording execution outcome.
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
        var requestType = request is ICommand ? "Command" : "Query";

        using var activity = _activitySource.StartActivity(
            $"{requestType}.{requestName}",
            ActivityKind.Internal);

        activity?.SetTag("request.type", requestType);
        activity?.SetTag("request.name", requestName);

        activity?.SetBaggage("request.type", requestType);
        activity?.SetBaggage("request.name", requestName);

        try
        {
            var response = await next();

            var isSuccess = IsSuccessResponse(response);
            activity?.SetTag("result.success", isSuccess);
            activity?.SetStatus(
                isSuccess ? ActivityStatusCode.Ok : ActivityStatusCode.Error);

            return response;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

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

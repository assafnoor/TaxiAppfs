using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TaksiApp.Gateway.Core.Configuration;

namespace TaksiApp.Gateway.Api.Middleware;

/// <summary>
/// Middleware that enforces a maximum execution time for incoming gateway requests.
/// If the timeout is exceeded, a 504 Gateway Timeout response is returned.
/// </summary>
public sealed class RequestTimeoutMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IOptionsMonitor<GatewayOptions> _options;

    /// <summary>
    /// Initializes a new instance of <see cref="RequestTimeoutMiddleware"/>.
    /// </summary>
    /// <param name="next">Next middleware in the pipeline</param>
    /// <param name="options">Gateway configuration options</param>
    public RequestTimeoutMiddleware(
        RequestDelegate next,
        IOptionsMonitor<GatewayOptions> options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Invokes the middleware and cancels the request if the configured timeout is exceeded.
    /// </summary>
    /// <param name="context">HTTP context</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var timeoutSeconds = _options.CurrentValue.DefaultTimeoutSeconds;

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
        cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        try
        {
            context.RequestAborted = cts.Token;
            await _next(context);
        }
        catch (OperationCanceledException)
            when (cts.IsCancellationRequested && !context.RequestAborted.IsCancellationRequested)
        {
            context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status504GatewayTimeout,
                Title = "Gateway Timeout",
                Detail = $"The request exceeded the configured timeout of {timeoutSeconds} seconds",
                Instance = context.Request.Path,
                Type = "https://httpstatuses.com/504"
            };

            problemDetails.Extensions["timeout"] = timeoutSeconds;
            problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}

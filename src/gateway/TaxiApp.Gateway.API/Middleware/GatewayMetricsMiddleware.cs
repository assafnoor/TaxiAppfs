using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using TaksiApp.Shared.Application.Abstractions;

namespace TaksiApp.Gateway.Api.Middleware;

/// <summary>
/// Middleware for gateway-specific metrics and monitoring
/// </summary>
public sealed class GatewayMetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Counter<long> _requestCounter;
    private readonly Histogram<double> _requestDuration;
    private readonly IExecutionContext _executionContext;

    public GatewayMetricsMiddleware(
        RequestDelegate next,
        Meter meter,
        IExecutionContext executionContext)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _executionContext = executionContext ?? throw new ArgumentNullException(nameof(executionContext));

        _requestCounter = meter.CreateCounter<long>(
            "gateway.requests.count",
            unit: "requests",
            description: "Total number of requests through gateway");

        _requestDuration = meter.CreateHistogram<double>(
            "gateway.requests.duration",
            unit: "ms",
            description: "Request duration in milliseconds");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var path = context.Request.Path.Value ?? "/";
        var method = context.Request.Method;

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();

            var tags = new TagList
            {
                { "method", method },
                { "route", path },
                { "status_code", context.Response.StatusCode },
                { "correlation_id", _executionContext.CorrelationId }
            };

            _requestCounter.Add(1, tags);
            _requestDuration.Record(sw.Elapsed.TotalMilliseconds, tags);
        }
    }
}

/// <summary>
/// Middleware for request timeout enforcement
/// </summary>
public sealed class RequestTimeoutMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IOptionsMonitor<GatewayOptions> _options;

    public RequestTimeoutMiddleware(
        RequestDelegate next,
        IOptionsMonitor<GatewayOptions> options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var timeoutSeconds = _options.CurrentValue.DefaultTimeoutSeconds;

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
        cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested && !context.RequestAborted.IsCancellationRequested)
        {
            context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Gateway Timeout",
                message = $"Request exceeded timeout of {timeoutSeconds} seconds",
                timestamp = DateTime.UtcNow
            });
        }
    }
}

/// <summary>
/// Extension methods for gateway middleware
/// </summary>
public static class GatewayMiddlewareExtensions
{
    public static IApplicationBuilder UseGatewayMetrics(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GatewayMetricsMiddleware>();
    }

    public static IApplicationBuilder UseRequestTimeout(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestTimeoutMiddleware>();
    }
}
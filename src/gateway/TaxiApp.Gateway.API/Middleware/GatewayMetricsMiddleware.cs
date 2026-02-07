using System.Diagnostics;
using System.Diagnostics.Metrics;
using TaksiApp.Shared.Api.Middleware;

namespace TaksiApp.Gateway.Api.Middleware;

/// <summary>
/// Middleware responsible for collecting gateway-level metrics such as
/// request count, duration, status codes, and correlation identifiers.
/// </summary>
public sealed class GatewayMetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Counter<long> _requestCounter;
    private readonly Histogram<double> _requestDuration;

    /// <summary>
    /// Initializes a new instance of <see cref="GatewayMetricsMiddleware"/>.
    /// </summary>
    /// <param name="next">Next middleware in the pipeline</param>
    /// <param name="meter">OpenTelemetry meter instance</param>
    public GatewayMetricsMiddleware(
        RequestDelegate next,
        Meter meter)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        ArgumentNullException.ThrowIfNull(meter);

        _requestCounter = meter.CreateCounter<long>(
            "gateway.requests.count",
            unit: "requests",
            description: "Total number of requests passing through the gateway");

        _requestDuration = meter.CreateHistogram<double>(
            "gateway.requests.duration",
            unit: "ms",
            description: "Gateway request processing duration in milliseconds");
    }

    /// <summary>
    /// Records metrics for each incoming request.
    /// </summary>
    /// <param name="context">HTTP context</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var path = context.Request.Path.Value ?? "/";
        var method = context.Request.Method;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var executionContext = context.GetExecutionContext();

            var tags = new TagList
            {
                { "method", method },
                { "route", path },
                { "status_code", context.Response.StatusCode },
                { "correlation_id", executionContext.CorrelationId }
            };

            _requestCounter.Add(1, tags);
            _requestDuration.Record(stopwatch.Elapsed.TotalMilliseconds, tags);
        }
    }
}

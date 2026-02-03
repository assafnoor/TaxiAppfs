using TaksiApp.Shared.Observability.Tracing;

namespace TaksiApp.Shared.Observability.Logging.Enrichers;

/// <summary>
/// Enriches log events with tracing information from the current <see cref="Activity"/>.
/// </summary>
/// <remarks>
/// Adds TraceId, SpanId, and ParentSpanId (if available) to the log event properties.
/// Useful for distributed tracing and observability in microservices.
/// </remarks>
public sealed class TraceContextEnricher : ILogEventEnricher
{
    /// <inheritdoc />
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;
        if (activity == null)
            return;

        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty(
                TracingConstants.TRACE_ID_KEY,
                activity.TraceId.ToString()));

        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty(
                TracingConstants.SPAN_ID_KEY,
                activity.SpanId.ToString()));

        if (activity.ParentSpanId != default)
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty(
                    TracingConstants.PARENT_ID_KEY,
                    activity.ParentSpanId.ToString()));
        }
    }
}

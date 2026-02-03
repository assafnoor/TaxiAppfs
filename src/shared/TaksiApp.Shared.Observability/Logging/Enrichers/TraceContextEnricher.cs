
using TaksiApp.Shared.Observability.Tracing;

namespace TaksiApp.Shared.Observability.Logging.Enrichers;
// TaksiApp.Shared.Observability/Logging/Enrichers/TraceContextEnricher.cs
public sealed class TraceContextEnricher : ILogEventEnricher
{
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


namespace TaksiApp.Shared.Observability.Tracing;

/// <summary>
/// Defines constants used for distributed tracing across services.
/// These values ensure consistent trace correlation between logs,
/// metrics, and traces.
/// </summary>
public static class TracingConstants
{
    /// <summary>
    /// The name of the ActivitySource used for creating spans.
    /// </summary>
    public const string ACTIVITY_SOURCE_NAME = "TaksiApp.Services";

    /// <summary>
    /// Key used to store the trace identifier in structured logs.
    /// </summary>
    public const string TRACE_ID_KEY = "trace_id";

    /// <summary>
    /// Key used to store the span identifier in structured logs.
    /// </summary>
    public const string SPAN_ID_KEY = "span_id";

    /// <summary>
    /// Key used to store the parent span identifier in structured logs.
    /// </summary>
    public const string PARENT_ID_KEY = "parent_id";
}

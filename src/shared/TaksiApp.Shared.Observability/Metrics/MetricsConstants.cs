namespace TaksiApp.Shared.Observability.Metrics;

/// <summary>
/// Defines standard metric names used across services.
/// These constants follow OpenTelemetry semantic conventions
/// and provide consistency for metrics aggregation and dashboards.
/// </summary>
public static class MetricsConstants
{
    /// <summary>
    /// The name of the OpenTelemetry meter used by application services.
    /// </summary>
    public const string METER_NAME = "TaksiApp.Services.Metrics";

    /// <summary>
    /// Measures the duration of incoming HTTP requests.
    /// Expected unit: milliseconds.
    /// </summary>
    public const string HTTP_REQUEST_DURATION = "http.server.request.duration";

    /// <summary>
    /// Counts the total number of HTTP requests processed by the server.
    /// </summary>
    public const string HTTP_REQUEST_COUNT = "http.server.request.count";

    /// <summary>
    /// Measures the duration of database client operations.
    /// Expected unit: milliseconds.
    /// </summary>
    public const string DB_OPERATION_DURATION = "db.client.operation.duration";

    /// <summary>
    /// Measures the execution duration of application commands.
    /// Expected unit: milliseconds.
    /// </summary>
    public const string COMMAND_DURATION = "app.command.duration";

    /// <summary>
    /// Measures the execution duration of application queries.
    /// Expected unit: milliseconds.
    /// </summary>
    public const string QUERY_DURATION = "app.query.duration";
}

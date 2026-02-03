namespace TaksiApp.Shared.Extensions.Configuration;

/// <summary>
/// Configuration options for OpenTelemetry observability features.
/// </summary>
/// <remarks>
/// These options control the behavior of tracing, metrics, and logging exporters.
/// Typically loaded from appsettings.json under the "Observability" section.
/// </remarks>
public sealed class ObservabilityOptions
{
    /// <summary>
    /// Gets or sets the OTLP (OpenTelemetry Protocol) endpoint URL.
    /// </summary>
    /// <value>
    /// Default is "http://localhost:4317" for local development.
    /// In production, this should point to your observability backend (e.g., Jaeger, Grafana Tempo).
    /// </value>
    public string OtlpEndpoint { get; set; } = "http://localhost:4317";

    /// <summary>
    /// Gets or sets a value indicating whether distributed tracing is enabled.
    /// </summary>
    /// <value>Default is <c>true</c>.</value>
    public bool EnableTracing { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether metrics collection is enabled.
    /// </summary>
    /// <value>Default is <c>true</c>.</value>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether structured logging is enabled.
    /// </summary>
    /// <value>Default is <c>true</c>.</value>
    public bool EnableLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets the service version reported in telemetry.
    /// </summary>
    /// <value>Default is "1.0.0".</value>
    public string ServiceVersion { get; set; } = "1.0.0";
}
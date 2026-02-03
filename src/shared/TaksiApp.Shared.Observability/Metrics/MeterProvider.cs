namespace TaksiApp.Shared.Observability.Metrics;

/// <summary>
/// Provides access to OpenTelemetry meters for creating custom metrics.
/// </summary>
/// <remarks>
/// Register as singleton in DI container. All metrics created from this provider
/// will use the same service name and version.
/// </remarks>
public sealed class MeterProvider
{
    private readonly Meter _meter;

    /// <summary>
    /// Initializes a new instance of the <see cref="MeterProvider"/> class.
    /// </summary>
    /// <param name="serviceName">The name of the service producing metrics.</param>
    /// <param name="version">Optional service version. Defaults to "1.0.0".</param>
    public MeterProvider(string serviceName, string? version = null)
    {
        _meter = new Meter(serviceName, version ?? "1.0.0");
    }

    /// <summary>
    /// Gets the underlying <see cref="Meter"/> instance used to create metrics.
    /// </summary>
    public Meter Meter => _meter;

    /// <summary>
    /// Creates a counter metric that can be incremented by a value of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The numeric type for the counter (e.g., int, long, double).</typeparam>
    /// <param name="name">The name of the counter metric.</param>
    /// <param name="unit">Optional unit of measurement (e.g., "requests").</param>
    /// <param name="description">Optional description of the counter metric.</param>
    /// <returns>An OpenTelemetry <see cref="Counter{T}"/> instance.</returns>
    public Counter<T> CreateCounter<T>(
        string name,
        string? unit = null,
        string? description = null) where T : struct
    {
        return _meter.CreateCounter<T>(name, unit, description);
    }

    /// <summary>
    /// Creates a histogram metric that records a distribution of values of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The numeric type for the histogram (e.g., int, long, double).</typeparam>
    /// <param name="name">The name of the histogram metric.</param>
    /// <param name="unit">Optional unit of measurement (e.g., "ms" for milliseconds).</param>
    /// <param name="description">Optional description of the histogram metric.</param>
    /// <returns>An OpenTelemetry <see cref="Histogram{T}"/> instance.</returns>
    public Histogram<T> CreateHistogram<T>(
        string name,
        string? unit = null,
        string? description = null) where T : struct
    {
        return _meter.CreateHistogram<T>(name, unit, description);
    }
}

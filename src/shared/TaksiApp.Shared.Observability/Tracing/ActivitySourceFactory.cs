namespace TaksiApp.Shared.Observability.Tracing;

/// <summary>
/// Factory for creating OpenTelemetry ActivitySource instances.
/// </summary>
/// <remarks>
/// ActivitySource is used to create and manage distributed tracing spans.
/// Register the created ActivitySource as a singleton in the DI container.
/// </remarks>
public static class ActivitySourceFactory
{
    /// <summary>
    /// Creates a new ActivitySource with the specified service name and version.
    /// </summary>
    /// <param name="serviceName">The name of the service (required).</param>
    /// <param name="version">Optional version string (defaults to "1.0.0").</param>
    /// <returns>A new ActivitySource instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when serviceName is null.</exception>
    /// <remarks>
    /// The caller is responsible for disposing the ActivitySource when the application shuts down.
    /// Typically registered as a singleton in DI, which handles disposal automatically.
    /// </remarks>
    public static ActivitySource Create(string serviceName, string? version = null)
    {
        ArgumentNullException.ThrowIfNull(serviceName);
        return new ActivitySource(serviceName, version ?? "1.0.0");
    }
}
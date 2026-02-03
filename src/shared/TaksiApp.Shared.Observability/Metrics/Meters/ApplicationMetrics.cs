namespace TaksiApp.Shared.Observability.Metrics;

/// <summary>
/// Provides generic application-level metrics for errors and active requests.
/// </summary>
/// <remarks>
/// <para>
/// This class is intentionally minimal to avoid becoming a dumping ground for
/// domain-specific metrics. It provides only universally applicable metrics:
/// - Error tracking (by type and optional operation)
/// - Active request tracking
/// </para>
/// <para>
/// <strong>Design principle:</strong> Keep this class focused on generic infrastructure concerns.
/// Domain-specific metrics (commands, queries, business operations) should be created
/// in individual services using their own Meter instances.
/// </para>
/// <para>
/// Thread-safe singleton - uses Interlocked for concurrent access.
/// Register as singleton in DI container.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // DI Registration
/// services.AddSingleton(sp => new ApplicationMetrics("MyService", "1.0.0"));
/// 
/// // Usage
/// public class MyService
/// {
///     private readonly ApplicationMetrics _metrics;
///     
///     public async Task ProcessAsync()
///     {
///         using var _ = _metrics.TrackActiveRequest();
///         try
///         {
///             // Process...
///         }
///         catch (ValidationException ex)
///         {
///             _metrics.RecordError("ValidationError", "ProcessAsync");
///             throw;
///         }
///     }
/// }
/// </code>
/// </example>
public sealed class ApplicationMetrics : IDisposable
{
    private readonly Meter _meter;
    private readonly Counter<long> _errorCount;
    private readonly ObservableGauge<int> _activeRequests;

    private int _activeRequestsCount;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of ApplicationMetrics.
    /// </summary>
    /// <param name="serviceName">The name of the service (used as meter name).</param>
    /// <param name="version">Optional version of the service (defaults to "1.0.0").</param>
    public ApplicationMetrics(string serviceName, string? version = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        _meter = new Meter(serviceName, version ?? "1.0.0");

        _errorCount = _meter.CreateCounter<long>(
            "app.error.count",
            unit: "errors",
            description: "Total number of errors encountered by the application");

        _activeRequests = _meter.CreateObservableGauge(
            "app.active_requests",
            () => _activeRequestsCount,
            unit: "requests",
            description: "Number of requests currently being processed");
    }

    /// <summary>
    /// Records an error occurrence.
    /// </summary>
    /// <param name="errorType">
    /// The type or category of error (e.g., "ValidationError", "DatabaseError", "ExternalServiceError").
    /// Keep these categories consistent for effective monitoring.
    /// </param>
    /// <param name="operation">
    /// Optional operation name where the error occurred (e.g., method name, endpoint).
    /// If null, only the error type will be tagged.
    /// </param>
    /// <remarks>
    /// Error types should be consistent across the application for effective monitoring.
    /// Avoid using exception type names directly; use semantic categories instead.
    /// 
    /// Good error types: "ValidationError", "DatabaseTimeout", "PaymentGatewayFailure"
    /// Poor error types: "System.ArgumentException", "Exception", "Error"
    /// </remarks>
    /// <example>
    /// <code>
    /// _metrics.RecordError("ValidationError", "CreateOrder");
    /// _metrics.RecordError("DatabaseTimeout", "GetCustomer");
    /// _metrics.RecordError("ExternalServiceError"); // operation is optional
    /// </code>
    /// </example>
    public void RecordError(string errorType, string? operation = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorType);

        var tags = new TagList { { "error.type", errorType } };

        if (!string.IsNullOrWhiteSpace(operation))
        {
            tags.Add("operation", operation);
        }

        _errorCount.Add(1, tags);
    }

    /// <summary>
    /// Tracks an active request. Increments the active request counter and returns
    /// a disposable that decrements the counter when disposed.
    /// </summary>
    /// <returns>
    /// A disposable that decrements the active request counter when disposed.
    /// Always dispose this within a using statement or using declaration.
    /// </returns>
    /// <remarks>
    /// This method uses Interlocked operations for thread-safe increment/decrement.
    /// The returned disposable is lightweight and can be used in high-throughput scenarios.
    /// </remarks>
    /// <example>
    /// <code>
    /// public async Task HandleRequestAsync()
    /// {
    ///     using var _ = _metrics.TrackActiveRequest();
    ///     
    ///     // Request processing...
    ///     // Counter is automatically decremented when method exits
    /// }
    /// </code>
    /// </example>
    public IDisposable TrackActiveRequest()
    {
        Interlocked.Increment(ref _activeRequestsCount);
        return new DisposableAction(() => Interlocked.Decrement(ref _activeRequestsCount));
    }

    /// <summary>
    /// Disposes the underlying Meter.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _meter.Dispose();
        _disposed = true;
    }

    /// <summary>
    /// Lightweight disposable wrapper for executing an action on disposal.
    /// </summary>
    private sealed class DisposableAction : IDisposable
    {
        private readonly Action _action;
        private int _disposed;

        public DisposableAction(Action action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void Dispose()
        {
            // Ensure action is only called once even if Dispose is called multiple times
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                _action();
            }
        }
    }
}